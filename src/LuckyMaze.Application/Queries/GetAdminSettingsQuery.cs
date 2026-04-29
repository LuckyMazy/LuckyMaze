using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Queries
{
    public record GetAdminSettingsQuery() : IQuery<Result<AdminSettingsDto>>;

    public class GetAdminSettingsQueryHandler(LuckyMazeDbContext dbContext) : IQueryHandler<GetAdminSettingsQuery, Result<AdminSettingsDto>>
    {
        public async ValueTask<Result<AdminSettingsDto>> Handle(GetAdminSettingsQuery query, CancellationToken cancellationToken)
        {
            var settings = await dbContext.AdminSettings.SingleAsync(cancellationToken);
            return Result<AdminSettingsDto>.Success(new AdminSettingsDto(
                settings.MaxPricePerMillionTokens,
                settings.ActiveModelListMode,
                settings.StartingBalance,
                settings.CostMultiplier,
                settings.CreditsPerUsd));
        }
    }
}
