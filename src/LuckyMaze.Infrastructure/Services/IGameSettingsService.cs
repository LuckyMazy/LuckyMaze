using System.Threading.Tasks;
using LuckyMaze.Domain;

namespace LuckyMaze.Infrastructure.Services
{
    public interface IGameSettingsService
    {
        Task<GameSettings> GetSettingsAsync();
        Task UpdateSettingsAsync(GameSettings settings);
    }
}
