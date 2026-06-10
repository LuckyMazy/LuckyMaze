using System;
using System.Collections.Generic;
using System.Linq;
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

            // 1. Initialize grid of cells with all walls up
            var grid = new MazeCell[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new MazeCell { X = x, Y = y };
                }
            }

            // 2. DFS Maze Generation
            var stack = new Stack<MazeCell>();
            var visited = new HashSet<MazeCell>();
            
            // Start DFS at center of the maze
            var startCell = grid[width / 2, height / 2];
            visited.Add(startCell);
            stack.Push(startCell);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = GetUnvisitedNeighbors(grid, current, visited, width, height);

                if (neighbors.Count > 0)
                {
                    // Choose a random neighbor
                    var next = neighbors[_random.Next(neighbors.Count)];
                    
                    // Remove wall between current and next
                    RemoveWall(current, next);
                    
                    visited.Add(next);
                    stack.Push(next);
                }
                else
                {
                    stack.Pop();
                }
            }

            // 3. Create exits on the borders
            var exits = ChooseExits(width, height, exitCount);
            foreach (var exit in exits)
            {
                var cell = grid[exit.X, exit.Y];
                if (exit.Y == 0) cell.North = false;
                else if (exit.X == width - 1) cell.East = false;
                else if (exit.Y == height - 1) cell.South = false;
                else if (exit.X == 0) cell.West = false;
            }

            // 4. Flatten grid for JSON serialization
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

            // North neighbor
            if (cell.Y > 0 && !visited.Contains(grid[cell.X, cell.Y - 1]))
                neighbors.Add(grid[cell.X, cell.Y - 1]);

            // East neighbor
            if (cell.X < width - 1 && !visited.Contains(grid[cell.X + 1, cell.Y]))
                neighbors.Add(grid[cell.X + 1, cell.Y]);

            // South neighbor
            if (cell.Y < height - 1 && !visited.Contains(grid[cell.X, cell.Y + 1]))
                neighbors.Add(grid[cell.X, cell.Y + 1]);

            // West neighbor
            if (cell.X > 0 && !visited.Contains(grid[cell.X - 1, cell.Y]))
                neighbors.Add(grid[cell.X - 1, cell.Y]);

            return neighbors;
        }

        private void RemoveWall(MazeCell current, MazeCell next)
        {
            if (next.Y < current.Y) // Next is North
            {
                current.North = false;
                next.South = false;
            }
            else if (next.X > current.X) // Next is East
            {
                current.East = false;
                next.West = false;
            }
            else if (next.Y > current.Y) // Next is South
            {
                current.South = false;
                next.North = false;
            }
            else if (next.X < current.X) // Next is West
            {
                current.West = false;
                next.East = false;
            }
        }

        private List<MazeExit> ChooseExits(int width, int height, int exitCount)
        {
            var borderCells = new List<(int X, int Y)>();

            // Collect North/South border cells (excluding corners to keep math simple)
            for (int x = 1; x < width - 1; x++)
            {
                borderCells.Add((x, 0)); // North
                borderCells.Add((x, height - 1)); // South
            }

            // Collect East/West border cells (excluding corners)
            for (int y = 1; y < height - 1; y++)
            {
                borderCells.Add((width - 1, y)); // East
                borderCells.Add((0, y)); // West
            }

            // Shuffle border cells
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
