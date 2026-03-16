using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Picks.Features.SyncPoints;

public sealed record SyncGameweekPointsCommand(int GameweekId) : IRequest<Result>;
