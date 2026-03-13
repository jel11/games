namespace NarutoRoguelike.World
{
    public enum TileType
    {
        Wall,
        Floor,
        Door,
        StairsDown,
        StairsUp,
        Water
    }

    public struct Tile
    {
        public TileType Type;
        public bool Walkable;
        public bool Transparent;
        public bool Visible;    // currently in FOV
        public bool Explored;   // ever seen by player

        public static Tile CreateWall()       => new Tile { Type = TileType.Wall,       Walkable = false, Transparent = false };
        public static Tile CreateFloor()      => new Tile { Type = TileType.Floor,      Walkable = true,  Transparent = true  };
        public static Tile CreateDoor()       => new Tile { Type = TileType.Door,       Walkable = true,  Transparent = false };
        public static Tile CreateStairsDown() => new Tile { Type = TileType.StairsDown, Walkable = true,  Transparent = true  };
        public static Tile CreateStairsUp()   => new Tile { Type = TileType.StairsUp,   Walkable = true,  Transparent = true  };
        public static Tile CreateWater()      => new Tile { Type = TileType.Water,      Walkable = false, Transparent = true  };
    }
}
