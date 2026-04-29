using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Dtos;

public record MessageDto(Guid Id, MessageRole Role, string Content, string? Model, string? ToolCalls, string? ToolCallId, string? FinishReason, int SequenceNumber, DateTime CreatedAt);
