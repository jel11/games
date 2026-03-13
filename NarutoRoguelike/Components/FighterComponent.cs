using System;
using NarutoRoguelike.Core;

namespace NarutoRoguelike.Components
{
    public class FighterComponent : IComponent
    {
        public int Attack  { get; set; }
        public int Defense { get; set; }
        public int Speed   { get; set; }

        // Temporary modifiers (e.g. from Sand Shield jutsu)
        public int TempDefenseBonus { get; set; }
        public int TempDefenseTurns { get; set; }

        public FighterComponent(int attack, int defense, int speed)
        {
            Attack  = attack;
            Defense = defense;
            Speed   = speed;
        }

        /// <summary>Calculates raw outgoing damage (before target defence).</summary>
        public int CalculateDamage(Random rng)
        {
            // ±20 % random variance
            float variance = 0.8f + (float)rng.NextDouble() * 0.4f;
            int   raw      = (int)(Attack * variance);

            // Critical hit
            if (rng.NextDouble() < Constants.CRITICAL_HIT_CHANCE)
                raw = (int)(raw * Constants.CRITICAL_HIT_MULTIPLIER);

            return Math.Max(1, raw);
        }

        /// <summary>Applies this fighter's defence (+ any temp bonus) to incoming damage.</summary>
        public int ApplyDefense(int incomingDamage)
        {
            int totalDefense = Defense + TempDefenseBonus;
            return Math.Max(1, incomingDamage - totalDefense);
        }

        /// <summary>Call once per turn to tick down temporary effects.</summary>
        public void TickEffects()
        {
            if (TempDefenseTurns > 0)
            {
                TempDefenseTurns--;
                if (TempDefenseTurns == 0)
                    TempDefenseBonus = 0;
            }
        }
    }
}
