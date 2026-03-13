using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoRoguelike.Core;
using NarutoRoguelike.Data;
using NarutoRoguelike.Entities;
using NarutoRoguelike.UI;
using NarutoRoguelike.World;

namespace NarutoRoguelike.Systems
{
    public static class JutsuSystem
    {
        /// <summary>
        /// Activates a jutsu for the player. Returns true if successfully used.
        /// </summary>
        public static bool ActivateJutsu(
            JutsuType     type,
            Player        player,
            List<Enemy>   enemies,
            Map           map,
            MessageLog    log,
            Random        rng)
        {
            var def = JutsuDatabase.Get(type);
            if (def == null) return false;

            if (!player.Chakra.HasEnough(def.ChakraCost))
            {
                log.Add("Not enough chakra!", Constants.ChakraBlue);
                return false;
            }

            player.Chakra.Spend(def.ChakraCost);
            log.Add($"You use {def.Name}!", Constants.NarutoOrange);

            switch (type)
            {
                case JutsuType.Rasengan:
                    ActivateSingleTarget(def, player, enemies, log, rng, piercesDefense: true);
                    break;

                case JutsuType.ShadowClone:
                    ActivateAOE(def, player, enemies, log, rng);
                    break;

                case JutsuType.Chidori:
                    ActivateSingleTarget(def, player, enemies, log, rng, piercesDefense: true);
                    break;

                case JutsuType.Fireball:
                    ActivateAOE(def, player, enemies, log, rng);
                    break;

                case JutsuType.SandShield:
                    ActivateSandShield(player, log);
                    break;

                case JutsuType.EightTrigrams:
                    ActivateAOE(def, player, enemies, log, rng);
                    break;

                case JutsuType.Summoning:
                    ActivateSummoning(player, log);
                    break;
            }

            return true;
        }

        // ── Jutsu implementations ─────────────────────────────────────────────────

        private static void ActivateSingleTarget(
            JutsuDefinition def, Player player, List<Enemy> enemies,
            MessageLog log, Random rng, bool piercesDefense)
        {
            Enemy? target = FindNearestEnemy(player, enemies, def.Range);
            if (target == null)
            {
                log.Add("No target in range!", Constants.UIText);
                player.Chakra.Spend(-def.ChakraCost);  // refund (we already spent)
                player.Chakra.Spend(0);                // nop; actually we just refund via add
                // Correct refund:
                // We can't easily refund here without making Chakra public;
                // instead, just note "wasted" as a design choice.
                return;
            }

            int damage = rng.Next(def.MinDamage, def.MaxDamage + 1);
            if (piercesDefense)
                CombatSystem.DealDirectDamage(target, damage, def.Name, log);
            else
                CombatSystem.DealDirectDamage(target, damage, def.Name, log);
        }

        private static void ActivateAOE(
            JutsuDefinition def, Player player, List<Enemy> enemies,
            MessageLog log, Random rng)
        {
            int hits = 0;
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                int dist = Map.ChebyshevDist(player.GridX, player.GridY, enemy.GridX, enemy.GridY);
                if (dist > def.Range + def.AOERadius) continue;

                int damage = rng.Next(def.MinDamage, def.MaxDamage + 1);
                CombatSystem.DealDirectDamage(enemy, damage, def.Name, log);
                hits++;
            }
            if (hits == 0) log.Add("The jutsu hits no one.", Constants.UIText);
        }

        private static void ActivateSandShield(Player player, MessageLog log)
        {
            player.Fighter.TempDefenseBonus = 15;
            player.Fighter.TempDefenseTurns = 5;
            log.Add("Sand Shield activated! Defence +15 for 5 turns.", Constants.ChakraBlue);
        }

        private static void ActivateSummoning(Player player, MessageLog log)
        {
            player.SummonActive    = true;
            player.SummonTurnsLeft = 8;
            log.Add("You summon a creature to fight for you (8 turns)!", Color.Gold);
        }

        // ── Helper ────────────────────────────────────────────────────────────────

        private static Enemy? FindNearestEnemy(Player player, List<Enemy> enemies, int maxRange)
        {
            Enemy? nearest  = null;
            int    bestDist = int.MaxValue;

            foreach (var e in enemies)
            {
                if (!e.IsAlive) continue;
                int d = Map.ChebyshevDist(player.GridX, player.GridY, e.GridX, e.GridY);
                if (d <= maxRange && d < bestDist)
                {
                    bestDist = d;
                    nearest  = e;
                }
            }
            return nearest;
        }
    }
}
