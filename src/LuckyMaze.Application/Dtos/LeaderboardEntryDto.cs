using System;

namespace LuckyMaze.Application.Dtos
{
    public record LeaderboardEntryDto(
        string DisplayName,
        decimal Balance,
        string? AvatarUrl);
}
