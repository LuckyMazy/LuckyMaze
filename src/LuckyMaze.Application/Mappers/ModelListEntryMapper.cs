using LuckyMaze.Application.Dtos;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Mappers
{
    public static class ModelListEntryMapper
    {
        public static ModelListEntryDto ToDto(this ModelListEntry entry) =>
            new(entry.Id, entry.ModelId, entry.ListType);
    }
}
