using Microsoft.Xna.Framework;
using NarutoRoguelike.Entities;

namespace NarutoRoguelike.Core
{
    /// <summary>
    /// 2D camera that follows the player and provides world-to-screen coordinate transform.
    /// The camera is clamped to the map boundaries so the viewport never shows outside the map.
    /// </summary>
    public class Camera
    {
        /// <summary>Top-left world-space position of the viewport (in pixels).</summary>
        public Vector2 WorldPosition { get; private set; }

        private readonly int _viewportPixelWidth;
        private readonly int _viewportPixelHeight;
        private readonly int _mapPixelWidth;
        private readonly int _mapPixelHeight;

        public Camera(int viewportPixelWidth, int viewportPixelHeight,
                      int mapPixelWidth,      int mapPixelHeight)
        {
            _viewportPixelWidth  = viewportPixelWidth;
            _viewportPixelHeight = viewportPixelHeight;
            _mapPixelWidth       = mapPixelWidth;
            _mapPixelHeight      = mapPixelHeight;
        }

        // ── Follow ────────────────────────────────────────────────────────────────

        /// <summary>Centre the camera on the player's tile, clamped to map bounds.</summary>
        public void Follow(Player player)
        {
            int ts = Constants.TILE_SIZE;

            // Desired centre (pixels)
            float cx = player.GridX * ts + ts / 2f;
            float cy = player.GridY * ts + ts / 2f;

            // Top-left such that player is centred
            float x = cx - _viewportPixelWidth  / 2f;
            float y = cy - _viewportPixelHeight / 2f;

            // Clamp
            x = MathHelper.Clamp(x, 0, System.Math.Max(0, _mapPixelWidth  - _viewportPixelWidth));
            y = MathHelper.Clamp(y, 0, System.Math.Max(0, _mapPixelHeight - _viewportPixelHeight));

            WorldPosition = new Vector2(x, y);
        }

        // ── Coordinate transform ──────────────────────────────────────────────────

        /// <summary>Converts a world pixel position to screen pixel position.</summary>
        public Vector2 WorldToScreen(float worldX, float worldY)
            => new Vector2(worldX - WorldPosition.X, worldY - WorldPosition.Y);

        /// <summary>Converts a world pixel position to screen pixel position.</summary>
        public Vector2 WorldToScreen(int worldX, int worldY)
            => WorldToScreen((float)worldX, (float)worldY);

        /// <summary>Converts a screen pixel position back to world pixel position.</summary>
        public Vector2 ScreenToWorld(float screenX, float screenY)
            => new Vector2(screenX + WorldPosition.X, screenY + WorldPosition.Y);

        // ── Visibility test ───────────────────────────────────────────────────────

        /// <summary>Returns true if the tile at (gridX, gridY) is within the current viewport.</summary>
        public bool IsTileVisible(int gridX, int gridY)
        {
            int ts  = Constants.TILE_SIZE;
            float wx = gridX * ts;
            float wy = gridY * ts;
            return wx + ts >= WorldPosition.X && wx <= WorldPosition.X + _viewportPixelWidth  &&
                   wy + ts >= WorldPosition.Y && wy <= WorldPosition.Y + _viewportPixelHeight;
        }
    }
}
