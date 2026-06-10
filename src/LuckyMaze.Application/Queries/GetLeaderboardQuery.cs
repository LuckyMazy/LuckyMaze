using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Queries
{
    public record GetLeaderboardQuery(int Limit = 10) : IQuery<Result<List<LeaderboardEntryDto>>>;

    public class GetLeaderboardQueryHandler(LuckyMazeDbContext dbContext)
        : IQueryHandler<GetLeaderboardQuery, Result<List<LeaderboardEntryDto>>>
    {
        public async ValueTask<Result<List<LeaderboardEntryDto>>> Handle(GetLeaderboardQuery query, CancellationToken cancellationToken)
        {
            var users = await dbContext.Users
                .OrderByDescending(u => u.Balance)
                .Take(query.Limit)
                .Select(u => new LeaderboardEntryDto(
                    u.DisplayName,
                    u.Balance,
                    u.AvatarUrl
                ))
                .ToListAsync(cancellationToken);

            return Result<List<LeaderboardEntryDto>>.Success(users);
        }
    }
}
