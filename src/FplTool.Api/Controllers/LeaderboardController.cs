using FplTool.Modules.Leaderboard.Features.GetGameweekBreakdown;
using FplTool.Modules.Leaderboard.Features.GetLeaderboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FplTool.Api.Controllers;

[ApiController]
[Route("api/v1/leaderboard")]
public sealed class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaderboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int? gameweekId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLeaderboardQuery(gameweekId), ct);
        return Ok(result.Value);
    }

    [HttpGet("{gameweekId:int}")]
    public async Task<IActionResult> GetGameweekBreakdown(int gameweekId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGameweekBreakdownQuery(gameweekId), ct);
        return Ok(result.Value);
    }
}
