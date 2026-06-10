namespace LuckyMaze.Application.Services
{
    public class ConnectedPlayer
    {
        public Guid UserId { get; set; }
        public required string ExternalId { get; set; }
        public required string DisplayName { get; set; }
        public required string ConnectionId { get; set; }
        public decimal Balance { get; set; }
        public bool IsReady { get; set; }
    }

    public class PlayerBet
    {
        public Guid UserId { get; set; }
        public required string DisplayName { get; set; }
        public required string ExitName { get; set; }
        public decimal Amount { get; set; }
    }

    public class PayoutDetail
    {
        public required string DisplayName { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal NetProfit { get; set; }
    }
}
