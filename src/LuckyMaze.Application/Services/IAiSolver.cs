using System.Collections.Generic;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Services
{
    public interface IAiSolver
    {
        /// <summary>
        /// Simulates pathfinding through the maze from the starting position until an exit is reached.
        /// Returns the sequence of coordinates traversed.
        /// </summary>
        List<Coordinate> Solve(Maze maze, Coordinate start);
    }
}
