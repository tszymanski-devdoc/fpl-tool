using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Pagination;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetMyPicks;

public sealed record GetMyPicksQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<CaptainPickDto>>>;
