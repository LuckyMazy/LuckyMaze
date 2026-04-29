using LuckyMaze.Infrastructure.Dtos;

namespace LuckyMaze.Infrastructure.Services
{
    public interface IOidcService
    {
        Task<OidcUser?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}
