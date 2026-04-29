using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Dtos;
using LuckyMaze.Application.Mappers;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;

namespace LuckyMaze.Application.Queries
{
    public record GetModelListEntriesQuery() : IQuery<Result<List<ModelListEntryDto>>>;

    public class GetModelListEntriesQueryHandler(LuckyMazeDbContext dbContext) : IQueryHandler<GetModelListEntriesQuery, Result<List<ModelListEntryDto>>>
    {
        public async ValueTask<Result<List<ModelListEntryDto>>> Handle(GetModelListEntriesQuery query, CancellationToken cancellationToken)
        {
            var entries = await dbContext.ModelListEntries.ToListAsync(cancellationToken);
            return Result<List<ModelListEntryDto>>.Success(entries.Select(e => e.ToDto()).ToList());
        }
    }
}
