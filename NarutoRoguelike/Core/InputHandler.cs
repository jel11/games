using Microsoft.Xna.Framework.Input;

namespace NarutoRoguelike.Core
{
    /// <summary>
    /// Polls keyboard and gamepad each frame; exposes current/previous state helpers.
    /// </summary>
    public class InputHandler
    {
        private KeyboardState _current;
        private KeyboardState _previous;

        private GamePadState _padCurrent;
        private GamePadState _padPrevious;

        // ── Per-frame update ──────────────────────────────────────────────────────

        public void Update()
        {
            _previous    = _current;
            _current     = Keyboard.GetState();

            _padPrevious = _padCurrent;
            _padCurrent  = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
        }

        // ── Keyboard queries ──────────────────────────────────────────────────────

        public bool IsHeld(Keys key)         => _current.IsKeyDown(key);
        public bool IsJustPressed(Keys key)  => _current.IsKeyDown(key) && _previous.IsKeyUp(key);
        public bool IsJustReleased(Keys key) => _current.IsKeyUp(key)   && _previous.IsKeyDown(key);

        // ── Gamepad queries ───────────────────────────────────────────────────────

        public bool IsButtonHeld(Buttons btn)         => _padCurrent.IsButtonDown(btn);
        public bool IsButtonJustPressed(Buttons btn)  => _padCurrent.IsButtonDown(btn) && _padPrevious.IsButtonUp(btn);
        public bool IsButtonJustReleased(Buttons btn) => _padCurrent.IsButtonUp(btn)   && _padPrevious.IsButtonDown(btn);

        // ── Direction helpers (keyboard + gamepad) ────────────────────────────────

        /// <summary>Returns (dx, dy) from arrow/WASD or D-pad. (0,0) if no direction.</summary>
        public (int dx, int dy) GetMoveDirection()
        {
            if (IsJustPressed(Keys.Up)    || IsJustPressed(Keys.W) ||
                IsButtonJustPressed(Buttons.DPadUp)                 ||
                IsButtonJustPressed(Buttons.LeftThumbstickUp))
                return (0, -1);

            if (IsJustPressed(Keys.Down)  || IsJustPressed(Keys.S) ||
                IsButtonJustPressed(Buttons.DPadDown)               ||
                IsButtonJustPressed(Buttons.LeftThumbstickDown))
                return (0, 1);

            if (IsJustPressed(Keys.Left)  || IsJustPressed(Keys.A) ||
                IsButtonJustPressed(Buttons.DPadLeft)               ||
                IsButtonJustPressed(Buttons.LeftThumbstickLeft))
                return (-1, 0);

            if (IsJustPressed(Keys.Right) || IsJustPressed(Keys.D) ||
                IsButtonJustPressed(Buttons.DPadRight)              ||
                IsButtonJustPressed(Buttons.LeftThumbstickRight))
                return (1, 0);

            // Diagonal movement (numpad)
            if (IsJustPressed(Keys.NumPad7)) return (-1, -1);
            if (IsJustPressed(Keys.NumPad8)) return ( 0, -1);
            if (IsJustPressed(Keys.NumPad9)) return ( 1, -1);
            if (IsJustPressed(Keys.NumPad4)) return (-1,  0);
            if (IsJustPressed(Keys.NumPad6)) return ( 1,  0);
            if (IsJustPressed(Keys.NumPad1)) return (-1,  1);
            if (IsJustPressed(Keys.NumPad2)) return ( 0,  1);
            if (IsJustPressed(Keys.NumPad3)) return ( 1,  1);

            return (0, 0);
        }

        /// <summary>Returns the 1-based jutsu index (1-7) pressed, or 0 if none.</summary>
        public int GetJutsuKey()
        {
            if (IsJustPressed(Keys.D1)) return 1;
            if (IsJustPressed(Keys.D2)) return 2;
            if (IsJustPressed(Keys.D3)) return 3;
            if (IsJustPressed(Keys.D4)) return 4;
            if (IsJustPressed(Keys.D5)) return 5;
            if (IsJustPressed(Keys.D6)) return 6;
            if (IsJustPressed(Keys.D7)) return 7;
            return 0;
        }
    }
}
