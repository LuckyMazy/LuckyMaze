using System;

namespace LuckyMaze.Domain
{
    public class Maze : BaseEntity
    {
        public int Width { get; set; }
        public int Height { get; set; }
        
        /// <summary>
        /// JSON representation of the maze cells, containing wall configurations.
        /// </summary>
        public required string GridData { get; set; }

        /// <summary>
        /// JSON representation of exit points, e.g., [{"X": 0, "Y": 2, "Name": "Exit A"}, {"X": 4, "Y": 0, "Name": "Exit B"}]
        /// </summary>
        public required string Exits { get; set; }
    }
}
