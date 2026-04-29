using Mediator;
using Microsoft.EntityFrameworkCore;
using LuckyMaze.Application.Models;
using LuckyMaze.Infrastructure;
using LuckyMaze.Infrastructure.Dtos;

namespace LuckyMaze.Application.Queries
{
    public record GetAllModelsQuery() : IQuery<Result<List<AvailableModelDto>>>;

    public class GetAllModelsQueryHandler(LuckyMazeDbContext dbContext) : IQueryHandler<GetAllModelsQuery, Result<List<AvailableModelDto>>>
    {
        public async ValueTask<Result<List<AvailableModelDto>>> Handle(GetAllModelsQuery query, CancellationToken cancellationToken)
        {
            var models = await dbContext.Models
                .Select(m => new AvailableModelDto(
                    m.ModelId,
                    m.Name,
                    m.ContextLength,
                    m.InputModalities,
                    m.OutputModalities,
                    m.PromptPricePerMillion,
                    m.CompletionPricePerMillion))
                .ToListAsync(cancellationToken);

            return Result<List<AvailableModelDto>>.Success(models);
        }
    }
}
