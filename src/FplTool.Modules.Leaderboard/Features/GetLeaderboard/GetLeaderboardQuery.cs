using FplTool.Modules.Leaderboard.Contracts;
using FplTool.SharedKernel.Results;
using MediatR;

namespace FplTool.Modules.Leaderboard.Features.GetLeaderboard;

public sealed record GetLeaderboardQuery(int? GameweekId = null) : IRequest<Result<List<LeaderboardEntryDto>>>;
