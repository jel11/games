using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Core;
using NarutoRoguelike.Entities;
using NarutoRoguelike.Systems;

namespace NarutoRoguelike.UI
{
    public class InventoryScreen
    {
        private readonly Texture2D   _pixel;
        private readonly SpriteFont? _font;

        private int  _scrollOffset;
        private const int RowHeight   = 18;
        private const int MaxVisible  = 20;

        public bool CloseRequested { get; private set; }

        // Action request set by Update; cleared after processing
        public enum PendingAction { None, Use, Drop, Equip }
        public PendingAction Action     { get; private set; }
        public Item?         ActionItem { get; private set; }

        public InventoryScreen(Texture2D pixel, SpriteFont? font)
        {
            _pixel = pixel;
            _font  = font;
        }

        // ── Input ─────────────────────────────────────────────────────────────────

        public void Update(InputHandler input, Player player)
        {
            CloseRequested = false;
            Action         = PendingAction.None;
            ActionItem     = null;

            if (input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.I) ||
                input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                CloseRequested = true;
                return;
            }

            int count = player.Inventory.Count;
            if (count == 0) return;

            if (input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                player.InventoryCursor = Math.Max(0, player.InventoryCursor - 1);
                if (player.InventoryCursor < _scrollOffset)
                    _scrollOffset = player.InventoryCursor;
            }
            if (input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                player.InventoryCursor = Math.Min(count - 1, player.InventoryCursor + 1);
                if (player.InventoryCursor >= _scrollOffset + MaxVisible)
                    _scrollOffset = player.InventoryCursor - MaxVisible + 1;
            }

            if (input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.Enter) ||
                input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.U))
            {
                ActionItem = player.Inventory[player.InventoryCursor];
                Action     = PendingAction.Use;
            }
            if (input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.D))
            {
                ActionItem = player.Inventory[player.InventoryCursor];
                Action     = PendingAction.Drop;
            }
            if (input.IsJustPressed(Microsoft.Xna.Framework.Input.Keys.E))
            {
                ActionItem = player.Inventory[player.InventoryCursor];
                Action     = PendingAction.Equip;
            }
        }

        // ── Rendering ─────────────────────────────────────────────────────────────

        public void Draw(SpriteBatch sb, Player player, int screenWidth, int screenHeight)
        {
            int panelW = 420;
            int panelH = 420;
            int panelX = (screenWidth  - panelW) / 2;
            int panelY = (screenHeight - panelH) / 2;
            var panel  = new Rectangle(panelX, panelY, panelW, panelH);

            // Background + border
            sb.Draw(_pixel, panel, Constants.UIBackground);
            MessageLog.DrawBorder(sb, _pixel, panel, Constants.UIHighlight, 2);

            DrawText(sb, "[ INVENTORY ]", panelX + 10, panelY + 8, Constants.UIHighlight);
            DrawText(sb, "U:Use  E:Equip  D:Drop  I/Esc:Close",
                         panelX + 10, panelY + 26, Constants.UIText);

            int listY = panelY + 48;

            if (player.Inventory.Count == 0)
            {
                DrawText(sb, "(empty)", panelX + 10, listY, Color.Gray);
                return;
            }

            int end = Math.Min(player.Inventory.Count, _scrollOffset + MaxVisible);
            for (int i = _scrollOffset; i < end; i++)
            {
                var item   = player.Inventory[i];
                int rowY   = listY + (i - _scrollOffset) * RowHeight;
                bool sel   = i == player.InventoryCursor;

                if (sel)
                    sb.Draw(_pixel, new Rectangle(panelX + 4, rowY - 1, panelW - 8, RowHeight), Constants.UIHighlight * 0.25f);

                char letter = (char)('a' + i);
                Color col   = sel ? Constants.UIHighlight : Constants.UIText;
                if (item.IsEquipped) col = Color.Gold;

                string tag    = item.IsEquipped ? "[E] " : "    ";
                string line   = $"{letter}) {tag}{item.Name}";
                DrawText(sb, line, panelX + 10, rowY, col);
            }

            // Item description for selected item
            if (player.InventoryCursor < player.Inventory.Count)
            {
                var sel = player.Inventory[player.InventoryCursor];
                int descY = panelY + panelH - 60;
                DrawText(sb, sel.Description.Length > 0 ? sel.Description : sel.Name,
                         panelX + 10, descY, Color.LightGray);
                string stats = $"ATK:{sel.AttackBonus:+#;-#;0}  DEF:{sel.DefenseBonus:+#;-#;0}  HP:{sel.HPBonus:+#;-#;0}  CK:{sel.ChakraBonus:+#;-#;0}";
                DrawText(sb, stats, panelX + 10, descY + 18, Color.LightYellow);
            }
        }

        private void DrawText(SpriteBatch sb, string text, int x, int y, Color color)
        {
            if (_font == null) return;
            sb.DrawString(_font, text, new Vector2(x, y), color);
        }
    }
}
