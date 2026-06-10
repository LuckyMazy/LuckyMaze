using System.Threading.Tasks;

namespace LuckyMaze.Application.Services
{
    public interface IGameNotificationService
    {
        Task BroadcastGameStateAsync(object gameState);
        Task BroadcastCountdownTickAsync(int secondsRemaining);
        Task BroadcastMazeGeneratedAsync(object mazeData);
        Task BroadcastAiStepAsync(int x, int y, string direction);
        Task BroadcastGameFinishedAsync(string winningExit, object payouts);
    }
}
