using System.Text.Json;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Services
{
    public class MazeGenerator : IMazeGenerator
    {
        private readonly Random _random = new();

        public Maze Generate(int width, int height, int exitCount = 2)
        {
            if (width < 3 || height < 3)
                throw new ArgumentException("Maze dimensions must be at least 3x3.");

            if (exitCount < 2)
                throw new ArgumentException("Maze must have at least 2 exits.");

            var grid = new MazeCell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new MazeCell { X = x, Y = y };
                }
            }

            // Carve a perfect maze with an iterative depth-first search from the center.
            var stack = new Stack<MazeCell>();
            var visited = new HashSet<MazeCell>();

            var startCell = grid[width / 2, height / 2];
            visited.Add(startCell);
            stack.Push(startCell);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = GetUnvisitedNeighbors(grid, current, visited, width, height);

                if (neighbors.Count > 0)
                {
                    var next = neighbors[_random.Next(neighbors.Count)];
                    RemoveWall(current, next);
                    visited.Add(next);
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }

            var exits = ChooseExits(width, height, exitCount);
            foreach (var exit in exits)
            {
                var cell = grid[exit.X, exit.Y];
                if (exit.Y == 0) cell.North = false;
                else if (exit.X == width - 1) cell.East = false;
                else if (exit.Y == height - 1) cell.South = false;
                else if (exit.X == 0) cell.West = false;
            }

            var flatGrid = new List<MazeCell>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flatGrid.Add(grid[x, y]);
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = false };

            return new Maze
            {
                Width = width,
                Height = height,
                GridData = JsonSerializer.Serialize(flatGrid, options),
                Exits = JsonSerializer.Serialize(exits, options)
            };
        }

        private List<MazeCell> GetUnvisitedNeighbors(MazeCell[,] grid, MazeCell cell, HashSet<MazeCell> visited, int width, int height)
        {
            var neighbors = new List<MazeCell>();

            if (cell.Y > 0 && !visited.Contains(grid[cell.X, cell.Y - 1]))
                neighbors.Add(grid[cell.X, cell.Y - 1]);

            if (cell.X < width - 1 && !visited.Contains(grid[cell.X + 1, cell.Y]))
                neighbors.Add(grid[cell.X + 1, cell.Y]);

            if (cell.Y < height - 1 && !visited.Contains(grid[cell.X, cell.Y + 1]))
                neighbors.Add(grid[cell.X, cell.Y + 1]);

            if (cell.X > 0 && !visited.Contains(grid[cell.X - 1, cell.Y]))
                neighbors.Add(grid[cell.X - 1, cell.Y]);

            return neighbors;
        }

        private void RemoveWall(MazeCell current, MazeCell next)
        {
            if (next.Y < current.Y)
            {
                current.North = false;
                next.South = false;
            }
            else if (next.X > current.X)
            {
                current.East = false;
                next.West = false;
            }
            else if (next.Y > current.Y)
            {
                current.South = false;
                next.North = false;
            }
            else if (next.X < current.X)
            {
                current.West = false;
                next.East = false;
            }
        }

        private List<MazeExit> ChooseExits(int width, int height, int exitCount)
        {
            var borderCells = new List<(int X, int Y)>();

            // Border cells excluding corners, so each exit sits on a single wall.
            for (int x = 1; x < width - 1; x++)
            {
                borderCells.Add((x, 0));
                borderCells.Add((x, height - 1));
            }

            for (int y = 1; y < height - 1; y++)
            {
                borderCells.Add((width - 1, y));
                borderCells.Add((0, y));
            }

            borderCells = borderCells.OrderBy(_ => _random.Next()).ToList();

            var selectedExits = new List<MazeExit>();
            char exitChar = 'A';

            for (int i = 0; i < Math.Min(exitCount, borderCells.Count); i++)
            {
                var coord = borderCells[i];
                selectedExits.Add(new MazeExit
                {
                    X = coord.X,
                    Y = coord.Y,
                    Name = $"Exit {exitChar++}"
                });
            }

            return selectedExits;
        }
    }
}
