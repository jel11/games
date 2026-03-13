using Microsoft.Xna.Framework;

namespace NarutoRoguelike.World
{
    public class Room
    {
        public Rectangle Bounds    { get; }
        public bool      Connected { get; set; }

        public int   Left   => Bounds.Left;
        public int   Right  => Bounds.Right;
        public int   Top    => Bounds.Top;
        public int   Bottom => Bounds.Bottom;
        public int   Width  => Bounds.Width;
        public int   Height => Bounds.Height;
        public Point Center => new Point(Bounds.X + Bounds.Width / 2, Bounds.Y + Bounds.Height / 2);

        public Room(int x, int y, int width, int height)
        {
            Bounds = new Rectangle(x, y, width, height);
        }

        /// <summary>Returns true if the rooms overlap (with 1-tile padding).</summary>
        public bool Overlaps(Room other)
        {
            var inflated = new Rectangle(
                other.Bounds.X - 1,
                other.Bounds.Y - 1,
                other.Bounds.Width  + 2,
                other.Bounds.Height + 2);
            return Bounds.Intersects(inflated);
        }
    }
}
