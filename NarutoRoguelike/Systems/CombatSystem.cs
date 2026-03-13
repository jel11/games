using System;
using Microsoft.Xna.Framework;
using NarutoRoguelike.Components;
using NarutoRoguelike.Core;
using NarutoRoguelike.Entities;
using NarutoRoguelike.UI;

namespace NarutoRoguelike.Systems
{
    public static class CombatSystem
    {
        /// <summary>
        /// Resolves one attack from <paramref name="attacker"/> against <paramref name="defender"/>.
        /// Writes result to the message log. Returns actual HP damage dealt.
        /// </summary>
        public static int Attack(Entity attacker, Entity defender, MessageLog log, Random rng)
        {
            var atkFighter = attacker.GetComponent<FighterComponent>();
            var defFighter = defender.GetComponent<FighterComponent>();
            var defHealth  = defender.GetComponent<HealthComponent>();

            if (atkFighter == null || defHealth == null) return 0;

            // Dodge check
            int dodgeChance = Constants.DODGE_BASE_CHANCE + (defFighter?.Speed ?? 0) / 4;
            if (rng.Next(100) < dodgeChance)
            {
                log.Add($"{defender.Name} dodges {attacker.Name}'s attack!", Constants.UIText);
                return 0;
            }

            int raw    = atkFighter.CalculateDamage(rng);
            int actual = defFighter != null ? defFighter.ApplyDefense(raw) : Math.Max(1, raw);
            int dealt  = defHealth.TakeDamage(actual);

            bool isCrit = raw > (int)(atkFighter.Attack * 1.8f);   // heuristic: was it boosted?
            string critStr = isCrit ? " [CRIT]" : "";

            Color msgColor = attacker is Player ? Constants.NarutoOrange : Constants.AkatsukiRed;
            log.Add($"{attacker.Name} attacks {defender.Name} for {dealt} damage!{critStr}", msgColor);

            if (defHealth.IsDead)
                log.Add($"{defender.Name} has been defeated!", Constants.UIHighlight);

            return dealt;
        }

        /// <summary>Deals direct damage (bypasses defence), e.g. from jutsu.</summary>
        public static int DealDirectDamage(Entity target, int amount, string sourceName, MessageLog log)
        {
            var health = target.GetComponent<HealthComponent>();
            if (health == null) return 0;

            int dealt = health.TakeDamage(amount);
            log.Add($"{sourceName} deals {dealt} damage to {target.Name}!", Color.OrangeRed);

            if (health.IsDead)
                log.Add($"{target.Name} has been defeated!", Constants.UIHighlight);

            return dealt;
        }
    }
}
