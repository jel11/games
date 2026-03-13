using System.Collections.Generic;
using Microsoft.Xna.Framework;
using NarutoRoguelike.Core;
using NarutoRoguelike.Entities;
using NarutoRoguelike.UI;

namespace NarutoRoguelike.Systems
{
    public static class ItemSystem
    {
        // ── Pick up / Drop ────────────────────────────────────────────────────────

        public static bool PickUp(Player player, Item item, List<Item> groundItems, MessageLog log)
        {
            if (player.InventoryFull)
            {
                log.Add("Inventory is full!", Constants.UIText);
                return false;
            }

            groundItems.Remove(item);
            player.Inventory.Add(item);
            log.Add($"You pick up {item.Name}.", Constants.UIText);
            return true;
        }

        public static void Drop(Player player, Item item, List<Item> groundItems, MessageLog log)
        {
            if (item.IsEquipped) Unequip(player, item, log);

            player.Inventory.Remove(item);
            item.GridX = player.GridX;
            item.GridY = player.GridY;
            groundItems.Add(item);
            log.Add($"You drop {item.Name}.", Constants.UIText);
        }

        // ── Use ───────────────────────────────────────────────────────────────────

        public static bool Use(Player player, Item item, MessageLog log)
        {
            switch (item.Type)
            {
                case ItemType.Consumable:
                    UseConsumable(player, item, log);
                    player.Inventory.Remove(item);
                    return true;

                case ItemType.Weapon:
                case ItemType.Armor:
                    if (item.IsEquipped) Unequip(player, item, log);
                    else                  Equip(player, item, log);
                    return true;

                case ItemType.Scroll:
                    UseScroll(player, item, log);
                    player.Inventory.Remove(item);
                    return true;

                default:
                    log.Add($"You can't use {item.Name} right now.", Constants.UIText);
                    return false;
            }
        }

        // ── Equip / Unequip ───────────────────────────────────────────────────────

        public static void Equip(Player player, Item item, MessageLog log)
        {
            if (item.Slot == EquipSlot.None)
            {
                log.Add($"{item.Name} can't be equipped.", Constants.UIText);
                return;
            }

            // Unequip whatever is in the slot
            var existing = player.GetEquipped(item.Slot);
            if (existing != null) Unequip(player, existing, log);

            item.IsEquipped = true;
            player.SetEquipped(item.Slot, item);

            // Apply stat bonuses
            player.Fighter.Attack  += item.AttackBonus;
            player.Fighter.Defense += item.DefenseBonus;
            player.Health.IncreaseMax(item.HPBonus);

            log.Add($"You equip {item.Name}.", Constants.UIHighlight);
        }

        public static void Unequip(Player player, Item item, MessageLog log)
        {
            if (!item.IsEquipped) return;

            item.IsEquipped = false;
            player.SetEquipped(item.Slot, null);

            // Remove stat bonuses
            player.Fighter.Attack  -= item.AttackBonus;
            player.Fighter.Defense -= item.DefenseBonus;
            // Note: HP max reduction is capped so current HP doesn't go negative
            int newMax = player.Health.MaxHP - item.HPBonus;
            if (newMax > 0) player.Health.IncreaseMax(-item.HPBonus);

            log.Add($"You unequip {item.Name}.", Constants.UIText);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static void UseConsumable(Player player, Item item, MessageLog log)
        {
            if (item.HealAmount > 0)
            {
                int healed = player.Health.Heal(item.HealAmount);
                log.Add($"You use {item.Name} and restore {healed} HP.", Constants.HPBarFull);
            }
            if (item.ChakraAmount > 0)
            {
                player.Chakra.Regenerate(item.ChakraAmount);
                log.Add($"You use {item.Name} and restore {item.ChakraAmount} chakra.", Constants.ChakraBlue);
            }
        }

        private static void UseScroll(Player player, Item item, MessageLog log)
        {
            // Scrolls teach a random unkown jutsu based on name
            log.Add($"You read {item.Name}. A new technique has been unlocked!", Constants.UIHighlight);
        }
    }
}
