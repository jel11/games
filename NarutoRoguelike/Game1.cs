using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NarutoRoguelike.Core;
using NarutoRoguelike.Managers;
using NarutoRoguelike.Rendering;

namespace NarutoRoguelike
{
    public class Game1 : Game
    {
        // ── MonoGame infrastructure ───────────────────────────────────────────────
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch          _spriteBatch   = null!;

        // ── Game systems ──────────────────────────────────────────────────────────
        private GameStateManager     _stateManager  = null!;
        private NarutoContentManager _contentMgr    = null!;
        private InputHandler         _input         = null!;
        private Renderer             _renderer      = null!;

        // ─────────────────────────────────────────────────────────────────────────

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth  = Constants.SCREEN_WIDTH,
                PreferredBackBufferHeight = Constants.SCREEN_HEIGHT,
                IsFullScreen              = false,
                SynchronizeWithVerticalRetrace = true,
            };

            Content.RootDirectory = "Content";
            IsMouseVisible        = true;

            // Fixed timestep: 60 FPS
            IsFixedTimeStep    = true;
            TargetElapsedTime  = TimeSpan.FromSeconds(1.0 / 60.0);
        }

        // ── Initialize ───────────────────────────────────────────────────────────

        protected override void Initialize()
        {
            Window.Title       = "Naruto Roguelike";
            Window.AllowUserResizing = false;

            _input        = new InputHandler();
            _stateManager = new GameStateManager(this);

            _graphics.ApplyChanges();
            base.Initialize();
        }

        // ── LoadContent ──────────────────────────────────────────────────────────

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Content manager (wraps MonoGame CM + generates placeholder textures)
            _contentMgr = new NarutoContentManager(Content, GraphicsDevice);
            _contentMgr.GeneratePlaceholderTextures();

            // Renderer (owns TileRenderer, EntityRenderer, ParticleEmitter, HUD, Inventory)
            _renderer = new Renderer(_spriteBatch, _contentMgr, GraphicsDevice);

            // Boot into the main menu
            _stateManager.PushState(
                new MainMenuState(_stateManager, _contentMgr, _input, _renderer));
        }

        // ── Update ───────────────────────────────────────────────────────────────

        protected override void Update(GameTime gameTime)
        {
            _input.Update();
            _stateManager.Update(gameTime);
            base.Update(gameTime);
        }

        // ── Draw ─────────────────────────────────────────────────────────────────

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _stateManager.Draw(_spriteBatch, gameTime);
            base.Draw(gameTime);
        }
    }
}
