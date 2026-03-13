using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoRoguelike.Core;

namespace NarutoRoguelike.World
{
    public static class MapGenerator
    {
        private static Random _rng = null!;

        // ── Public Entry Point ────────────────────────────────────────────────────

        public static Map Generate(int width, int height, int seed)
        {
            _rng = new Random(seed);
            var map = new Map(width, height);

            var root = new BspNode(new Rectangle(1, 1, width - 2, height - 2));
            Split(root, 0);
            CreateRooms(root, map);
            ConnectRooms(map);
            PlaceStairs(map);

            return map;
        }

        // ── BSP Node ──────────────────────────────────────────────────────────────

        private sealed class BspNode
        {
            public readonly Rectangle Area;
            public BspNode? Left, Right;
            public Room?    Room;

            public bool IsLeaf => Left == null && Right == null;
            public BspNode(Rectangle area) { Area = area; }
        }

        // ── BSP Split ─────────────────────────────────────────────────────────────

        private static void Split(BspNode node, int depth)
        {
            if (depth > 5) return;

            int w = node.Area.Width;
            int h = node.Area.Height;

            bool canSplitH = h >= Constants.BSP_MIN_SPLIT * 2;
            bool canSplitV = w >= Constants.BSP_MIN_SPLIT * 2;

            if (!canSplitH && !canSplitV) return;

            // Prefer splitting the longer dimension
            bool splitH;
            if (!canSplitH)       splitH = false;
            else if (!canSplitV)  splitH = true;
            else                  splitH = h >= w * 1.25 ? true  :
                                           w >= h * 1.25 ? false :
                                           _rng.Next(2) == 0;

            int minSplit = Constants.BSP_MIN_SPLIT;

            if (splitH)
            {
                int splitPos = _rng.Next(minSplit, h - minSplit + 1);
                node.Left  = new BspNode(new Rectangle(node.Area.X, node.Area.Y,           w, splitPos));
                node.Right = new BspNode(new Rectangle(node.Area.X, node.Area.Y + splitPos, w, h - splitPos));
            }
            else
            {
                int splitPos = _rng.Next(minSplit, w - minSplit + 1);
                node.Left  = new BspNode(new Rectangle(node.Area.X,           node.Area.Y, splitPos,     h));
                node.Right = new BspNode(new Rectangle(node.Area.X + splitPos, node.Area.Y, w - splitPos, h));
            }

            Split(node.Left,  depth + 1);
            Split(node.Right, depth + 1);
        }

        // ── Room Creation ─────────────────────────────────────────────────────────

        private static void CreateRooms(BspNode node, Map map)
        {
            if (node.IsLeaf)
            {
                int maxW = Math.Min(node.Area.Width  - 2, Constants.BSP_MAX_ROOM_SIZE);
                int maxH = Math.Min(node.Area.Height - 2, Constants.BSP_MAX_ROOM_SIZE);
                int minW = Math.Min(Constants.BSP_MIN_ROOM_SIZE, maxW);
                int minH = Math.Min(Constants.BSP_MIN_ROOM_SIZE, maxH);

                if (maxW < minW || maxH < minH) return;

                int rw = _rng.Next(minW, maxW + 1);
                int rh = _rng.Next(minH, maxH + 1);
                int rx = node.Area.X + _rng.Next(1, node.Area.Width  - rw);
                int ry = node.Area.Y + _rng.Next(1, node.Area.Height - rh);

                var room = new Room(rx, ry, rw, rh);
                node.Room = room;
                map.Rooms.Add(room);

                for (int x = rx; x < rx + rw; x++)
                for (int y = ry; y < ry + rh; y++)
                    map.SetTile(x, y, Tile.CreateFloor());
            }
            else
            {
                if (node.Left  != null) CreateRooms(node.Left,  map);
                if (node.Right != null) CreateRooms(node.Right, map);
            }
        }

        // ── Corridor Connection ───────────────────────────────────────────────────

        private static void ConnectRooms(Map map)
        {
            if (map.Rooms.Count < 2) return;

            // Sort rooms left-to-right, top-to-bottom for predictable corridors
            map.Rooms.Sort((a, b) =>
                a.Center.X != b.Center.X
                    ? a.Center.X.CompareTo(b.Center.X)
                    : a.Center.Y.CompareTo(b.Center.Y));

            for (int i = 0; i < map.Rooms.Count - 1; i++)
                CarveCorridorL(map, map.Rooms[i].Center, map.Rooms[i + 1].Center);
        }

        private static void CarveCorridorL(Map map, Point from, Point to)
        {
            if (_rng.Next(2) == 0)
            {
                CarveHLine(map, from.X, to.X, from.Y);
                CarveVLine(map, from.Y, to.Y, to.X);
            }
            else
            {
                CarveVLine(map, from.Y, to.Y, from.X);
                CarveHLine(map, from.X, to.X, to.Y);
            }
        }

        private static void CarveHLine(Map map, int x1, int x2, int y)
        {
            int minX = Math.Min(x1, x2), maxX = Math.Max(x1, x2);
            for (int x = minX; x <= maxX; x++)
                if (map.IsInBounds(x, y)) map.SetTile(x, y, Tile.CreateFloor());
        }

        private static void CarveVLine(Map map, int y1, int y2, int x)
        {
            int minY = Math.Min(y1, y2), maxY = Math.Max(y1, y2);
            for (int y = minY; y <= maxY; y++)
                if (map.IsInBounds(x, y)) map.SetTile(x, y, Tile.CreateFloor());
        }

        // ── Stairs Placement ──────────────────────────────────────────────────────

        private static void PlaceStairs(Map map)
        {
            if (map.Rooms.Count == 0) return;

            map.StairsUpPos   = map.Rooms[0].Center;
            map.StairsDownPos = map.Rooms[map.Rooms.Count - 1].Center;

            map.SetTile(map.StairsUpPos.X,   map.StairsUpPos.Y,   Tile.CreateStairsUp());
            map.SetTile(map.StairsDownPos.X, map.StairsDownPos.Y, Tile.CreateStairsDown());
        }

        // ── Enemy / Item Placement Helpers ────────────────────────────────────────

        /// <summary>Returns a list of random floor positions, one per room (skipping room 0).</summary>
        public static List<Point> GetEnemySpawnPoints(Map map, int maxEnemies)
        {
            var points = new List<Point>();
            int count  = Math.Min(maxEnemies, map.Rooms.Count - 1);

            for (int i = 1; i <= count; i++)
            {
                var room = map.Rooms[i];
                int x = _rng.Next(room.Left, room.Right);
                int y = _rng.Next(room.Top,  room.Bottom);
                points.Add(new Point(x, y));
            }
            return points;
        }

        /// <summary>Returns random floor positions for items (one per 3 rooms).</summary>
        public static List<Point> GetItemSpawnPoints(Map map)
        {
            var points = new List<Point>();
            for (int i = 1; i < map.Rooms.Count; i += 3)
            {
                var room = map.Rooms[i];
                int x = _rng.Next(room.Left, room.Right);
                int y = _rng.Next(room.Top,  room.Bottom);
                points.Add(new Point(x, y));
            }
            return points;
        }
    }
}
