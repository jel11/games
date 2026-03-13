using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace NarutoRoguelike.World
{
    public class Map
    {
        private readonly Tile[,] _tiles;

        public int        Width         { get; }
        public int        Height        { get; }
        public List<Room> Rooms         { get; } = new();
        public Point      StairsDownPos { get; set; }
        public Point      StairsUpPos   { get; set; }

        public Map(int width, int height)
        {
            Width  = width;
            Height = height;
            _tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _tiles[x, y] = Tile.CreateWall();
        }

        // ── Accessors ─────────────────────────────────────────────────────────────

        public ref Tile GetTile(int x, int y) => ref _tiles[x, y];

        public bool IsInBounds(int x, int y)   => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsWalkable(int x, int y)   => IsInBounds(x, y) && _tiles[x, y].Walkable;
        public bool IsTransparent(int x, int y)=> IsInBounds(x, y) && _tiles[x, y].Transparent;
        public bool IsVisible(int x, int y)    => IsInBounds(x, y) && _tiles[x, y].Visible;
        public bool IsExplored(int x, int y)   => IsInBounds(x, y) && _tiles[x, y].Explored;

        public void SetTile(int x, int y, Tile tile)
        {
            if (IsInBounds(x, y)) _tiles[x, y] = tile;
        }

        // ── FOV – Recursive Shadowcasting ─────────────────────────────────────────

        public void ClearVisibility()
        {
            for (int x = 0; x < Width;  x++)
            for (int y = 0; y < Height; y++)
                _tiles[x, y].Visible = false;
        }

        public void ComputeFOV(int originX, int originY, int radius)
        {
            ClearVisibility();

            if (!IsInBounds(originX, originY)) return;

            _tiles[originX, originY].Visible  = true;
            _tiles[originX, originY].Explored = true;

            for (int octant = 0; octant < 8; octant++)
                ScanOctant(originX, originY, radius, 1, 1.0f, 0.0f, octant);
        }

        private void ScanOctant(int cx, int cy, int radius,
                                 int row, float startSlope, float endSlope, int octant)
        {
            if (startSlope < endSlope) return;
            float nextStartSlope = startSlope;

            for (int i = row; i <= radius; i++)
            {
                bool blocked = false;
                for (int dx = -i; dx <= 0; dx++)
                {
                    float lSlope = (dx - 0.5f) / (i + 0.5f);
                    float rSlope = (dx + 0.5f) / (i - 0.5f);

                    if (startSlope < rSlope) continue;
                    if (endSlope > lSlope)   break;

                    (int sx, int sy) = TransformOctant(dx, i, octant);
                    int nx = cx + sx;
                    int ny = cy + sy;

                    if (IsInBounds(nx, ny))
                    {
                        float dist = MathF.Sqrt(dx * dx + i * i);
                        if (dist <= radius)
                        {
                            _tiles[nx, ny].Visible  = true;
                            _tiles[nx, ny].Explored = true;
                        }

                        if (blocked)
                        {
                            if (!IsTransparent(nx, ny))
                            {
                                nextStartSlope = rSlope;
                            }
                            else
                            {
                                blocked    = false;
                                startSlope = nextStartSlope;
                            }
                        }
                        else if (!IsTransparent(nx, ny))
                        {
                            blocked = true;
                            nextStartSlope = rSlope;
                            ScanOctant(cx, cy, radius, i + 1, startSlope, lSlope, octant);
                        }
                    }
                }
                if (blocked) break;
            }
        }

        private static (int x, int y) TransformOctant(int col, int row, int octant) =>
            octant switch
            {
                0 => ( col,  row),
                1 => ( row,  col),
                2 => ( row, -col),
                3 => ( col, -row),
                4 => (-col, -row),
                5 => (-row, -col),
                6 => (-row,  col),
                7 => (-col,  row),
                _ => ( col,  row)
            };

        // ── Utility ───────────────────────────────────────────────────────────────

        /// <summary>Manhattan distance between two grid positions.</summary>
        public static int ManhattanDist(int x1, int y1, int x2, int y2)
            => Math.Abs(x1 - x2) + Math.Abs(y1 - y2);

        /// <summary>Chebyshev (8-directional) distance.</summary>
        public static int ChebyshevDist(int x1, int y1, int x2, int y2)
            => Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
    }
}
