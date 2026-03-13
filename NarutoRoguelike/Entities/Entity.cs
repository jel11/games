using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NarutoRoguelike.Components;

namespace NarutoRoguelike.Entities
{
    public abstract class Entity
    {
        // ── Grid position ─────────────────────────────────────────────────────────
        public int GridX { get; set; }
        public int GridY { get; set; }

        // ── Identity ──────────────────────────────────────────────────────────────
        public string    Name           { get; set; }
        public Texture2D? Texture       { get; set; }
        public Color      RenderColor   { get; set; } = Color.White;
        public bool       BlocksMovement{ get; set; } = true;
        public bool       IsAlive       { get; set; } = true;

        // ── Component bag ─────────────────────────────────────────────────────────
        private readonly Dictionary<Type, IComponent> _components = new();

        protected Entity(string name, int x, int y)
        {
            Name  = name;
            GridX = x;
            GridY = y;
        }

        public void AddComponent(IComponent component)
            => _components[component.GetType()] = component;

        public T? GetComponent<T>() where T : class, IComponent
            => _components.TryGetValue(typeof(T), out var c) ? c as T : null;

        public bool HasComponent<T>() where T : class, IComponent
            => _components.ContainsKey(typeof(T));

        // ── Helpers ───────────────────────────────────────────────────────────────

        public Point GridPosition => new Point(GridX, GridY);

        public bool IsAt(int x, int y) => GridX == x && GridY == y;
    }
}
