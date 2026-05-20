namespace LuckyMaze.Domain.Entities
{
    public class PlayerState
    {
        public required string UserId { get; init; }
        public bool IsReady { get; set; }
        public string? BetExitId { get; set; }
        public bool HasBet => !string.IsNullOrEmpty(BetExitId);
    }
}
