using System.Collections.Generic;
using NarutoRoguelike.Entities;

namespace NarutoRoguelike.Systems
{
    /// <summary>
    /// Orchestrates turn order. Entities take turns in descending Speed order.
    /// The player's turn is flagged with <see cref="PlayerTurnPending"/>; the game loop
    /// must wait for player input before calling <see cref="EndPlayerTurn"/>.
    /// </summary>
    public class TurnSystem
    {
        private readonly List<Entity> _order = new();
        private int  _currentIndex;

        public bool PlayerTurnPending { get; private set; }
        public int  TurnNumber        { get; private set; }

        // ── Setup ─────────────────────────────────────────────────────────────────

        public void RegisterEntities(Player player, IEnumerable<Enemy> enemies)
        {
            _order.Clear();
            _order.Add(player);
            foreach (var e in enemies)
                if (e.IsAlive) _order.Add(e);

            SortBySpeed();
            _currentIndex     = 0;
            PlayerTurnPending = _order.Count > 0 && _order[0] is Player;
        }

        public void AddEnemy(Enemy enemy)
        {
            if (!_order.Contains(enemy))
            {
                _order.Add(enemy);
                SortBySpeed();
            }
        }

        public void RemoveDead()
        {
            _order.RemoveAll(e => !e.IsAlive);
            if (_currentIndex >= _order.Count) _currentIndex = 0;
        }

        // ── Turn processing ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns the entity whose turn it currently is,
        /// or null if the player turn is pending input.
        /// </summary>
        public Entity? GetCurrentEntity()
        {
            if (_order.Count == 0) return null;
            return _order[_currentIndex];
        }

        /// <summary>Advances to the next entity; sets PlayerTurnPending if it's the player.</summary>
        public void Advance()
        {
            if (_order.Count == 0) return;

            _currentIndex = (_currentIndex + 1) % _order.Count;

            var current = _order[_currentIndex];
            if (current is Player)
            {
                PlayerTurnPending = true;
                TurnNumber++;
            }
            else
            {
                PlayerTurnPending = false;
            }
        }

        /// <summary>Called after player input has been processed.</summary>
        public void EndPlayerTurn()
        {
            PlayerTurnPending = false;
            Advance();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private void SortBySpeed()
        {
            _order.Sort((a, b) =>
            {
                int sa = a.GetComponent<NarutoRoguelike.Components.FighterComponent>()?.Speed ?? 0;
                int sb = b.GetComponent<NarutoRoguelike.Components.FighterComponent>()?.Speed ?? 0;
                return sb.CompareTo(sa);   // descending
            });
        }
    }
}
