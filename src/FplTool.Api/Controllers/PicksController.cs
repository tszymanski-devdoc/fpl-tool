using FplTool.Modules.Picks.Features.GetCurrentPick;
using FplTool.Modules.Picks.Features.GetMyPicks;
using FplTool.Modules.Picks.Features.GetPreviousPick;
using FplTool.Modules.Picks.Features.SubmitPick;
using FplTool.SharedKernel.Interfaces;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FplTool.Api.Controllers;

[ApiController]
[Route("api/v1/picks")]
[Authorize]
public sealed class PicksController : ControllerBase
{
    private readonly IMediator _mediator;

    public PicksController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> SubmitPick(
        [FromServices] ICurrentUserContext currentUser,
        [FromBody] SubmitPickRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitPickCommand(currentUser.UserId, request.GameweekId, request.FplPlayerId), ct);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "DEADLINE_PASSED" or "INVALID_GAMEWEEK" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                "NOT_FOUND" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Message })
            };
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPicks(
        [FromServices] ICurrentUserContext currentUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMyPicksQuery(currentUser.UserId, page, pageSize), ct);
        return Ok(result.Value);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentPick(
        [FromServices] ICurrentUserContext currentUser,
        [FromQuery] int gameweekId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentPickQuery(currentUser.UserId, gameweekId), ct);

        if (result.Value is null)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpGet("previous")]
    public async Task<IActionResult> GetPreviousPick(
        [FromServices] ICurrentUserContext currentUser,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPreviousPickQuery(currentUser.UserId), ct);

        if (result.Value is null)
            return NoContent();

        return Ok(result.Value);
    }
}

public sealed record SubmitPickRequest(int GameweekId, int FplPlayerId);
