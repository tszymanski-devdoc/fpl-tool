using FplTool.Modules.Picks.Contracts;
using FplTool.Modules.Picks.Infrastructure;
using FplTool.SharedKernel.Pagination;
using FplTool.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FplTool.Modules.Picks.Features.GetMyPicks;

internal sealed class GetMyPicksHandler : IRequestHandler<GetMyPicksQuery, Result<PagedResult<CaptainPickDto>>>
{
    private readonly PicksDbContext _dbContext;

    public GetMyPicksHandler(PicksDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<PagedResult<CaptainPickDto>>> Handle(GetMyPicksQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.CaptainPicks
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId)
            .OrderByDescending(p => p.GameweekId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new CaptainPickDto(p.Id, p.GameweekId, p.FplPlayerId, p.PlayerWebName, p.PointsScored, p.PickedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<CaptainPickDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }
}
