namespace FplTool.Modules.FplIntegration.Auth;

internal sealed class FplAuthHandler : DelegatingHandler
{
    private readonly FplAuthService _authService;

    public FplAuthHandler(FplAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var cookie = await _authService.GetCookieHeaderAsync(ct);
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.TryAddWithoutValidation("Cookie", cookie);

        var response = await base.SendAsync(request, ct);

        // If we get a 401/403, invalidate the session so the next request re-authenticates
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            _authService.Invalidate();

        return response;
    }
}
