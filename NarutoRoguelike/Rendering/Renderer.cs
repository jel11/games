using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Core;
using NarutoRoguelike.Entities;
using NarutoRoguelike.Managers;
using NarutoRoguelike.UI;
using NarutoRoguelike.World;

namespace NarutoRoguelike.Rendering
{
    /// <summary>
    /// Master draw pipeline.  Call <see cref="DrawWorld"/> once per frame from PlayingState.
    /// </summary>
    public class Renderer
    {
        private readonly SpriteBatch           _sb;
        private readonly NarutoContentManager  _content;
        private readonly GraphicsDevice        _gd;

        public readonly TileRenderer   Tiles;
        public readonly EntityRenderer Entities;
        public readonly ParticleEmitter Particles;

        private readonly HUD             _hud;
        private readonly InventoryScreen _inventory;

        public Renderer(SpriteBatch sb, NarutoContentManager content, GraphicsDevice gd)
        {
            _sb      = sb;
            _content = content;
            _gd      = gd;

            var pixel = content.GetTexture("pixel");
            var font  = content.GetFont("default");

            Tiles     = new TileRenderer(content);
            Entities  = new EntityRenderer(content);
            Particles = new ParticleEmitter(new System.Random());

            _hud       = new HUD(pixel, font);
            _inventory = new InventoryScreen(pixel, font);
        }

        // ── Public draw API ───────────────────────────────────────────────────────

        public void DrawWorld(
            Map         map,
            Camera      camera,
            Player      player,
            List<Enemy> enemies,
            List<Item>  groundItems,
            MessageLog  log,
            int         floor,
            GameTime    gameTime)
        {
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                      SamplerState.PointClamp, null, null, null, null);

            // 1. Map tiles
            Tiles.Draw(_sb, map, camera);

            // 2. Entities (items → enemies → player)
            Entities.DrawEntities(_sb, map, camera, player, enemies, groundItems);

            // 3. Particles
            Particles.Draw(_sb, _content.GetTexture("pixel"), camera.WorldPosition);

            // 4. HUD (bottom bar)
            _hud.Draw(_sb, player, floor, Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);

            // 5. Message log (right panel)
            int logX = Constants.SCREEN_WIDTH  - Constants.LOG_WIDTH;
            int logH = Constants.SCREEN_HEIGHT - Constants.HUD_HEIGHT;
            log.DrawPanel(_sb, _content.GetFont("default"),
                          _content.GetTexture("pixel"),
                          new Rectangle(logX, 0, Constants.LOG_WIDTH, logH));

            _sb.End();
        }

        public void DrawMenu(System.Action<SpriteBatch> drawAction)
        {
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                      SamplerState.PointClamp);
            drawAction(_sb);
            _sb.End();
        }

        // Expose HUD for direct use; expose inventory (needs log at runtime)
        public HUD             GetHUD()       => _hud;
        public InventoryScreen GetInventory() => _inventory;
    }
}
