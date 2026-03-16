using FplTool.Modules.Picks.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.GetCurrentPick;

public sealed record GetCurrentPickQuery(Guid UserId, int GameweekId) : IRequest<Result<CaptainPickDto?>>;
