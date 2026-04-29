using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Command
{
    public record SetUserLockCommand(Guid UserId, bool IsLocked) : ICommand<Result>;

    public class SetUserLockCommandHandler(LuckyMazeDbContext dbContext) : ICommandHandler<SetUserLockCommand, Result>
    {
        public async ValueTask<Result> Handle(SetUserLockCommand command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            if (user is null)
                return Result.Failure("User not found");

            user.IsLocked = command.IsLocked;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
