using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NarutoRoguelike.Core;
using NarutoRoguelike.Managers;

namespace NarutoRoguelike.UI
{
    // ── Shared menu helper ────────────────────────────────────────────────────────

    public abstract class MenuBase
    {
        protected readonly Texture2D   _pixel;
        protected readonly SpriteFont? _font;
        protected readonly int         _screenW;
        protected readonly int         _screenH;

        protected string[] Options = Array.Empty<string>();
        protected int      Cursor;

        public int  SelectedOption   { get; protected set; } = -1;
        public bool SelectionMade    { get; protected set; }

        protected MenuBase(Texture2D pixel, SpriteFont? font, int screenW, int screenH)
        {
            _pixel   = pixel;
            _font    = font;
            _screenW = screenW;
            _screenH = screenH;
        }

        public virtual void Update(InputHandler input)
        {
            SelectionMade = false;

            if (input.IsJustPressed(Keys.Up))
                Cursor = (Cursor - 1 + Options.Length) % Options.Length;
            if (input.IsJustPressed(Keys.Down))
                Cursor = (Cursor + 1) % Options.Length;
            if (input.IsJustPressed(Keys.Enter) || input.IsJustPressed(Keys.Space))
            {
                SelectedOption = Cursor;
                SelectionMade  = true;
            }
        }

        protected void DrawCenteredText(SpriteBatch sb, string text, int y, Color color, float scale = 1f)
        {
            if (_font == null) return;
            var size = _font.MeasureString(text) * scale;
            float x  = (_screenW - size.X) / 2f;
            sb.DrawString(_font, text, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        protected void DrawOption(SpriteBatch sb, string text, int y, bool selected)
        {
            Color col = selected ? Constants.UIHighlight : Constants.UIText;
            string line = selected ? $"> {text} <" : $"  {text}  ";
            DrawCenteredText(sb, line, y, col);
        }
    }

    // ── Main Menu ─────────────────────────────────────────────────────────────────

    public class MainMenuScreen : MenuBase
    {
        public enum Choice { NewGame = 0, Continue = 1, Quit = 2 }

        public MainMenuScreen(Texture2D pixel, SpriteFont? font, int w, int h)
            : base(pixel, font, w, h)
        {
            Options = new[] { "New Game", "Continue", "Quit" };
        }

        public void Draw(SpriteBatch sb)
        {
            // Dim background
            sb.Draw(_pixel, new Rectangle(0, 0, _screenW, _screenH), Color.Black * 0.8f);

            // Title
            DrawCenteredText(sb, "NARUTO", _screenH / 4,      Constants.NarutoOrange, 2.5f);
            DrawCenteredText(sb, "ROGUELIKE", _screenH / 4 + 60, Constants.NarutoOrange, 1.8f);
            DrawCenteredText(sb, "A dungeon-crawling ninja adventure",
                                 _screenH / 4 + 110, Constants.UIText);

            int optionY = _screenH / 2 + 20;
            for (int i = 0; i < Options.Length; i++)
            {
                DrawOption(sb, Options[i], optionY + i * 32, i == Cursor);
            }

            DrawCenteredText(sb, "Arrow keys: navigate   Enter: select",
                                 _screenH - 60, Color.DimGray);
        }
    }

    // ── Pause Menu ────────────────────────────────────────────────────────────────

    public class PauseMenuScreen : MenuBase
    {
        public enum Choice { Resume = 0, SaveQuit = 1, QuitNoSave = 2 }

        public PauseMenuScreen(Texture2D pixel, SpriteFont? font, int w, int h)
            : base(pixel, font, w, h)
        {
            Options = new[] { "Resume", "Save & Quit", "Quit Without Saving" };
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_pixel, new Rectangle(0, 0, _screenW, _screenH), Color.Black * 0.6f);

            int panelW = 320, panelH = 200;
            int panelX = (_screenW - panelW) / 2;
            int panelY = (_screenH - panelH) / 2;
            sb.Draw(_pixel, new Rectangle(panelX, panelY, panelW, panelH), Constants.UIBackground);
            MessageLog.DrawBorder(sb, _pixel, new Rectangle(panelX, panelY, panelW, panelH),
                                  Constants.UIBorder, 2);

            DrawCenteredText(sb, "-- PAUSED --", panelY + 20, Constants.UIHighlight);
            for (int i = 0; i < Options.Length; i++)
                DrawOption(sb, Options[i], panelY + 60 + i * 32, i == Cursor);
        }
    }

    // ── Game Over Screen ──────────────────────────────────────────────────────────

    public class GameOverScreen : MenuBase
    {
        private readonly int _floor;
        private readonly int _level;

        public enum Choice { Retry = 0, MainMenu = 1, Quit = 2 }

        public GameOverScreen(Texture2D pixel, SpriteFont? font, int w, int h, int floor, int level)
            : base(pixel, font, w, h)
        {
            Options = new[] { "Try Again", "Main Menu", "Quit" };
            _floor  = floor;
            _level  = level;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_pixel, new Rectangle(0, 0, _screenW, _screenH), Color.Black * 0.85f);

            DrawCenteredText(sb, "YOU DIED", _screenH / 3,      Color.DarkRed, 2.5f);
            DrawCenteredText(sb, $"Reached Floor {_floor} at Level {_level}",
                                 _screenH / 3 + 80, Constants.UIText);

            int optionY = _screenH / 2 + 40;
            for (int i = 0; i < Options.Length; i++)
                DrawOption(sb, Options[i], optionY + i * 32, i == Cursor);
        }
    }

    // ── Victory Screen ────────────────────────────────────────────────────────────

    public class VictoryScreen : MenuBase
    {
        public enum Choice { MainMenu = 0, Quit = 1 }

        public VictoryScreen(Texture2D pixel, SpriteFont? font, int w, int h)
            : base(pixel, font, w, h)
        {
            Options = new[] { "Main Menu", "Quit" };
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_pixel, new Rectangle(0, 0, _screenW, _screenH), Color.Black * 0.8f);

            DrawCenteredText(sb, "VICTORY!", _screenH / 3,       Color.Gold, 2.5f);
            DrawCenteredText(sb, "You have defeated all enemies and escaped!",
                                 _screenH / 3 + 80, Constants.UIText);

            int optionY = _screenH / 2 + 40;
            for (int i = 0; i < Options.Length; i++)
                DrawOption(sb, Options[i], optionY + i * 32, i == Cursor);
        }
    }
}
