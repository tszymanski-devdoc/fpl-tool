using FplTool.Modules.FplIntegration.Configuration;
using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.FplIntegration.Contracts.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FplTool.Modules.FplIntegration.Caching;

internal sealed class CachedFplApiService : IFplApiService
{
    private readonly IFplApiService _inner;
    private readonly IMemoryCache _cache;
    private readonly FplIntegrationOptions _options;

    public CachedFplApiService(IFplApiService inner, IMemoryCache cache, IOptions<FplIntegrationOptions> options)
    {
        _inner = inner;
        _cache = cache;
        _options = options.Value;
    }

    public Task<BootstrapStaticDto> GetBootstrapStaticAsync(CancellationToken ct = default)
        => _cache.GetOrCreateAsync(CacheKeys.BootstrapStatic, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.BootstrapCacheMinutes);
            return await _inner.GetBootstrapStaticAsync(ct);
        })!;

    public Task<ManagerPicksResponseDto> GetManagerPicksAsync(int fplManagerId, int gameweekId, CancellationToken ct = default)
        => _cache.GetOrCreateAsync(CacheKeys.ManagerPicks(fplManagerId, gameweekId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.PicksCacheMinutes);
            return await _inner.GetManagerPicksAsync(fplManagerId, gameweekId, ct);
        })!;

    public Task<LiveEventDto> GetLiveEventAsync(int gameweekId, CancellationToken ct = default)
        => _cache.GetOrCreateAsync(CacheKeys.LiveEvent(gameweekId), async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(_options.LiveCacheMinutes);
            return await _inner.GetLiveEventAsync(gameweekId, ct);
        })!;

    public Task<ManagerEntryDto> GetManagerEntryAsync(int fplManagerId, CancellationToken ct = default)
        => _cache.GetOrCreateAsync(CacheKeys.ManagerEntry(fplManagerId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _inner.GetManagerEntryAsync(fplManagerId, ct);
        })!;
}
