namespace LuckyMaze.Domain
{
    public class MazeCell
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        // True if there is a wall in this direction, False if open
        public bool North { get; set; } = true;
        public bool East { get; set; } = true;
        public bool South { get; set; } = true;
        public bool West { get; set; } = true;
    }
}
