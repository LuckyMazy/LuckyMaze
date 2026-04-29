using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Application.Command
{
    public record RenameChatCommand(Guid ChatId, string Title) : ICommand<Result>;

    public class RenameChatCommandHandler(IUserService userService, LuckyMazeDbContext dbContext) : ICommandHandler<RenameChatCommand, Result>
    {
        public async ValueTask<Result> Handle(RenameChatCommand command, CancellationToken cancellationToken)
        {
            var userId = await userService.GetCurrentUserIdAsync(cancellationToken);
            var chat = await dbContext.Chats.SingleOrDefaultAsync(c => c.Id == command.ChatId && c.UserId == userId, cancellationToken);

            if (chat is null)
                return Result.Failure("Chat not found");

            chat.Title = command.Title;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}
