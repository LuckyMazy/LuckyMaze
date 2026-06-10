using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LuckyMaze.Domain;

namespace LuckyMaze.Application.Services
{
    public class AiSolver : IAiSolver
    {
        private readonly Random _random = new();

        public List<Coordinate> Solve(Maze maze, Coordinate start)
        {
            var cellsList = JsonSerializer.Deserialize<List<MazeCell>>(maze.GridData) 
                ?? throw new InvalidOperationException("Failed to deserialize maze cells.");
            var exitsList = JsonSerializer.Deserialize<List<MazeExit>>(maze.Exits) 
                ?? throw new InvalidOperationException("Failed to deserialize maze exits.");

            int width = maze.Width;
            int height = maze.Height;

            // Map cells for quick lookups by coordinate
            var gridMap = cellsList.ToDictionary(c => new Coordinate(c.X, c.Y));
            var exitCoords = exitsList.Select(e => new Coordinate(e.X, e.Y)).ToHashSet();

            // 1. Train Q-learning agent on the generated maze layout
            double[,] qTable = TrainAgent(width, height, gridMap, exitCoords, start);

            // 2. Simulate gameplay path using Epsilon-Greedy policy (15% randomness for unpredictability)
            return SimulatePath(width, height, gridMap, exitCoords, start, qTable, epsilon: 0.15);
        }

        private double[,] TrainAgent(int width, int height, Dictionary<Coordinate, MazeCell> gridMap, HashSet<Coordinate> exits, Coordinate start)
        {
            int numStates = width * height;
            int numActions = 4; // 0: North, 1: East, 2: South, 3: West
            double[,] qTable = new double[numStates, numActions];

            // Hyperparameters
            double alpha = 0.2;     // Learning rate
            double gamma = 0.9;     // Discount factor
            double epsilon = 0.3;   // Exploration rate during training
            int episodes = 1500;    // Number of training iterations
            int maxSteps = width * height * 2;

            for (int episode = 0; episode < episodes; episode++)
            {
                // Start training at start coordinate or a random non-exit coordinate to generalize paths
                Coordinate current = _random.NextDouble() < 0.3 ? start : GetRandomState(width, height, exits);
                int steps = 0;

                while (!exits.Contains(current) && steps < maxSteps)
                {
                    int stateIdx = GetStateIndex(current, width);
                    var validActions = GetValidActions(current, gridMap);

                    if (validActions.Count == 0)
                        break; // Dead end (shouldn't happen in a valid perfect maze)

                    int action;
                    if (_random.NextDouble() < epsilon)
                    {
                        // Explore
                        action = validActions[_random.Next(validActions.Count)];
                    }
                    else
                    {
                        // Exploit
                        action = GetBestAction(stateIdx, validActions, qTable);
                    }

                    Coordinate next = Move(current, action);
                    double reward = exits.Contains(next) ? 100.0 : -1.0;

                    int nextStateIdx = GetStateIndex(next, width);
                    var nextValidActions = GetValidActions(next, gridMap);
                    
                    double maxNextQ = 0.0;
                    if (nextValidActions.Count > 0 && !exits.Contains(next))
                    {
                        maxNextQ = nextValidActions.Max(a => qTable[nextStateIdx, a]);
                    }

                    // Q-learning update formula
                    qTable[stateIdx, action] += alpha * (reward + gamma * maxNextQ - qTable[stateIdx, action]);

                    current = next;
                    steps++;
                }

                // Decay epsilon
                epsilon = Math.Max(0.01, epsilon * 0.995);
            }

            return qTable;
        }

        private List<Coordinate> SimulatePath(int width, int height, Dictionary<Coordinate, MazeCell> gridMap, HashSet<Coordinate> exits, Coordinate start, double[,] qTable, double epsilon)
        {
            var path = new List<Coordinate> { start };
            Coordinate current = start;
            int maxSteps = width * height * 4; // Safely avoid infinite loops
            int steps = 0;

            while (!exits.Contains(current) && steps < maxSteps)
            {
                int stateIdx = GetStateIndex(current, width);
                var validActions = GetValidActions(current, gridMap);

                if (validActions.Count == 0)
                    break;

                int action;
                // Epsilon-Greedy choice (15% random chance for suspense)
                if (_random.NextDouble() < epsilon)
                {
                    action = validActions[_random.Next(validActions.Count)];
                }
                else
                {
                    action = GetBestAction(stateIdx, validActions, qTable);
                }

                current = Move(current, action);
                path.Add(current);
                steps++;
            }

            // If we broke out of the loop and ended up in an exit's adjacent neighbor,
            // make sure we step out of the maze if the wall is broken
            // (The exits are on the border, but we can step one unit out if needed, or consider the border cell as the exit).
            // In our ChooseExits method, border cells themselves are marked as exits.
            // When we reach the border cell (the exit), we have reached the exit state.

            return path;
        }

        private List<int> GetValidActions(Coordinate coord, Dictionary<Coordinate, MazeCell> gridMap)
        {
            var valid = new List<int>();
            if (!gridMap.TryGetValue(coord, out var cell))
                return valid;

            if (!cell.North) valid.Add(0);
            if (!cell.East)  valid.Add(1);
            if (!cell.South) valid.Add(2);
            if (!cell.West)  valid.Add(3);

            return valid;
        }

        private Coordinate Move(Coordinate coord, int action)
        {
            return action switch
            {
                0 => new Coordinate(coord.X, coord.Y - 1), // North
                1 => new Coordinate(coord.X + 1, coord.Y), // East
                2 => new Coordinate(coord.X, coord.Y + 1), // South
                3 => new Coordinate(coord.X - 1, coord.Y), // West
                _ => coord
            };
        }

        private int GetStateIndex(Coordinate coord, int width)
        {
            return coord.Y * width + coord.X;
        }

        private Coordinate GetRandomState(int width, int height, HashSet<Coordinate> exits)
        {
            while (true)
            {
                int x = _random.Next(width);
                int y = _random.Next(height);
                var coord = new Coordinate(x, y);
                if (!exits.Contains(coord))
                    return coord;
            }
        }

        private int GetBestAction(int stateIdx, List<int> validActions, double[,] qTable)
        {
            int bestAction = validActions[0];
            double maxVal = qTable[stateIdx, bestAction];

            for (int i = 1; i < validActions.Count; i++)
            {
                int action = validActions[i];
                if (qTable[stateIdx, action] > maxVal)
                {
                    maxVal = qTable[stateIdx, action];
                    bestAction = action;
                }
            }

            return bestAction;
        }
    }
}
