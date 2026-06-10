using System;
using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Application.Dtos
{
    public record GameHistoryDto(
        Guid Id,
        GameState State,
        string? WinningExit,
        DateTime? StartedAt,
        DateTime? EndedAt);
}
