using System.Text;
using FplTool.Api.BackgroundServices;
using FplTool.Api.Infrastructure.Security;
using FplTool.Api.Middleware;
using FplTool.Modules.Auth;
using FplTool.Modules.Auth.Infrastructure;
using FplTool.Modules.FplIntegration;
using FplTool.Modules.Leaderboard;
using FplTool.Modules.Picks;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
    {
        if (ctx.HostingEnvironment.IsProduction())
            config.WriteTo.Console(new CompactJsonFormatter());
        else
            config.WriteTo.Console();

        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

    // Modules
    builder.Services.AddAuthModule(builder.Configuration);
    builder.Services.AddPicksModule(builder.Configuration);
    builder.Services.AddFplIntegration(builder.Configuration);
    builder.Services.AddLeaderboardModule();

    // JWT Authentication
    var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
                        ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "fpl-tool",
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "fpl-tool-client",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey))
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireClaim("is_admin", "true"));
    });

    // CORS
    var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // Current user context
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

    // Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "FPL Tool API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new()
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        c.AddSecurityRequirement(new()
        {
            {
                new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                []
            }
        });
    });

    // Health checks
    builder.Services.AddHealthChecks();

    // Background services
    builder.Services.AddHostedService<BootstrapCacheWarmupService>();
    builder.Services.AddHostedService<PointsSyncBackgroundService>();

    var app = builder.Build();

    // Run EF migrations at startup
    await using (var scope = app.Services.CreateAsyncScope())
    {
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var picksDb = scope.ServiceProvider.GetRequiredService<PicksDbContext>();
        await authDb.Database.MigrateAsync();
        await picksDb.Database.MigrateAsync();
    }

    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
