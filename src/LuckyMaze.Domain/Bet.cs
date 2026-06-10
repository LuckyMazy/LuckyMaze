using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Domain
{
    public class Bet : BaseEntity
    {
        public Guid GameId { get; set; }
        public Game? Game { get; set; }

        public Guid PlayerId { get; set; }
        public User? Player { get; set; }

        public required string ExitName { get; set; }
        public decimal Amount { get; set; }
        public BetStatus Status { get; set; } = BetStatus.Pending;
    }
}
