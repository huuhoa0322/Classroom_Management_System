using System.Reflection;
using System.Text;
using CLS.BLL.Interfaces;
using CLS.BLL.Mappings;
using CLS.BLL.Services;
using CLS.DAL.Data;
using CLS.DAL.Repositories;
using CLS.Server.Middlewares;
using CLS.Server.Filters;
using CLS.Server.Hubs;
using CLS.Server.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

// ── Bootstrap Serilog trước khi host build (catch startup errors) ─────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog — đọc config từ appsettings.json > "Serilog" section ─────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext());

    // ── CORS — cho phép Vite dev server gọi API ──────────────────────────────
    builder.Services.AddCors(options =>
    {
        // Dev: Mở toàn bộ origin — tránh CORS block khi debug với bất kỳ Vite port nào
        options.AddPolicy("DevCors", policy =>
            policy.SetIsOriginAllowed(_ => true)   // Chấp nhận mọi origin trong dev
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());

        options.AddPolicy("ProductionCors", policy =>
            policy.WithOrigins(
                    builder.Configuration.GetValue<string>("AllowedOrigins") ?? ""
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // ── Controllers + JSON camelCase + Global Exception Filter ─────────────────
    builder.Services.AddControllers(opts =>
        {
            opts.Filters.Add<ApiExceptionFilter>();   // ← User-code exception handler
        })
        .AddJsonOptions(opts =>
            opts.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase);

    // ── AutoMapper 16.x — scan Mapping Profiles trong CLS.BLL ────────────────
    builder.Services.AddAutoMapper(cfg =>
    {
        // License key từ env var: AutoMapper__LicenseKey
        var licenseKey = builder.Configuration["AutoMapper:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(licenseKey))
            cfg.LicenseKey = licenseKey;

        cfg.AddMaps(Assembly.Load("CLS.BLL"));
    });

    // ── FluentValidation (manual pattern — không dùng auto-pipeline) ─────────
    // Validators được inject qua IValidator<T> vào Service và gọi ValidateAsync()
    builder.Services.AddValidatorsFromAssemblies([
        typeof(Program).Assembly,   // CLS.Server
        Assembly.Load("CLS.BLL")    // CLS.BLL/Validators/
    ]);

    // ── JWT Authentication ────────────────────────────────────────────────────
    var jwtSection = builder.Configuration.GetSection("JwtSettings");
    var secretKey  = jwtSection["SecretKey"];
    if (string.IsNullOrWhiteSpace(secretKey))
        throw new InvalidOperationException(
            "JwtSettings:SecretKey is missing or empty. " +
            "Set environment variable JwtSettings__SecretKey with a value >= 32 characters.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew                = TimeSpan.Zero,
            // JsonWebTokenHandler (.NET 10) — map đúng role claim
            RoleClaimType            = System.Security.Claims.ClaimTypes.Role,
            NameClaimType            = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
        };
    });

    builder.Services.AddAuthorization();

    // ── OpenAPI — Native .NET 10 (Microsoft.AspNetCore.OpenApi) ──────────────
    // Scalar UI sẽ đọc từ /openapi/v1.json và render giao diện tại /scalar/v1
    // JWT Bearer auth được cấu hình trực tiếp trong Scalar UI (không cần AddSecurityDefinition)
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new()
            {
                Title       = "Classroom Management System API",
                Version     = "v1",
                Description = "RESTful API for CLS — ASP.NET Core 10"
            };
            return Task.CompletedTask;
        });
    });

    // ── Global Exception Handler (P4) ────────────────────────────────────────
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ── Health Checks — Docker HEALTHCHECK + Uptime Robot (ADR-001 §DevOps) ──
    builder.Services.AddHealthChecks();

    // ── JWT Service (P5) ────────────────────────────────────────────────────
    builder.Services.AddScoped<IJwtService, JwtService>();

    // ── Auth Service ──────────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();

    // ── EF Core & Database (P6) ───────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(opts =>
        opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ── Repositories (P7/P8 — Student Slice) ─────────────────────────────────
    builder.Services.AddScoped<IStudentRepository, StudentRepository>();
    builder.Services.AddScoped<IParentRepository, ParentRepository>();

    // ── Repositories (P7/P8 — Financial Administration / CLS-003) ─────────────
    builder.Services.AddScoped<ITuitionPackageRepository, TuitionPackageRepository>();
    builder.Services.AddScoped<IStudentPackageRepository, StudentPackageRepository>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

    // ── Services (P9 — Student Slice) ─────────────────────────────────────────
    builder.Services.AddScoped<IStudentService, StudentService>();

    // ── Services (P9 — Financial Administration / CLS-003) ────────────────────
    builder.Services.AddScoped<IPaymentService, PaymentService>();

    // ── Repositories (P7/P8 — Schedule Management / CLS-004+005) ──────────────
    builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    builder.Services.AddScoped<IClassRepository, ClassRepository>();
    builder.Services.AddScoped<IRoomRepository, RoomRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // ── Services (P9 — Schedule Management / CLS-004+005) ─────────────────────
    builder.Services.AddScoped<ISessionService, SessionService>();

    // ── Repositories (P7/P8 — Retention Management / CLS-006+010) ─────────────
    builder.Services.AddScoped<IRenewalAlertRepository, RenewalAlertRepository>();

    // ── Services (P9 — Retention Management / CLS-006+010) ────────────────────
    builder.Services.AddScoped<IRenewalAlertService, RenewalAlertService>();

    // ── Repositories (Academic Operations / UC-07+08) ─────────────────────────
    builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

    // ── Services (Academic Operations / UC-07+08) ─────────────────────────────
    builder.Services.AddScoped<IAttendanceService, AttendanceService>();

    // ── Repositories (Academic Quality Assurance / UC-09) ──────────────────────
    builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

    // ── Services (Academic Quality Assurance / UC-09) ──────────────────────────
    builder.Services.AddScoped<IFeedbackService, FeedbackService>();

    // ── SignalR — Real-time notifications ─────────────────────────────────────
    builder.Services.AddSignalR();

    // ── Background Service — UC-10: Daily Depletion Scan ──────────────────────
    builder.Services.AddHostedService<DepletionScanService>();

    // (AutoMapper đã đăng ký ở trên — không cần đăng ký lại)

    // ═════════════════════════════════════════════════════════════════════════
    var app = builder.Build();
    // ═════════════════════════════════════════════════════════════════════════

    app.UseDefaultFiles();
    app.MapStaticAssets();

    // ── OpenAPI + Scalar UI — Development only ────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();                           // serves /openapi/v1.json

        app.MapScalarApiReference(opts =>
        {
            opts.WithTitle("CLS API");
            opts.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            opts.AddPreferredSecuritySchemes("Bearer");     // mặc định chọn Bearer auth
        });                                         // UI tại /scalar/v1
    }

    // ── Middleware pipeline (thứ tự quan trọng) ───────────────────────────────
    app.UseSerilogRequestLogging();

    // HTTPS redirect TẮT khi chạy behind reverse proxy (Render, Nginx)
    // Render handles TLS termination at edge → app chỉ cần listen HTTP
    // Nếu self-host không qua proxy, bật lại dòng dưới:
    // app.UseHttpsRedirection();

    // CORS phải đứng TRƯỚC Authentication/Authorization
    var corsPolicy = app.Environment.IsDevelopment() ? "DevCors" : "ProductionCors";
    app.UseCors(corsPolicy);

    app.UseExceptionHandler();  // ← Framework-level handler, không bị VS break

    app.UseAuthentication();           // ← PHẢI trước UseAuthorization
    app.UseAuthorization();

    // ── Health check endpoint (no auth required) ────────────────────────────
    app.MapHealthChecks("/health");

    app.MapControllers();

    // ── SignalR Hub endpoint ──────────────────────────────────────────────────
    app.MapHub<NotificationHub>("/hubs/notifications");

    app.MapFallbackToFile("/index.html");

    Log.Information("CLS API is starting up...");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "CLS API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
