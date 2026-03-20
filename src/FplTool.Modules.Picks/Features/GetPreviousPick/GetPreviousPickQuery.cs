using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetPreviousPick;

public sealed record GetPreviousPickQuery(Guid UserId) : IRequest<Result<PreviousPickResultDto?>>;
