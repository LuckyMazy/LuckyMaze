namespace LuckyMaze.Application.Interfaces;

public interface IGameClient
{
    Task ReceiveMaze();
    Task ReceiveMove();
    Task ReceiveGameState(string state);
    Task Kicked();
}