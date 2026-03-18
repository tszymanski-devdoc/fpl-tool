using System.Net;
using FplTool.Modules.FplIntegration.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FplTool.Modules.FplIntegration.Auth;

internal sealed class FplAuthService
{
    private string? _cookieHeader;
    private DateTime _expiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly FplIntegrationOptions _options;
    private readonly ILogger<FplAuthService> _logger;

    public FplAuthService(IOptions<FplIntegrationOptions> options, ILogger<FplAuthService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Returns the Cookie header value to attach to FPL API requests.
    /// Returns null if no credentials are configured.
    /// </summary>
    public async Task<string?> GetCookieHeaderAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.FplEmail) || string.IsNullOrEmpty(_options.FplPassword))
            return null;

        if (_cookieHeader is not null && DateTime.UtcNow < _expiresAt)
            return _cookieHeader;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring lock
            if (_cookieHeader is not null && DateTime.UtcNow < _expiresAt)
                return _cookieHeader;

            await LoginAsync(ct);
            return _cookieHeader;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Invalidates the cached session so the next call to GetCookieHeaderAsync re-authenticates.
    /// </summary>
    public void Invalidate() => _expiresAt = DateTime.MinValue;

    private async Task LoginAsync(CancellationToken ct)
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = true
        };

        using var client = new System.Net.Http.HttpClient(handler);
        client.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("login", _options.FplEmail),
            new KeyValuePair<string, string>("password", _options.FplPassword),
            new KeyValuePair<string, string>("app", "plfpl-web"),
            new KeyValuePair<string, string>("redirect_uri", "https://fantasy.premierleague.com/")
        ]);

        var response = await client.PostAsync(
            "https://users.premierleague.com/accounts/login/", formData, ct);

        var allCookies = cookieContainer.GetAllCookies();
        if (allCookies.Count == 0)
        {
            _logger.LogWarning("FPL login completed (HTTP {Status}) but no session cookies were returned.", (int)response.StatusCode);
            return;
        }

        _cookieHeader = string.Join("; ", allCookies.Select(c => $"{c.Name}={c.Value}"));
        _expiresAt = DateTime.UtcNow.AddHours(1);
        _logger.LogInformation("FPL session established, {Count} cookies cached.", allCookies.Count);
    }
}
