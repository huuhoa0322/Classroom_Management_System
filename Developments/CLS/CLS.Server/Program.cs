using System.Reflection;
using System.Text;
using CLS.BLL.Interfaces;
using CLS.BLL.Services;
using CLS.Server.Middlewares;
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
            ClockSkew                = TimeSpan.Zero
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

    // ── TODO P6: Register AppDbContext (EF Core + Npgsql) ────────────────────
    // builder.Services.AddDbContext<AppDbContext>(opts =>
    //     opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    app.UseHttpsRedirection();

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
