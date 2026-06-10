using LuckyMaze.Domain;

namespace LuckyMaze.Application.Services
{
    public interface IMazeGenerator
    {
        Maze Generate(int width, int height, int exitCount = 2);
    }
}
