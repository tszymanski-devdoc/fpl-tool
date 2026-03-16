using System.Security.Claims;
using FplTool.SharedKernel.Interfaces;

namespace FplTool.Api.Infrastructure.Security;

public sealed class CurrentUserContext : ICurrentUserContext
{
    public Guid UserId { get; }
    public string Email { get; }

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User
                   ?? throw new InvalidOperationException("No HTTP context available.");

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? throw new InvalidOperationException("User ID claim not found.");

        UserId = Guid.Parse(userIdClaim);
        Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    }
}
