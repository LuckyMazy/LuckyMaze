using System.Collections.Concurrent;

namespace LuckyMaze.Application.Services;

public class ConnectionManager
{
    // Map UserId -> ConnectionId
    private readonly ConcurrentDictionary<string, string> _userConnections = new();

    public void AddConnection(string userId, string connectionId)
    {
        _userConnections[userId] = connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        var item = _userConnections.FirstOrDefault(kvp => kvp.Value == connectionId);
        if (!string.IsNullOrEmpty(item.Key))
        {
            _userConnections.TryRemove(item.Key, out _);
        }
    }

    public string? GetConnectionId(string userId)
    {
        _userConnections.TryGetValue(userId, out var connectionId);
        return connectionId;
    }
}
