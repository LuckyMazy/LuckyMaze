using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Mappers;
using LuckyMaze.Application.Models;
using LuckyMaze.Domain;
using LuckyMaze.Domain.Enums;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Command
{
    public record AddModelListEntryCommand(string ModelId, ModelListType ListType) : ICommand<Result<ModelListEntryDto>>;

    public class AddModelListEntryCommandHandler(LuckyMazeDbContext dbContext) : ICommandHandler<AddModelListEntryCommand, Result<ModelListEntryDto>>
    {
        public async ValueTask<Result<ModelListEntryDto>> Handle(AddModelListEntryCommand command, CancellationToken cancellationToken)
        {
            var exists = await dbContext.ModelListEntries.AnyAsync(e => e.ModelId == command.ModelId && e.ListType == command.ListType, cancellationToken);
            if (exists)
                return Result<ModelListEntryDto>.Failure($"Model '{command.ModelId}' is already on the {command.ListType} list");

            var entry = new ModelListEntry { ModelId = command.ModelId, ListType = command.ListType };
            dbContext.ModelListEntries.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result<ModelListEntryDto>.Success(entry.ToDto());
        }
    }
}
