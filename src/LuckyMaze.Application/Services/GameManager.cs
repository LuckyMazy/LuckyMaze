using LuckyMaze.Domain.Entities;

namespace LuckyMaze.Application.Services;

public class GameManager
{
    // For simplicity, manage a single active game session for now.
    // This could easily be expanded to a dictionary of sessions.
    public GameSession CurrentSession { get; private set; }

    public GameManager()
    {
        CurrentSession = new GameSession();
    }

    public void EnsureActiveSession()
    {
        if (CurrentSession == null || CurrentSession.CurrentState == Domain.Enums.GameState.GameFinished)
        {
            CurrentSession = new GameSession();
        }
    }
}
