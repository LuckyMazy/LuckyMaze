namespace LuckyMaze.Application.Dtos;

public record ChatDetailDto(Guid Id, string Title, DateTime CreatedAt, DateTime UpdatedAt, List<MessageDto> Messages);
