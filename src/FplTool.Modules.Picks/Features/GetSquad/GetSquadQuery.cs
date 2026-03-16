using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetSquad;

public sealed record GetSquadQuery(Guid UserId) : IRequest<Result<SquadDto>>;
