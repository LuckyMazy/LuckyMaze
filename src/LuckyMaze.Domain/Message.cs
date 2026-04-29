using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Domain
{
    public class Message : BaseEntity
    {
        public required Guid ChatId { get; init; }
        public required MessageRole Role { get; set; }
        public required string Content { get; set; }
        public string? Model { get; set; }
        public string? ToolCalls { get; set; }
        public string? ToolCallId { get; set; }
        public string? TokenUsage { get; set; }
        public string? FinishReason { get; set; }
        public required int SequenceNumber { get; set; }
        public Chat Chat { get; init; } = null!;
    }
}
