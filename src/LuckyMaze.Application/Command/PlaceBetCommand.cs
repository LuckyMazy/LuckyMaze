using System.Threading;
using System.Threading.Tasks;
using Mediator;
using LuckyMaze.Application.Models;
using LuckyMaze.Application.Services;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Application.Command
{
    public record PlaceBetCommand(string ExitName, decimal Amount) : ICommand<Result<Unit>>;

    public class PlaceBetCommandHandler(IOidcService oidcService, GameManager gameManager)
        : ICommandHandler<PlaceBetCommand, Result<Unit>>
    {
        public async ValueTask<Result<Unit>> Handle(PlaceBetCommand command, CancellationToken cancellationToken)
        {
            var oidcUser = await oidcService.GetCurrentUserAsync(cancellationToken);
            if (oidcUser == null)
                return Result<Unit>.Failure("Unauthorized");

            await gameManager.PlaceBetAsync(oidcUser.ExternalId, command.ExitName, command.Amount);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}
