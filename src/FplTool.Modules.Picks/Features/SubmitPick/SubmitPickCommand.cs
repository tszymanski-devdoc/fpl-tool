using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.SubmitPick;

public sealed record SubmitPickCommand(
    Guid UserId,
    int GameweekId,
    int FplPlayerId
) : IRequest<Result<CaptainPickDto>>;
