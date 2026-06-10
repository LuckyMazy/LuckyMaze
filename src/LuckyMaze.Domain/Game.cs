using System;
using LuckyMaze.Domain.Enums;

namespace LuckyMaze.Domain
{
    public class Game : BaseEntity
    {
        public GameState State { get; set; } = GameState.Idle;

        public Guid? MazeId { get; set; }
        public Maze? Maze { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public string? WinningExit { get; set; }
    }
}
