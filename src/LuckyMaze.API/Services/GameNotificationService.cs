using Microsoft.AspNetCore.SignalR;
using LuckyMaze.API.Hubs;
using LuckyMaze.Application.Services;

namespace LuckyMaze.API.Services
{
    public class GameNotificationService(IHubContext<GameHub> hubContext) : IGameNotificationService
    {
        public async Task BroadcastGameStateAsync(object gameState)
        {
            await hubContext.Clients.All.SendAsync("ReceiveGameState", gameState);
        }

        public async Task BroadcastCountdownTickAsync(int secondsRemaining)
        {
            await hubContext.Clients.All.SendAsync("CountdownTick", secondsRemaining);
        }

        public async Task BroadcastMazeGeneratedAsync(object mazeData)
        {
            await hubContext.Clients.All.SendAsync("MazeGenerated", mazeData);
        }

        public async Task BroadcastAiStepAsync(int x, int y, string direction)
        {
            await hubContext.Clients.All.SendAsync("AiStep", x, y, direction);
        }

        public async Task BroadcastGameFinishedAsync(string winningExit, object payouts)
        {
            await hubContext.Clients.All.SendAsync("GameFinished", winningExit, payouts);
        }
    }
}
