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

            var gridMap = cellsList.ToDictionary(c => new Coordinate(c.X, c.Y));
            var exitCoords = exitsList.Select(e => new Coordinate(e.X, e.Y)).ToHashSet();

            double[,] qTable = TrainAgent(width, height, gridMap, exitCoords, start);

            // A small amount of randomness keeps the run unpredictable for spectators.
            return SimulatePath(width, height, gridMap, exitCoords, start, qTable, epsilon: 0.15);
        }

        private double[,] TrainAgent(int width, int height, Dictionary<Coordinate, MazeCell> gridMap, HashSet<Coordinate> exits, Coordinate start)
        {
            int numStates = width * height;
            int numActions = 4; // North, East, South, West
            double[,] qTable = new double[numStates, numActions];

            double alpha = 0.2; // Learning rate
            double gamma = 0.9; // Discount factor
            double epsilon = 0.3; // Exploration rate (decayed each episode)
            int episodes = 1500;
            int maxSteps = width * height * 2;

            for (int episode = 0; episode < episodes; episode++)
            {
                // Bias toward the real start, but sometimes begin elsewhere to generalize.
                Coordinate current = _random.NextDouble() < 0.3 ? start : GetRandomState(width, height, exits);
                int steps = 0;

                while (!exits.Contains(current) && steps < maxSteps)
                {
                    int stateIdx = GetStateIndex(current, width);
                    var validActions = GetValidActions(current, gridMap);

                    if (validActions.Count == 0)
                        break;

                    int action = _random.NextDouble() < epsilon
                        ? validActions[_random.Next(validActions.Count)]
                        : GetBestAction(stateIdx, validActions, qTable);

                    Coordinate next = Move(current, action);
                    double reward = exits.Contains(next) ? 100.0 : -1.0;

                    int nextStateIdx = GetStateIndex(next, width);
                    var nextValidActions = GetValidActions(next, gridMap);

                    double maxNextQ = 0.0;
                    if (nextValidActions.Count > 0 && !exits.Contains(next))
                    {
                        maxNextQ = nextValidActions.Max(a => qTable[nextStateIdx, a]);
                    }

                    qTable[stateIdx, action] += alpha * (reward + gamma * maxNextQ - qTable[stateIdx, action]);

                    current = next;
                    steps++;
                }

                epsilon = Math.Max(0.01, epsilon * 0.995);
            }

            return qTable;
        }

        private List<Coordinate> SimulatePath(int width, int height, Dictionary<Coordinate, MazeCell> gridMap, HashSet<Coordinate> exits, Coordinate start, double[,] qTable, double epsilon)
        {
            var path = new List<Coordinate> { start };
            Coordinate current = start;
            int maxSteps = width * height * 4; // Guard against infinite loops
            int steps = 0;

            while (!exits.Contains(current) && steps < maxSteps)
            {
                int stateIdx = GetStateIndex(current, width);
                var validActions = GetValidActions(current, gridMap);

                if (validActions.Count == 0)
                    break;

                int action = _random.NextDouble() < epsilon
                    ? validActions[_random.Next(validActions.Count)]
                    : GetBestAction(stateIdx, validActions, qTable);

                current = Move(current, action);
                path.Add(current);
                steps++;
            }

            return path;
        }

        private List<int> GetValidActions(Coordinate coord, Dictionary<Coordinate, MazeCell> gridMap)
        {
            var valid = new List<int>();
            if (!gridMap.TryGetValue(coord, out var cell))
                return valid;

            if (!cell.North) valid.Add(0);
            if (!cell.East) valid.Add(1);
            if (!cell.South) valid.Add(2);
            if (!cell.West) valid.Add(3);

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
