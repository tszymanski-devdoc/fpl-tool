using FplTool.Modules.Picks.Features.GetAllPlayers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FplTool.Api.Controllers;

[ApiController]
[Route("api/v1/players")]
[Authorize]
public sealed class PlayersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlayersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAllPlayers(
        [FromQuery] int? position,
        [FromQuery] string? sortBy,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllPlayersQuery(position, sortBy, search, page, pageSize), ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Code, message = result.Error.Message });

        return Ok(result.Value);
    }
}
