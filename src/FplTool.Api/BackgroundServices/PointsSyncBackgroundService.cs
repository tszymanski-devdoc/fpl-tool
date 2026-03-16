using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Features.SyncPoints;
using FplTool.Modules.Picks.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Api.BackgroundServices;

public sealed class PointsSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PointsSyncBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public PointsSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PointsSyncBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var intervalMinutes = configuration.GetValue<int>("BackgroundServices:PointsSyncIntervalMinutes", 15);
        _interval = TimeSpan.FromMinutes(intervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Points sync background service started. Interval: {Interval}", _interval);

        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncPointsAsync(stoppingToken);
        }
    }

    private async Task SyncPointsAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var fplService = scope.ServiceProvider.GetRequiredService<IFplApiService>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var picksDb = scope.ServiceProvider.GetRequiredService<PicksDbContext>();

            var bootstrap = await fplService.GetBootstrapStaticAsync(ct);

            var finishedGwIds = bootstrap.Events
                .Where(e => e.Finished)
                .Select(e => e.Id)
                .ToHashSet();

            var alreadySyncedGwIds = (await picksDb.GameweekPointsSyncs
                .Where(s => s.IsComplete)
                .Select(s => s.GameweekId)
                .ToListAsync(ct))
                .ToHashSet();

            var gwsToSync = finishedGwIds.Except(alreadySyncedGwIds).ToList();

            foreach (var gwId in gwsToSync)
            {
                _logger.LogInformation("Syncing points for gameweek {GameweekId}", gwId);
                await mediator.Send(new SyncGameweekPointsCommand(gwId), ct);
                _logger.LogInformation("Points synced for gameweek {GameweekId}", gwId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during points sync.");
        }
    }
}
