using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Models;
using LuckyMaze.Domain.Enums;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Command
{
    public record AdjustUserCreditsCommand(Guid UserId, long Amount, CreditAdjustmentMode Mode) : ICommand<Result>;

    public class AdjustUserCreditsCommandHandler(LuckyMazeDbContext dbContext) : ICommandHandler<AdjustUserCreditsCommand, Result>
    {
        public async ValueTask<Result> Handle(AdjustUserCreditsCommand command, CancellationToken cancellationToken)
        {
            if (command.Amount < 0)
                return Result.Failure("Amount must be non-negative; use Mode = Remove to subtract");

            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);
            if (user is null)
                return Result.Failure("User not found");

            user.CreditBalance = command.Mode switch
            {
                CreditAdjustmentMode.Add => user.CreditBalance + command.Amount,
                CreditAdjustmentMode.Remove => user.CreditBalance - command.Amount,
                CreditAdjustmentMode.Overwrite => command.Amount,
                _ => user.CreditBalance
            };

            if (user.CreditBalance < 0)
                return Result.Failure("Credit balance cannot go below zero");

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
