namespace LuckyMaze.Application.Dtos;

public record SendMessageDto(Guid ChatId, MessageDto UserMessage, MessageDto AssistantMessage);
