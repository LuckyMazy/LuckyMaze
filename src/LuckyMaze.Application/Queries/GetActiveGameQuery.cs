using System.Text.Json;
using Mediator;
using LuckyMaze.Application.Models;
using LuckyMaze.Application.Services;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Queries
{
    public record GetActiveGameQuery() : IQuery<Result<ActiveGameDetailsDto>>;

    public record ActiveGameDetailsDto(
        string State,
        int TimerSeconds,
        int? Width,
        int? Height,
        List<MazeCell>? GridData,
        List<MazeExit>? Exits);

    public class GetActiveGameQueryHandler(GameManager gameManager)
        : IQueryHandler<GetActiveGameQuery, Result<ActiveGameDetailsDto>>
    {
        public ValueTask<Result<ActiveGameDetailsDto>> Handle(GetActiveGameQuery query, CancellationToken cancellationToken)
        {
            var maze = gameManager.CurrentMaze;
            List<MazeCell>? gridData = null;
            List<MazeExit>? exits = null;

            if (maze != null)
            {
                gridData = JsonSerializer.Deserialize<List<MazeCell>>(maze.GridData);
                exits = JsonSerializer.Deserialize<List<MazeExit>>(maze.Exits);
            }

            var dto = new ActiveGameDetailsDto(
                gameManager.State.ToString(),
                gameManager.TimerSeconds,
                maze?.Width,
                maze?.Height,
                gridData,
                exits
            );

            return new ValueTask<Result<ActiveGameDetailsDto>>(Result<ActiveGameDetailsDto>.Success(dto));
        }
    }
}
