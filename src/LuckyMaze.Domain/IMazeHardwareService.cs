using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Domain
{
    public interface IMazeHardwareService
    {
        Task InitializeAsync(Maze maze);
        Task ShowStepAsync(int x, int y, Direction direction);
        Task FlashWinnerAsync(string exitName);
        Task ResetAsync();
    }
}
