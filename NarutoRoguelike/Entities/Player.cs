using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoRoguelike.Components;
using NarutoRoguelike.Core;
using NarutoRoguelike.Data;
using NarutoRoguelike.UI;

namespace NarutoRoguelike.Entities
{
    public class Player : Entity
    {
        // ── Progression ───────────────────────────────────────────────────────────
        public int Level          { get; private set; } = 1;
        public int XP             { get; set; }
        public int XPToNextLevel  => (int)(Constants.PLAYER_LEVEL_UP_BASE
                                          * MathF.Pow(Constants.PLAYER_LEVEL_UP_SCALE, Level - 1));

        // ── Core components ───────────────────────────────────────────────────────
        public HealthComponent  Health  { get; }
        public ChakraComponent  Chakra  { get; }
        public FighterComponent Fighter { get; }

        // ── Inventory ─────────────────────────────────────────────────────────────
        public List<Item>   Inventory    { get; } = new(capacity: 26);
        public Item?[]      EquipSlots   { get; } = new Item?[7];    // indexed by (int)EquipSlot
        public List<JutsuType> KnownJutsu { get; } = new();

        // ── State ─────────────────────────────────────────────────────────────────
        public int  CurrentFloor       { get; set; } = 1;
        public bool IsInventoryOpen    { get; set; }
        public int  InventoryCursor    { get; set; }
        public int  SelectedJutsuIndex { get; set; }

        // ── Summon state ──────────────────────────────────────────────────────────
        public bool SummonActive      { get; set; }
        public int  SummonTurnsLeft   { get; set; }
        public int  ShieldTurnsLeft   => Fighter.TempDefenseTurns;

        public Player(int x, int y) : base("Naruto", x, y)
        {
            BlocksMovement = true;
            RenderColor    = Constants.NarutoOrange;

            Health  = new HealthComponent(Constants.PLAYER_HP_BASE);
            Chakra  = new ChakraComponent(Constants.PLAYER_CHAKRA_BASE);
            Fighter = new FighterComponent(
                Constants.PLAYER_ATTACK_BASE,
                Constants.PLAYER_DEFENSE_BASE,
                Constants.PLAYER_SPEED_BASE);

            Health.OnDeath += () => IsAlive = false;

            AddComponent(Health);
            AddComponent(Chakra);
            AddComponent(Fighter);

            // Starting jutsu
            KnownJutsu.Add(JutsuType.ShadowClone);
            KnownJutsu.Add(JutsuType.Rasengan);
        }

        // ── XP / Level-up ─────────────────────────────────────────────────────────

        public void GainXP(int amount, MessageLog log)
        {
            XP += amount;
            log.Add($"You gain {amount} XP.", Constants.XPBarColor);

            while (XP >= XPToNextLevel)
            {
                XP -= XPToNextLevel;
                LevelUp(log);
            }
        }

        private void LevelUp(MessageLog log)
        {
            Level++;
            Health.IncreaseMax(10);
            Chakra.IncreaseMax(8);
            Fighter.Attack  += 2;
            Fighter.Defense += 1;

            // Unlock new jutsu at certain levels
            if (Level == 3 && !KnownJutsu.Contains(JutsuType.Chidori))
            {
                KnownJutsu.Add(JutsuType.Chidori);
                log.Add("You learned Chidori!", Constants.ChakraBlue);
            }
            if (Level == 5 && !KnownJutsu.Contains(JutsuType.Fireball))
            {
                KnownJutsu.Add(JutsuType.Fireball);
                log.Add("You learned Great Fireball Jutsu!", Color.OrangeRed);
            }
            if (Level == 7 && !KnownJutsu.Contains(JutsuType.EightTrigrams))
            {
                KnownJutsu.Add(JutsuType.EightTrigrams);
                log.Add("You learned Eight Trigrams 64 Palms!", Color.White);
            }
            if (Level == 9 && !KnownJutsu.Contains(JutsuType.Summoning))
            {
                KnownJutsu.Add(JutsuType.Summoning);
                log.Add("You learned Summoning Jutsu!", Color.Gold);
            }

            log.Add($"Level Up! You are now Level {Level}!", Constants.UIHighlight);
        }

        // ── Inventory helpers ─────────────────────────────────────────────────────

        public bool InventoryFull => Inventory.Count >= 26;

        public Item? GetEquipped(EquipSlot slot) => EquipSlots[(int)slot];

        public void SetEquipped(EquipSlot slot, Item? item) => EquipSlots[(int)slot] = item;

        // ── Per-turn tick ─────────────────────────────────────────────────────────

        public void OnTurnEnd()
        {
            Chakra.Regenerate(Constants.CHAKRA_REGEN_PER_TURN);
            Fighter.TickEffects();

            if (SummonActive && --SummonTurnsLeft <= 0)
                SummonActive = false;
        }
    }
}
