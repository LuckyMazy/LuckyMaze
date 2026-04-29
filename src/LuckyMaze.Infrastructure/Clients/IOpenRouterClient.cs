using LuckyMaze.Infrastructure.Dtos;

namespace LuckyMaze.Infrastructure.Clients
{
    public interface IOpenRouterClient
    {
        Task<List<AvailableModelDto>> GetModelsAsync(CancellationToken cancellationToken = default);
    }
}
