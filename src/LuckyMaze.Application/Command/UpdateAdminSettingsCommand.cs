using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Models;
using LuckyMaze.Domain.Enums;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Command
{
    public record UpdateAdminSettingsCommand(
        decimal? MaxPricePerMillionTokens,
        ModelListMode ActiveModelListMode,
        long StartingBalance,
        decimal CostMultiplier,
        decimal CreditsPerUsd) : ICommand<Result<AdminSettingsDto>>;

    public class UpdateAdminSettingsCommandHandler(LuckyMazeDbContext dbContext) : ICommandHandler<UpdateAdminSettingsCommand, Result<AdminSettingsDto>>
    {
        public async ValueTask<Result<AdminSettingsDto>> Handle(UpdateAdminSettingsCommand command, CancellationToken cancellationToken)
        {
            if (command.CreditsPerUsd <= 0)
                return Result<AdminSettingsDto>.Failure("CreditsPerUsd must be greater than zero");
            if (command.CostMultiplier <= 0)
                return Result<AdminSettingsDto>.Failure("CostMultiplier must be greater than zero");
            if (command.StartingBalance < 0)
                return Result<AdminSettingsDto>.Failure("StartingBalance cannot be negative");

            var settings = await dbContext.AdminSettings.SingleAsync(cancellationToken);
            settings.MaxPricePerMillionTokens = command.MaxPricePerMillionTokens;
            settings.ActiveModelListMode = command.ActiveModelListMode;
            settings.StartingBalance = command.StartingBalance;
            settings.CostMultiplier = command.CostMultiplier;
            settings.CreditsPerUsd = command.CreditsPerUsd;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<AdminSettingsDto>.Success(new AdminSettingsDto(
                settings.MaxPricePerMillionTokens,
                settings.ActiveModelListMode,
                settings.StartingBalance,
                settings.CostMultiplier,
                settings.CreditsPerUsd));
        }
    }
}
