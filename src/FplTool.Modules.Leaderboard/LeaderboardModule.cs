using Microsoft.Extensions.DependencyInjection;

namespace FplTool.Modules.Leaderboard;

public static class LeaderboardModule
{
    public static IServiceCollection AddLeaderboardModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LeaderboardModule).Assembly));
        return services;
    }
}
