using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Mappers;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Application.Queries
{
    public record GetChatsQuery() : IQuery<Result<List<ChatDto>>>;

    public class GetChatsQueryHandler(IUserService userService, LuckyMazeDbContext dbContext) : IQueryHandler<GetChatsQuery, Result<List<ChatDto>>>
    {
        public async ValueTask<Result<List<ChatDto>>> Handle(GetChatsQuery query, CancellationToken cancellationToken)
        {
            var userId = await userService.GetCurrentUserIdAsync(cancellationToken);
            var chats = await dbContext.Chats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync(cancellationToken);
            return Result<List<ChatDto>>.Success(chats.Select(c => c.ToDto()).ToList());
        }
    }
}
