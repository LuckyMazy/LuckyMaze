using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Queries
{
    public record GetGameHistoryQuery(int Limit = 20) : IQuery<Result<List<GameHistoryDto>>>;

    public class GetGameHistoryQueryHandler(LuckyMazeDbContext dbContext) 
        : IQueryHandler<GetGameHistoryQuery, Result<List<GameHistoryDto>>>
    {
        public async ValueTask<Result<List<GameHistoryDto>>> Handle(GetGameHistoryQuery query, CancellationToken cancellationToken)
        {
            var games = await dbContext.Games
                .OrderByDescending(g => g.StartedAt)
                .Take(query.Limit)
                .Select(g => new GameHistoryDto(
                    g.Id,
                    g.State,
                    g.WinningExit,
                    g.StartedAt,
                    g.EndedAt
                ))
                .ToListAsync(cancellationToken);

            return Result<List<GameHistoryDto>>.Success(games);
        }
    }
}
