namespace LuckyMaze.Infrastructure.Services
{
    public interface ICreditsService
    {
        Task<long> FromUsdAsync(decimal usd, CancellationToken cancellationToken = default);
        Task<decimal> ToUsdAsync(long credits, CancellationToken cancellationToken = default);
        Task<long> CalculateChatCreditsAsync(int promptTokens, int completionTokens, decimal promptPricePerMillion, decimal completionPricePerMillion, CancellationToken cancellationToken = default);
        Task<long> EstimateChatCreditsAsync(int inputCharCount, decimal promptPricePerMillion, decimal completionPricePerMillion, CancellationToken cancellationToken = default);
    }
}
