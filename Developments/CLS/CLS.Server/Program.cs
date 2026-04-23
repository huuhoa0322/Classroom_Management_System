using System.Reflection;
using System.Text;
using CLS.BLL.Interfaces;
using CLS.BLL.Mappings;
using CLS.BLL.Services;
using CLS.DAL.Data;
using CLS.DAL.Repositories;
using CLS.Server.Middlewares;
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

    // ── Controllers + JSON camelCase ──────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
            opts.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase);

    // ── AutoMapper 16.x — scan Mapping Profiles trong CLS.BLL ────────────────
    builder.Services.AddAutoMapper(cfg =>
        cfg.AddMaps(Assembly.Load("CLS.BLL")));

    // ── FluentValidation (manual pattern — không dùng auto-pipeline) ─────────
    // Validators được inject qua IValidator<T> vào Service và gọi ValidateAsync()
    builder.Services.AddValidatorsFromAssemblies([
        typeof(Program).Assembly,   // CLS.Server
        Assembly.Load("CLS.BLL")    // CLS.BLL/Validators/
    ]);

    // ── JWT Authentication ────────────────────────────────────────────────────
    var jwtSection = builder.Configuration.GetSection("JwtSettings");
    var secretKey  = jwtSection["SecretKey"]
        ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing in configuration.");

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

    // ── ExceptionHandlingMiddleware (P4) ────────────────────────────────────────
    builder.Services.AddTransient<ExceptionHandlingMiddleware>();

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

    // HTTPS redirect chỉ bật ở Production — dev dùng http để tránh 307 redirect phá POST request
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    // CORS phải đứng TRƯỚC Authentication/Authorization
    var corsPolicy = app.Environment.IsDevelopment() ? "DevCors" : "ProductionCors";
    app.UseCors(corsPolicy);

    app.UseMiddleware<ExceptionHandlingMiddleware>();  // ← phải trước Authentication

    app.UseAuthentication();           // ← PHẢI trước UseAuthorization
    app.UseAuthorization();

    app.MapControllers();
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
