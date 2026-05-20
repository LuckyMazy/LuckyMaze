using LuckyMaze.Application.Interfaces;
using LuckyMaze.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LuckyMaze.API.Hubs;

[Authorize]
public class GameSyncHub : Hub<IGameClient>
{
    private readonly GameManager _gameManager;
    private readonly ConnectionManager _connectionManager;

    public GameSyncHub(GameManager gameManager, ConnectionManager connectionManager)
    {
        _gameManager = gameManager;
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var existingConnection = _connectionManager.GetConnectionId(userId);
            if (!string.IsNullOrEmpty(existingConnection))
            {
                // Kick the old connection
                await Clients.Client(existingConnection).Kicked();
            }

            _connectionManager.AddConnection(userId, Context.ConnectionId);

            _gameManager.EnsureActiveSession();
            _gameManager.CurrentSession.AddPlayer(userId);

            // Subscribe to state changes if not already subscribed somewhere else,
            // but for simplicity, we can broadcast via hub context from outside, or 
            // since we are just inside the hub right now, let's keep it simple.
            // Ideally GameManager should use IHubContext<GameSyncHub, IGameClient> to broadcast.
            // For now, let's just sync state.
            await Clients.Caller.ReceiveGameState(_gameManager.CurrentSession.CurrentState.ToString());
        }

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            var currentConnection = _connectionManager.GetConnectionId(userId);
            if (currentConnection == Context.ConnectionId)
            {
                _connectionManager.RemoveConnection(Context.ConnectionId);
            }
        }
        return base.OnDisconnectedAsync(exception);
    }

    public Task SetReady()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            _gameManager.CurrentSession.SetPlayerReady(userId);
        }
        return Task.CompletedTask;
    }

    public Task PlaceBet(string exitId)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            _gameManager.CurrentSession.PlaceBet(userId, exitId);
        }
        return Task.CompletedTask;
    }
}