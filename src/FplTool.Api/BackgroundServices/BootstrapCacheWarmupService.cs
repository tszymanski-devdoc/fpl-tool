using FplTool.Modules.FplIntegration.Contracts;

namespace FplTool.Api.BackgroundServices;

public sealed class BootstrapCacheWarmupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BootstrapCacheWarmupService> _logger;

    public BootstrapCacheWarmupService(IServiceProvider serviceProvider, ILogger<BootstrapCacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Warming up FPL bootstrap cache...");
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var fplService = scope.ServiceProvider.GetRequiredService<IFplApiService>();
            await fplService.GetBootstrapStaticAsync(cancellationToken);
            _logger.LogInformation("FPL bootstrap cache warmed up successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to warm up FPL bootstrap cache. Will retry on next request.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
