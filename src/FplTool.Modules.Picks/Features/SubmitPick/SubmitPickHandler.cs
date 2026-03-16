using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Contracts;
using FplTool.Modules.Picks.Domain;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.SubmitPick;

internal sealed class SubmitPickHandler : IRequestHandler<SubmitPickCommand, Result<CaptainPickDto>>
{
    private readonly PicksDbContext _dbContext;
    private readonly IFplApiService _fplApiService;

    public SubmitPickHandler(PicksDbContext dbContext, IFplApiService fplApiService)
    {
        _dbContext = dbContext;
        _fplApiService = fplApiService;
    }

    public async Task<Result<CaptainPickDto>> Handle(SubmitPickCommand request, CancellationToken cancellationToken)
    {
        var bootstrap = await _fplApiService.GetBootstrapStaticAsync(cancellationToken);

        var upcomingGw = bootstrap.Events.FirstOrDefault(e => e.IsNext)
                         ?? bootstrap.Events.FirstOrDefault(e => e.IsCurrent);

        if (upcomingGw is null)
            return Result.Failure<CaptainPickDto>(Error.Business("NO_ACTIVE_GAMEWEEK", "No active or upcoming gameweek found."));

        if (request.GameweekId != upcomingGw.Id)
            return Result.Failure<CaptainPickDto>(Error.Business("INVALID_GAMEWEEK", $"Captain pick is only allowed for gameweek {upcomingGw.Id}."));

        if (DateTime.UtcNow >= upcomingGw.DeadlineTime)
            return Result.Failure<CaptainPickDto>(Error.Business("DEADLINE_PASSED", $"The deadline for gameweek {upcomingGw.Id} has passed."));

        var player = bootstrap.Elements.FirstOrDefault(p => p.Id == request.FplPlayerId);
        if (player is null)
            return Result.Failure<CaptainPickDto>(Error.NotFound($"Player with ID {request.FplPlayerId} not found."));

        var existingPick = await _dbContext.CaptainPicks
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.GameweekId == request.GameweekId, cancellationToken);

        CaptainPick pick;
        if (existingPick is null)
        {
            pick = CaptainPick.Create(request.UserId, request.GameweekId, request.FplPlayerId, player.WebName);
            _dbContext.CaptainPicks.Add(pick);
        }
        else
        {
            existingPick.UpdatePlayer(request.FplPlayerId, player.WebName);
            pick = existingPick;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new CaptainPickDto(
            pick.Id,
            pick.GameweekId,
            pick.FplPlayerId,
            pick.PlayerWebName,
            pick.PointsScored,
            pick.PickedAtUtc
        ));
    }
}
