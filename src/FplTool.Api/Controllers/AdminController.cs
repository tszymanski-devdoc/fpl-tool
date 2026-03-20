using FplTool.Modules.Picks.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FplTool.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly IPointsSyncService _pointsSyncService;

    public AdminController(IPointsSyncService pointsSyncService)
    {
        _pointsSyncService = pointsSyncService;
    }

    [HttpPost("sync")]
    public async Task<IActionResult> ForceSync(CancellationToken ct)
    {
        var syncedGameweeks = await _pointsSyncService.SyncPendingGameweeksAsync(ct);
        return Ok(new { syncedGameweeks });
    }
}
