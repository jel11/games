using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Core;

namespace NarutoRoguelike.Managers
{
    /// <summary>
    /// Wraps MonoGame's ContentManager and provides programmatically-generated
    /// placeholder textures so the game can run without any external asset files.
    /// </summary>
    public class NarutoContentManager
    {
        private readonly ContentManager        _mgContent;
        private readonly GraphicsDevice        _gd;
        private readonly Dictionary<string, Texture2D>   _textures = new();
        private readonly Dictionary<string, SpriteFont?> _fonts    = new();

        public NarutoContentManager(ContentManager mgContent, GraphicsDevice gd)
        {
            _mgContent = mgContent;
            _gd        = gd;
        }

        // ── Texture access ────────────────────────────────────────────────────────

        public Texture2D GetTexture(string name)
        {
            if (_textures.TryGetValue(name, out var tex)) return tex;

            // Try loading from content pipeline; fall back to placeholder
            try
            {
                tex = _mgContent.Load<Texture2D>($"Sprites/{name}");
                _textures[name] = tex;
                return tex;
            }
            catch
            {
                // Generate 1×1 white pixel as ultimate fallback
                var fallback = CreateSolidTexture(1, 1, Color.White);
                _textures[name] = fallback;
                return fallback;
            }
        }

        public SpriteFont? GetFont(string name)
        {
            if (_fonts.TryGetValue(name, out var font)) return font;

            try
            {
                font = _mgContent.Load<SpriteFont>($"Fonts/{name}");
                _fonts[name] = font;
                return font;
            }
            catch
            {
                _fonts[name] = null;
                return null;
            }
        }

        // ── Placeholder texture generation ────────────────────────────────────────

        /// <summary>
        /// Generates all placeholder textures (32×32 coloured squares) at startup.
        /// Must be called after the GraphicsDevice is ready (i.e. from LoadContent).
        /// </summary>
        public void GeneratePlaceholderTextures()
        {
            // 1×1 utility pixel (used for UI bars, borders, particles)
            Register("pixel",    CreateSolidTexture(1,  1,  Color.White));

            // Map tiles
            Register("floor",    CreateTileTexture(Constants.TILE_SIZE, Constants.FloorLitColor,  Constants.FloorColor));
            Register("wall",     CreateTileTexture(Constants.TILE_SIZE, Constants.WallLitColor,   new Color(15, 15, 15)));

            // Entities — all 32×32 coloured squares with a dark outline
            Register("player",   CreateEntityTexture(Constants.TILE_SIZE, Constants.NarutoOrange,  Color.DarkOrange));
            Register("genin",    CreateEntityTexture(Constants.TILE_SIZE, Color.LimeGreen,         Color.DarkGreen));
            Register("chunin",   CreateEntityTexture(Constants.TILE_SIZE, Color.CornflowerBlue,    Color.DarkBlue));
            Register("jonin",    CreateEntityTexture(Constants.TILE_SIZE, Color.MediumPurple,      Color.DarkViolet));
            Register("akatsuki", CreateEntityTexture(Constants.TILE_SIZE, Constants.AkatsukiRed,   new Color(80, 0, 0)));

            // Items
            Register("item",     CreateEntityTexture(Constants.TILE_SIZE / 2, Color.Gold,          Color.DarkGoldenrod));
        }

        private void Register(string key, Texture2D tex) => _textures[key] = tex;

        // ── Texture factories ─────────────────────────────────────────────────────

        private Texture2D CreateSolidTexture(int width, int height, Color color)
        {
            var tex    = new Texture2D(_gd, width, height);
            var pixels = new Color[width * height];
            Array.Fill(pixels, color);
            tex.SetData(pixels);
            return tex;
        }

        /// <summary>Tile with a slightly lighter fill and darker border pixel.</summary>
        private Texture2D CreateTileTexture(int size, Color fill, Color border)
        {
            var tex    = new Texture2D(_gd, size, size);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                pixels[y * size + x] = isBorder ? border : fill;
            }

            tex.SetData(pixels);
            return tex;
        }

        /// <summary>Entity sprite: filled square with 2-pixel dark outline.</summary>
        private Texture2D CreateEntityTexture(int size, Color fill, Color outline)
        {
            var tex    = new Texture2D(_gd, size, size);
            var pixels = new Color[size * size];
            const int border = 2;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool isBorder = x < border || y < border || x >= size - border || y >= size - border;

                // Simple cross/detail in center to differentiate entities
                int cx = size / 2, cy = size / 2;
                bool isDetail = Math.Abs(x - cx) <= 1 || Math.Abs(y - cy) <= 1;

                if (isBorder)
                    pixels[y * size + x] = outline;
                else if (isDetail)
                    pixels[y * size + x] = Color.Lerp(fill, Color.White, 0.5f);
                else
                    pixels[y * size + x] = fill;
            }

            tex.SetData(pixels);
            return tex;
        }
    }
}
