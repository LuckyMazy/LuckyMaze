using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LuckyMaze.Domain;
using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Infrastructure.Services
{
    public class MockHardwareService(ILogger<MockHardwareService> logger) : IMazeHardwareService
    {
        public Task InitializeAsync(Maze maze)
        {
            logger.LogInformation("[HARDWARE MOCK] Initializing maze of size {Width}x{Height}. Exits: {Exits}", 
                maze.Width, maze.Height, maze.Exits);
            return Task.CompletedTask;
        }

        public Task ShowStepAsync(int x, int y, Direction direction)
        {
            logger.LogInformation("[HARDWARE MOCK] Showing step at ({X}, {Y}) moving {Direction}", x, y, direction);
            return Task.CompletedTask;
        }

        public Task FlashWinnerAsync(string exitName)
        {
            logger.LogInformation("[HARDWARE MOCK] Flashing winner: {ExitName}", exitName);
            return Task.CompletedTask;
        }

        public Task ResetAsync()
        {
            logger.LogInformation("[HARDWARE MOCK] Resetting hardware");
            return Task.CompletedTask;
        }
    }
}
