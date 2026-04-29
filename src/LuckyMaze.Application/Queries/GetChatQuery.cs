using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Mappers;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Application.Queries
{
    public record GetChatQuery(Guid ChatId) : IQuery<Result<ChatDetailDto>>;

    public class GetChatQueryHandler(IUserService userService, LuckyMazeDbContext dbContext) : IQueryHandler<GetChatQuery, Result<ChatDetailDto>>
    {
        public async ValueTask<Result<ChatDetailDto>> Handle(GetChatQuery query, CancellationToken cancellationToken)
        {
            var userId = await userService.GetCurrentUserIdAsync(cancellationToken);
            var chat = await dbContext.Chats
                .Include(c => c.Messages.OrderBy(m => m.SequenceNumber))
                .SingleOrDefaultAsync(c => c.Id == query.ChatId && c.UserId == userId, cancellationToken);

            if (chat is null)
                return Result<ChatDetailDto>.Failure("Chat not found");

            return Result<ChatDetailDto>.Success(chat.ToDetailDto());
        }
    }
}
