using FplTool.Modules.Auth.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FplTool.Modules.Auth;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FplToolDb")
                               ?? throw new InvalidOperationException("Connection string 'FplToolDb' is not configured.");

        services.AddDbContext<AuthDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0)),
                mysql => mysql.MigrationsAssembly(typeof(AuthModule).Assembly.GetName().Name)
            ));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AuthModule).Assembly));
        services.AddHttpClient();

        return services;
    }
}
