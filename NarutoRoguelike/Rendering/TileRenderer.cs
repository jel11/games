using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Core;
using NarutoRoguelike.Managers;
using NarutoRoguelike.World;

namespace NarutoRoguelike.Rendering
{
    public class TileRenderer
    {
        private readonly NarutoContentManager _content;

        public TileRenderer(NarutoContentManager content)
        {
            _content = content;
        }

        public void Draw(SpriteBatch sb, Map map, Camera camera)
        {
            int ts = Constants.TILE_SIZE;

            // Viewport tile range
            int startX = (int)(camera.WorldPosition.X / ts) - 1;
            int startY = (int)(camera.WorldPosition.Y / ts) - 1;
            int endX   = startX + Constants.VIEWPORT_TILES_X + 2;
            int endY   = startY + Constants.VIEWPORT_TILES_Y + 2;

            startX = System.Math.Max(0, startX);
            startY = System.Math.Max(0, startY);
            endX   = System.Math.Min(map.Width  - 1, endX);
            endY   = System.Math.Min(map.Height - 1, endY);

            var floorTex = _content.GetTexture("floor");
            var wallTex  = _content.GetTexture("wall");
            var pixel    = _content.GetTexture("pixel");

            for (int x = startX; x <= endX; x++)
            for (int y = startY; y <= endY; y++)
            {
                ref var tile = ref map.GetTile(x, y);

                if (!tile.Explored) continue;    // never seen → draw nothing

                Vector2 screenPos = camera.WorldToScreen(x * ts, y * ts);
                var     destRect  = new Rectangle((int)screenPos.X, (int)screenPos.Y, ts, ts);

                if (tile.Type == TileType.Wall)
                {
                    Color col = tile.Visible ? Constants.WallLitColor : Constants.WallColor * 0.4f;
                    sb.Draw(wallTex ?? pixel, destRect, col);
                }
                else
                {
                    // Floor-variant tiles: stairs, door, water all use floor texture with tint
                    Color col = tile.Visible ? GetFloorColor(tile.Type) : Constants.FloorColor * 0.25f;
                    sb.Draw(floorTex ?? pixel, destRect, col);

                    // Overlay symbol for special tiles (using pixel rects)
                    if (tile.Visible && pixel != null)
                        DrawTileOverlay(sb, pixel, tile.Type, destRect);
                }
            }
        }

        private static Color GetFloorColor(TileType type) => type switch
        {
            TileType.Floor      => Constants.FloorLitColor,
            TileType.Door       => new Color(140, 100, 40),
            TileType.StairsDown => new Color(200, 180, 80),
            TileType.StairsUp   => new Color(80, 200, 80),
            TileType.Water      => new Color(40, 80, 200),
            _                   => Constants.FloorLitColor
        };

        private static void DrawTileOverlay(SpriteBatch sb, Texture2D pixel, TileType type, Rectangle dest)
        {
            // Draw a small indicator in the center of the tile
            if (type == TileType.StairsDown || type == TileType.StairsUp)
            {
                int cx = dest.X + dest.Width  / 2 - 4;
                int cy = dest.Y + dest.Height / 2 - 4;
                Color c = type == TileType.StairsDown ? Color.Gold : Color.LightGreen;
                sb.Draw(pixel, new Rectangle(cx, cy, 8, 8), c);
            }
        }
    }
}
