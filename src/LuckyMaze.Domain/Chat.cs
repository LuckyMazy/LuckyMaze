namespace LuckyMaze.Domain
{
    public class Chat : BaseEntity
    {
        public required Guid UserId { get; init; }
        public string Title { get; set; } = "New Chat";
        public required User User { get; init; }
        public required ICollection<Message> Messages { get; init; } = [];
    }
}
