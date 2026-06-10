using Mediator;
using LuckyMaze.Application.Models;
using LuckyMaze.Application.Services;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Application.Command
{
    public record ToggleReadyCommand(bool IsReady) : ICommand<Result<Unit>>;

    public class ToggleReadyCommandHandler(IOidcService oidcService, GameManager gameManager)
        : ICommandHandler<ToggleReadyCommand, Result<Unit>>
    {
        public async ValueTask<Result<Unit>> Handle(ToggleReadyCommand command, CancellationToken cancellationToken)
        {
            var oidcUser = await oidcService.GetCurrentUserAsync(cancellationToken);
            if (oidcUser == null)
                return Result<Unit>.Failure("Unauthorized");

            await gameManager.ToggleReadyAsync(oidcUser.ExternalId, command.IsReady);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
