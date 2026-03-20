using FplTool.Modules.Picks.Services;

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
            var syncService = scope.ServiceProvider.GetRequiredService<IPointsSyncService>();
            await syncService.SyncPendingGameweeksAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during points sync.");
        }
    }
}
