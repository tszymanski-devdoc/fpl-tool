using FplTool.Modules.Picks.Infrastructure;
using FplTool.Modules.Picks.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FplTool.Modules.Picks;

public static class PicksModule
{
    public static IServiceCollection AddPicksModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FplToolDb")
                               ?? throw new InvalidOperationException("Connection string 'FplToolDb' is not configured.");

        services.AddDbContext<PicksDbContext>(options =>
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0)),
                mysql => mysql.MigrationsAssembly(typeof(PicksModule).Assembly.GetName().Name)
            ));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PicksModule).Assembly));
        services.AddScoped<IPointsSyncService, PointsSyncService>();

        return services;
    }
}
