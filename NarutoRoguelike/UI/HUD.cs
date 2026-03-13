using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Core;
using NarutoRoguelike.Data;
using NarutoRoguelike.Entities;

namespace NarutoRoguelike.UI
{
    public class HUD
    {
        private readonly Texture2D  _pixel;
        private readonly SpriteFont? _font;

        // Layout constants
        private const int BarWidth  = 160;
        private const int BarHeight = 14;
        private const int Margin    = 8;
        private const int LabelW    = 50;

        public HUD(Texture2D pixel, SpriteFont? font)
        {
            _pixel = pixel;
            _font  = font;
        }

        public void Draw(SpriteBatch sb, Player player, int floor, int screenWidth, int screenHeight)
        {
            // ── HUD panel at bottom ───────────────────────────────────────────────
            var hudRect = new Rectangle(0, screenHeight - Constants.HUD_HEIGHT, screenWidth, Constants.HUD_HEIGHT);
            sb.Draw(_pixel, hudRect, Constants.UIBackground * 0.9f);
            MessageLog.DrawBorder(sb, _pixel, hudRect, Constants.UIBorder);

            int x = Margin;
            int y = hudRect.Y + Margin;

            // HP bar
            DrawLabel(sb, "HP", x, y);
            DrawBar(sb, x + LabelW, y, player.Health.Fraction,
                    Color.Lerp(Constants.HPBarLow, Constants.HPBarFull, player.Health.Fraction));
            DrawBarText(sb, $"{player.Health.CurrentHP}/{player.Health.MaxHP}", x + LabelW, y);

            y += BarHeight + 4;

            // Chakra bar
            DrawLabel(sb, "CK", x, y);
            DrawBar(sb, x + LabelW, y, player.Chakra.Fraction, Constants.ChakraBarColor);
            DrawBarText(sb, $"{player.Chakra.CurrentChakra}/{player.Chakra.MaxChakra}", x + LabelW, y);

            y += BarHeight + 4;

            // XP bar
            DrawLabel(sb, "XP", x, y);
            float xpFrac = player.XPToNextLevel > 0 ? (float)player.XP / player.XPToNextLevel : 0f;
            DrawBar(sb, x + LabelW, y, xpFrac, Constants.XPBarColor);
            DrawBarText(sb, $"{player.XP}/{player.XPToNextLevel}", x + LabelW, y);

            // Level / Floor info (right of bars)
            int infoX = x + LabelW + BarWidth + Margin * 2;
            int infoY = hudRect.Y + Margin;
            DrawText(sb, $"LVL {player.Level}", infoX, infoY, Constants.UIHighlight);
            DrawText(sb, $"Floor {floor}", infoX, infoY + 16, Constants.UIText);

            // Active jutsu list (if known)
            int jutsuX = infoX + 100;
            DrawText(sb, "Jutsu (1-7):", jutsuX, hudRect.Y + Margin, Constants.UIText);
            for (int i = 0; i < Math.Min(7, player.KnownJutsu.Count); i++)
            {
                var jDef = NarutoRoguelike.Data.JutsuDatabase.Get(player.KnownJutsu[i]);
                if (jDef == null) continue;
                bool selected = i == player.SelectedJutsuIndex;
                Color col = selected ? Constants.UIHighlight : Constants.UIText;
                bool canUse = player.Chakra.HasEnough(jDef.ChakraCost);
                if (!canUse) col = Color.Gray;
                DrawText(sb, $"{i + 1}:{jDef.Name} ({jDef.ChakraCost})", jutsuX, hudRect.Y + Margin + 16 + i * 12, col);
            }

            // Status effects
            int stX = jutsuX + 220;
            int stY = hudRect.Y + Margin;
            if (player.ShieldTurnsLeft > 0)
                DrawText(sb, $"[Shield {player.ShieldTurnsLeft}t]", stX, stY, Constants.ChakraBlue);
            if (player.SummonActive)
                DrawText(sb, $"[Summon {player.SummonTurnsLeft}t]", stX, stY + 14, Color.Gold);
        }

        // ── Drawing helpers ───────────────────────────────────────────────────────

        private void DrawBar(SpriteBatch sb, int x, int y, float fraction, Color fillColor)
        {
            fraction = Math.Clamp(fraction, 0f, 1f);
            // Background
            sb.Draw(_pixel, new Rectangle(x, y, BarWidth, BarHeight), Color.DarkGray);
            // Fill
            int fillW = (int)(BarWidth * fraction);
            if (fillW > 0)
                sb.Draw(_pixel, new Rectangle(x, y, fillW, BarHeight), fillColor);
            // Border
            MessageLog.DrawBorder(sb, _pixel, new Rectangle(x, y, BarWidth, BarHeight), Color.Gray);
        }

        private void DrawBarText(SpriteBatch sb, string text, int barX, int barY)
        {
            if (_font == null) return;
            var size = _font.MeasureString(text);
            float tx = barX + (BarWidth - size.X) / 2f;
            float ty = barY + (BarHeight - size.Y) / 2f;
            sb.DrawString(_font, text, new Vector2(tx, ty), Color.White);
        }

        private void DrawLabel(SpriteBatch sb, string label, int x, int y)
        {
            if (_font == null) return;
            sb.DrawString(_font, label, new Vector2(x, y), Constants.UIText);
        }

        private void DrawText(SpriteBatch sb, string text, int x, int y, Color color)
        {
            if (_font == null) return;
            sb.DrawString(_font, text, new Vector2(x, y), color);
        }
    }
}
