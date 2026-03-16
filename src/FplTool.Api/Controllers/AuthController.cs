using FplTool.Modules.Auth.Features.GetProfile;
using FplTool.Modules.Auth.Features.UpdateProfile;
using FplTool.Modules.Auth.Features.VerifyGoogleToken;
using FplTool.SharedKernel.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FplTool.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("auth/google")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyGoogleTokenCommand(request.IdToken), ct);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error.Code, message = result.Error.Message });

        return Ok(result.Value);
    }

    [Authorize]
    [HttpGet("users/me")]
    public async Task<IActionResult> GetProfile([FromServices] ICurrentUserContext currentUser, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProfileQuery(currentUser.UserId), ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error.Code, message = result.Error.Message });

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPatch("users/me")]
    public async Task<IActionResult> UpdateProfile(
        [FromServices] ICurrentUserContext currentUser,
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProfileCommand(currentUser.UserId, request.DisplayName, request.FplManagerId), ct);

        if (result.IsFailure)
            return NotFound(new { error = result.Error.Code, message = result.Error.Message });

        return Ok(result.Value);
    }
}

public sealed record GoogleAuthRequest(string IdToken);
public sealed record UpdateProfileRequest(string? DisplayName, int? FplManagerId);
