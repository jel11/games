using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Components;
using NarutoRoguelike.Core;
using NarutoRoguelike.Entities;
using NarutoRoguelike.Managers;
using NarutoRoguelike.World;

namespace NarutoRoguelike.Rendering
{
    public class EntityRenderer
    {
        private readonly NarutoContentManager _content;

        private const int HealthBarWidth  = 28;
        private const int HealthBarHeight = 4;

        public EntityRenderer(NarutoContentManager content)
        {
            _content = content;
        }

        public void DrawEntities(SpriteBatch sb, Map map, Camera camera,
                                 Player player, List<Enemy> enemies, List<Item> groundItems)
        {
            var pixel = _content.GetTexture("pixel");

            // Ground items first (under entities)
            foreach (var item in groundItems)
            {
                if (!map.IsVisible(item.GridX, item.GridY)) continue;
                DrawEntity(sb, camera, item, _content.GetTexture("item") ?? pixel);
            }

            // Enemies
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive || !map.IsVisible(enemy.GridX, enemy.GridY)) continue;
                var tex = GetEnemyTexture(enemy.Rank);
                DrawEntity(sb, camera, enemy, tex ?? pixel);
                DrawHealthBar(sb, camera, enemy, pixel);
            }

            // Player (always drawn if alive)
            if (player.IsAlive)
            {
                var playerTex = _content.GetTexture("player") ?? pixel;
                DrawEntity(sb, camera, player, playerTex);

                // Summon indicator
                if (player.SummonActive && pixel != null)
                {
                    Vector2 sp = camera.WorldToScreen(
                        (player.GridX + 1) * Constants.TILE_SIZE,
                        player.GridY * Constants.TILE_SIZE);
                    sb.Draw(pixel, new Rectangle((int)sp.X, (int)sp.Y, Constants.TILE_SIZE, Constants.TILE_SIZE),
                            Color.Gold * 0.7f);
                }
            }
        }

        private void DrawEntity(SpriteBatch sb, Camera camera, Entity entity, Texture2D? tex)
        {
            if (tex == null) return;
            Vector2 screenPos = camera.WorldToScreen(entity.GridX * Constants.TILE_SIZE,
                                                     entity.GridY * Constants.TILE_SIZE);
            var rect = new Rectangle((int)screenPos.X, (int)screenPos.Y,
                                     Constants.TILE_SIZE, Constants.TILE_SIZE);
            sb.Draw(tex, rect, entity.RenderColor);
        }

        private void DrawHealthBar(SpriteBatch sb, Camera camera, Enemy enemy, Texture2D? pixel)
        {
            if (pixel == null) return;
            var health = enemy.GetComponent<HealthComponent>();
            if (health == null || health.Fraction >= 1f) return;

            Vector2 screenPos = camera.WorldToScreen(enemy.GridX * Constants.TILE_SIZE,
                                                     enemy.GridY * Constants.TILE_SIZE);
            int barX = (int)screenPos.X + (Constants.TILE_SIZE - HealthBarWidth)  / 2;
            int barY = (int)screenPos.Y - HealthBarHeight - 2;

            // Background
            sb.Draw(pixel, new Rectangle(barX, barY, HealthBarWidth, HealthBarHeight), Color.DarkRed);
            // Fill
            int fillW = (int)(HealthBarWidth * health.Fraction);
            if (fillW > 0)
                sb.Draw(pixel, new Rectangle(barX, barY, fillW, HealthBarHeight), Color.LimeGreen);
        }

        private Texture2D? GetEnemyTexture(EnemyRank rank) => rank switch
        {
            EnemyRank.Genin    => _content.GetTexture("genin"),
            EnemyRank.Chunin   => _content.GetTexture("chunin"),
            EnemyRank.Jonin    => _content.GetTexture("jonin"),
            EnemyRank.Akatsuki => _content.GetTexture("akatsuki"),
            _                  => _content.GetTexture("genin")
        };
    }
}
