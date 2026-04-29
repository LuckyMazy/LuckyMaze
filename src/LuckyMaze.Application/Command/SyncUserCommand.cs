using Mediator;
using LuckyMaze.Infrastructure.Services;
using LuckyMaze.Application.Models;

namespace LuckyMaze.Application.Command
{
    public record SyncUserCommand() : ICommand<Result>;

    public class SyncUserCommandHandler(IUserService userService) : ICommandHandler<SyncUserCommand, Result>
    {
        public async ValueTask<Result> Handle(SyncUserCommand command, CancellationToken cancellationToken)
        {
            await userService.SyncCurrentUserAsync(cancellationToken);
            return Result.Success();
        }
    }
}
