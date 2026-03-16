using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetSquad;

public sealed record GetSquadByManagerQuery(int FplManagerId) : IRequest<Result<SquadDto>>;

internal sealed class GetSquadByManagerHandler : IRequestHandler<GetSquadByManagerQuery, Result<SquadDto>>
{
    private readonly IFplApiService _fplApiService;

    public GetSquadByManagerHandler(IFplApiService fplApiService)
    {
        _fplApiService = fplApiService;
    }

    public async Task<Result<SquadDto>> Handle(GetSquadByManagerQuery request, CancellationToken cancellationToken)
    {
        var bootstrap = await _fplApiService.GetBootstrapStaticAsync(cancellationToken);

        var upcomingGw = bootstrap.Events.FirstOrDefault(e => e.IsNext)
                         ?? bootstrap.Events.FirstOrDefault(e => e.IsCurrent);

        if (upcomingGw is null)
            return Result.Failure<SquadDto>(Error.Business("NO_ACTIVE_GAMEWEEK", "No active or upcoming gameweek found."));

        var picks = await _fplApiService.GetManagerPicksAsync(request.FplManagerId, upcomingGw.Id, cancellationToken);

        var playerMap = bootstrap.Elements.ToDictionary(p => p.Id);

        var squadPlayers = picks.Picks
            .Where(p => playerMap.ContainsKey(p.Element))
            .Select(p =>
            {
                var player = playerMap[p.Element];
                return new SquadPlayerDto(player.Id, player.WebName, player.TeamId, player.ElementType);
            })
            .ToList();

        return Result.Success(new SquadDto(
            upcomingGw.Id,
            upcomingGw.Name,
            upcomingGw.DeadlineTime,
            squadPlayers
        ));
    }
}
