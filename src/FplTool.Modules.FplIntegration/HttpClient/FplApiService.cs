using System.Text.Json;
using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.FplIntegration.Contracts.Dto;

namespace FplTool.Modules.FplIntegration.HttpClient;

internal sealed class FplApiService : IFplApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly FplHttpClient _client;

    public FplApiService(FplHttpClient client)
    {
        _client = client;
    }

    public async Task<BootstrapStaticDto> GetBootstrapStaticAsync(CancellationToken ct = default)
    {
        var response = await _client.Client.GetAsync("bootstrap-static/", ct);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<BootstrapStaticDto>(content, JsonOptions, ct)
               ?? throw new InvalidOperationException("Failed to deserialize bootstrap-static response.");
    }

    public async Task<ManagerPicksResponseDto> GetManagerPicksAsync(int fplManagerId, int gameweekId, CancellationToken ct = default)
    {
        var response = await _client.Client.GetAsync($"entry/{fplManagerId}/event/{gameweekId}/picks/", ct);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<ManagerPicksResponseDto>(content, JsonOptions, ct)
               ?? throw new InvalidOperationException("Failed to deserialize manager picks response.");
    }

    public async Task<LiveEventDto> GetLiveEventAsync(int gameweekId, CancellationToken ct = default)
    {
        var response = await _client.Client.GetAsync($"event/{gameweekId}/live/", ct);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<LiveEventDto>(content, JsonOptions, ct)
               ?? throw new InvalidOperationException("Failed to deserialize live event response.");
    }
}
