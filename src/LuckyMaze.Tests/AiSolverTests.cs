using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LuckyMaze.Application.Services;
using LuckyMaze.Domain;
using TUnit.Assertions;

namespace LuckyMaze.Tests
{
    public class AiSolverTests
    {
        private readonly MazeGenerator _generator = new();
        private readonly AiSolver _solver = new();

        [Test]
        public async Task Solve_FindsValidPathToExit()
        {
            // Arrange
            int width = 7;
            int height = 7;
            var maze = _generator.Generate(width, height, 2);
            var start = new Coordinate(width / 2, height / 2);

            var cells = JsonSerializer.Deserialize<List<MazeCell>>(maze.GridData)!;
            var cellMap = cells.ToDictionary(c => new Coordinate(c.X, c.Y));
            var exits = JsonSerializer.Deserialize<List<MazeExit>>(maze.Exits)!;
            var exitCoords = exits.Select(e => new Coordinate(e.X, e.Y)).ToHashSet();

            // Act
            var path = _solver.Solve(maze, start);

            // Assert
            await Assert.That(path).IsNotNull();
            await Assert.That(path.Count).IsGreaterThan(0);
            
            // Check first element is start
            await Assert.That(path.First()).IsEqualTo(start);

            // Check last element is one of the exits
            var last = path.Last();
            await Assert.That(exitCoords.Contains(last)).IsTrue();

            // Validate the path step-by-step
            for (int i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];

                // Must be adjacent (manhattan distance = 1)
                int manhattanDist = Math.Abs(current.X - next.X) + Math.Abs(current.Y - next.Y);
                await Assert.That(manhattanDist).IsEqualTo(1);

                // Must not pass through a wall
                var currentCell = cellMap[current];
                if (next.Y < current.Y) // Moved North
                {
                    await Assert.That(currentCell.North).IsFalse();
                }
                else if (next.X > current.X) // Moved East
                {
                    await Assert.That(currentCell.East).IsFalse();
                }
                else if (next.Y > current.Y) // Moved South
                {
                    await Assert.That(currentCell.South).IsFalse();
                }
                else if (next.X < current.X) // Moved West
                {
                    await Assert.That(currentCell.West).IsFalse();
                }
            }
        }
    }
}
