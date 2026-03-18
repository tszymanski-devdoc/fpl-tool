using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetAllPlayers;

public sealed record GetAllPlayersQuery(
    int? Position,
    string? SortBy,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<AllPlayersDto>>;
