using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Dtos;

public record ModelListEntryDto(Guid Id, string ModelId, ModelListType ListType);
