namespace LuckyMaze.Infrastructure.Dtos;

public record AvailableModelDto(
    string Id,
    string Name,
    int ContextLength,
    List<string> InputModalities,
    List<string> OutputModalities,
    decimal PromptPricePerMillion,
    decimal CompletionPricePerMillion);
