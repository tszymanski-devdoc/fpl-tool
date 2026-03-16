using FplTool.Modules.FplIntegration.Contracts;
using FplTool.Modules.Picks.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.GetSquad;

internal sealed class GetSquadHandler : IRequestHandler<GetSquadQuery, Result<SquadDto>>
{
    private readonly PicksDbContext _dbContext;
    private readonly IFplApiService _fplApiService;

    public GetSquadHandler(PicksDbContext dbContext, IFplApiService fplApiService)
    {
        _dbContext = dbContext;
        _fplApiService = fplApiService;
    }

    public async Task<Result<SquadDto>> Handle(GetSquadQuery request, CancellationToken cancellationToken)
    {
        // We need the user's FplManagerId — fetched from a shared read via raw SQL or passed in
        // For now we use a denormalized approach: caller provides FplManagerId via a separate query.
        // GetSquadQuery stores UserId; we look up FplManagerId from the auth DB via a service.
        // Simplified: we inject IFplManagerResolver that does the cross-module lookup.
        throw new NotImplementedException("Use GetSquadByManagerIdQuery instead via the controller.");
    }
}
