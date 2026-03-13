using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Core;

namespace NarutoRoguelike.UI
{
    public class MessageLog
    {
        private readonly record struct LogEntry(string Text, Color Color);

        private readonly List<LogEntry> _messages = new();
        private readonly int            _maxMessages;

        public int Count => _messages.Count;

        public MessageLog(int maxMessages = Constants.LOG_MAX_MESSAGES)
        {
            _maxMessages = maxMessages;
        }

        // ── Writing ───────────────────────────────────────────────────────────────

        public void Add(string text, Color color)
        {
            _messages.Add(new LogEntry(text, color));
            if (_messages.Count > _maxMessages)
                _messages.RemoveAt(0);
        }

        public void Add(string text) => Add(text, Constants.UIText);

        // ── Rendering ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Draws the most recent messages inside <paramref name="bounds"/>.
        /// </summary>
        public void Draw(SpriteBatch sb, SpriteFont font, Rectangle bounds)
        {
            if (font == null) return;

            float lineHeight = font.LineSpacing;
            int   maxLines   = (int)(bounds.Height / lineHeight);

            // Draw from the bottom up
            int start = System.Math.Max(0, _messages.Count - maxLines);
            for (int i = start; i < _messages.Count; i++)
            {
                int   lineIndex = i - start;
                float y         = bounds.Y + lineIndex * lineHeight;
                var   entry     = _messages[i];
                sb.DrawString(font, entry.Text, new Vector2(bounds.X + 4, y), entry.Color);
            }
        }

        /// <summary>Draws a bordered panel background then the log messages.</summary>
        public void DrawPanel(SpriteBatch sb, SpriteFont? font, Texture2D pixel, Rectangle bounds)
        {
            // Background
            sb.Draw(pixel, bounds, Constants.UIBackground * 0.85f);
            // Border
            DrawBorder(sb, pixel, bounds, Constants.UIBorder);

            if (font != null)
                Draw(sb, font, new Rectangle(bounds.X, bounds.Y + 4, bounds.Width - 4, bounds.Height - 4));
        }

        // ── Utility ───────────────────────────────────────────────────────────────

        public static void DrawBorder(SpriteBatch sb, Texture2D pixel, Rectangle rect, Color color, int thickness = 1)
        {
            // Top
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            // Bottom
            sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
            // Left
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            // Right
            sb.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}
