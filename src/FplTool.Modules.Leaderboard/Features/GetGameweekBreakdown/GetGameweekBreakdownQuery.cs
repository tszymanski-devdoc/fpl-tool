using FplTool.Modules.Leaderboard.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Leaderboard.Features.GetGameweekBreakdown;

public sealed record GetGameweekBreakdownQuery(int GameweekId) : IRequest<Result<List<GameweekBreakdownDto>>>;
