using Microsoft.EntityFrameworkCore;

namespace LuckyMaze.Infrastructure.Services
{
    public class CreditsService(LuckyMazeDbContext dbContext) : ICreditsService
    {
        private const int MaxOutputTokensEstimate = 4000;
        private const int CharsPerToken = 4;

        public async Task<long> FromUsdAsync(decimal usd, CancellationToken cancellationToken = default)
        {
            var settings = await dbContext.AdminSettings.SingleAsync(cancellationToken);
            return (long)Math.Ceiling(usd * settings.CreditsPerUsd);
        }

        public async Task<decimal> ToUsdAsync(long credits, CancellationToken cancellationToken = default)
        {
            var settings = await dbContext.AdminSettings.SingleAsync(cancellationToken);
            return credits / settings.CreditsPerUsd;
        }

        public async Task<long> CalculateChatCreditsAsync(int promptTokens, int completionTokens, decimal promptPricePerMillion, decimal completionPricePerMillion, CancellationToken cancellationToken = default)
        {
            var settings = await dbContext.AdminSettings.SingleAsync(cancellationToken);
            var multiplier = settings.CostMultiplier == 0 ? 1m : settings.CostMultiplier;
            var usd = (promptTokens * promptPricePerMillion / 1_000_000m + completionTokens * completionPricePerMillion / 1_000_000m) * multiplier;
            return (long)Math.Ceiling(usd * settings.CreditsPerUsd);
        }

        public async Task<long> EstimateChatCreditsAsync(int inputCharCount, decimal promptPricePerMillion, decimal completionPricePerMillion, CancellationToken cancellationToken = default)
        {
            var estimatedInputTokens = inputCharCount / CharsPerToken;
            return await CalculateChatCreditsAsync(estimatedInputTokens, MaxOutputTokensEstimate, promptPricePerMillion, completionPricePerMillion, cancellationToken);
        }
    }
}
