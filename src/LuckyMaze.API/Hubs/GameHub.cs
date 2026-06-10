using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LuckyMaze.Application.Services;

namespace LuckyMaze.API.Hubs
{
    [Authorize]
    public class GameHub(GameManager gameManager) : Hub
    {
        private string? GetExternalId()
        {
            return Context.User?.FindFirst("sub")?.Value 
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public override async Task OnConnectedAsync()
        {
            var externalId = GetExternalId();
            if (string.IsNullOrEmpty(externalId))
            {
                Context.Abort();
                return;
            }

            await gameManager.PlayerConnectedAsync(externalId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await gameManager.PlayerDisconnectedAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ToggleReady(bool isReady)
        {
            var externalId = GetExternalId();
            if (string.IsNullOrEmpty(externalId)) return;

            await gameManager.ToggleReadyAsync(externalId, isReady);
        }

        public async Task PlaceBet(string exitName, decimal amount)
        {
            var externalId = GetExternalId();
            if (string.IsNullOrEmpty(externalId)) return;

            await gameManager.PlaceBetAsync(externalId, exitName, amount);
        }
    }
}
