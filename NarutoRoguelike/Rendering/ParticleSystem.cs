using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NarutoRoguelike.Rendering
{
    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color   StartColor;
        public Color   EndColor;
        public float   Life;         // current remaining life (seconds)
        public float   MaxLife;
        public float   Size;
        public float   Rotation;
        public float   RotationSpeed;

        public float   Alpha => Life / MaxLife;
        public Color   Current => Color.Lerp(EndColor, StartColor, Alpha) * Alpha;
        public bool    IsAlive => Life > 0;
    }

    public class ParticleEmitter
    {
        private readonly List<Particle> _particles = new();
        private readonly Random         _rng;

        public int Count => _particles.Count;

        public ParticleEmitter(Random rng)
        {
            _rng = rng;
        }

        // ── Emission ──────────────────────────────────────────────────────────────

        public void Emit(Vector2 worldPos, Color startColor, Color endColor,
                         int count = 12, float maxLife = 0.6f, float speed = 60f, float size = 4f)
        {
            for (int i = 0; i < count; i++)
            {
                double angle = _rng.NextDouble() * Math.PI * 2;
                float  spd   = (float)(_rng.NextDouble() * speed + speed * 0.5);
                _particles.Add(new Particle
                {
                    Position     = worldPos,
                    Velocity     = new Vector2((float)Math.Cos(angle) * spd,
                                               (float)Math.Sin(angle) * spd),
                    StartColor   = startColor,
                    EndColor     = endColor,
                    Life         = maxLife,
                    MaxLife      = maxLife,
                    Size         = size,
                    Rotation     = 0f,
                    RotationSpeed = (float)(_rng.NextDouble() * 4 - 2)
                });
            }
        }

        public void EmitImpact(Vector2 worldPos)
            => Emit(worldPos, Color.OrangeRed, Color.DarkRed, 20, 0.5f, 80f, 5f);

        public void EmitChakra(Vector2 worldPos)
            => Emit(worldPos, Color.CornflowerBlue, Color.DarkBlue, 15, 0.8f, 50f, 3f);

        public void EmitHeal(Vector2 worldPos)
            => Emit(worldPos, Color.LimeGreen, Color.DarkGreen, 10, 0.7f, 40f, 3f);

        // ── Update / Draw ─────────────────────────────────────────────────────────

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Life     -= dt;
                p.Position += p.Velocity * dt;
                p.Velocity *= 0.92f;          // friction
                p.Rotation += p.RotationSpeed * dt;

                if (!p.IsAlive) _particles.RemoveAt(i);
            }
        }

        public void Draw(SpriteBatch sb, Texture2D pixel, Vector2 cameraOffset)
        {
            foreach (var p in _particles)
            {
                var screenPos = p.Position - cameraOffset;
                var rect = new Rectangle(
                    (int)(screenPos.X - p.Size / 2),
                    (int)(screenPos.Y - p.Size / 2),
                    (int)p.Size, (int)p.Size);
                sb.Draw(pixel, rect, null, p.Current,
                        p.Rotation, Vector2.Zero, SpriteEffects.None, 0f);
            }
        }
    }
}
