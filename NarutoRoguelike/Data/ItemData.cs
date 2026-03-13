using System.Collections.Generic;
using System.Text.Json.Serialization;
using NarutoRoguelike.Entities;

namespace NarutoRoguelike.Data
{
    public record ItemTemplate(
        [property: JsonPropertyName("name")]         string   Name,
        [property: JsonPropertyName("type")]         ItemType Type,
        [property: JsonPropertyName("slot")]         EquipSlot Slot,
        [property: JsonPropertyName("attackBonus")]  int      AttackBonus,
        [property: JsonPropertyName("defenseBonus")] int      DefenseBonus,
        [property: JsonPropertyName("hpBonus")]      int      HPBonus,
        [property: JsonPropertyName("chakraBonus")]  int      ChakraBonus,
        [property: JsonPropertyName("healAmount")]   int      HealAmount,
        [property: JsonPropertyName("chakraAmount")] int      ChakraAmount,
        [property: JsonPropertyName("description")]  string   Description,
        [property: JsonPropertyName("floorMin")]     int      FloorMin,
        [property: JsonPropertyName("floorMax")]     int      FloorMax);

    public static class ItemDatabase
    {
        public static readonly IReadOnlyList<ItemTemplate> All = new List<ItemTemplate>
        {
            // ── Consumables ───────────────────────────────────────────────────────
            new("Soldier Pill",     ItemType.Consumable, EquipSlot.None,  0,  0,  0,  0, 30,  0,
                "A medicinal pill used by shinobi to restore health.",              1, 10),
            new("Chakra Pill",      ItemType.Consumable, EquipSlot.None,  0,  0,  0,  0,  0, 30,
                "Restores chakra reserves quickly.",                                 1, 10),
            new("Army Ration Pill", ItemType.Consumable, EquipSlot.None,  0,  0,  0,  0, 50, 20,
                "A powerful pill that restores both HP and chakra.",                 3, 10),
            new("Antidote",         ItemType.Consumable, EquipSlot.None,  0,  0,  0,  0, 15,  0,
                "Cures poison and restores a small amount of health.",              1, 10),

            // ── Weapons ───────────────────────────────────────────────────────────
            new("Kunai",            ItemType.Weapon, EquipSlot.Weapon,   3,  0,  0,  0,  0,  0,
                "Standard-issue ninja throwing knife.",                              1,  5),
            new("Shuriken Pack",    ItemType.Weapon, EquipSlot.Weapon,   5,  0,  0,  0,  0,  0,
                "A pack of shuriken for ranged attacks.",                            1,  5),
            new("Sword of the Mist",ItemType.Weapon, EquipSlot.Weapon,  12,  0,  0,  0,  0,  0,
                "A legendary sword wielded by the Seven Swordsmen.",                 5, 10),
            new("Hiraishin Kunai",  ItemType.Weapon, EquipSlot.Weapon,  18,  2,  0, 10,  0,  0,
                "Minato's signature technique kunai — carries flying thunder.",      7, 10),

            // ── Armour ────────────────────────────────────────────────────────────
            new("Flak Jacket",      ItemType.Armor,  EquipSlot.Body,     0,  4, 10,  0,  0,  0,
                "Standard Chunin-rank chest armour.",                                2,  6),
            new("Headband",         ItemType.Armor,  EquipSlot.Head,     0,  1,  5,  0,  0,  0,
                "A ninja headband that offers minimal protection.",                  1,  5),
            new("Chakra Armour",    ItemType.Armor,  EquipSlot.Body,     0,  8, 20, 20,  0,  0,
                "Advanced armour infused with chakra — used by the Third Hokage.",   6, 10),
            new("Sage Cloak",       ItemType.Armor,  EquipSlot.Body,     2, 10, 30, 30,  0,  0,
                "A cloak imbued with natural energy, worn by Sages.",                8, 10),

            // ── Scrolls ───────────────────────────────────────────────────────────
            new("Scroll of Fireball",    ItemType.Scroll, EquipSlot.None, 0, 0, 0, 0, 0, 0,
                "Teaches the Great Fireball Jutsu.",                                 2,  8),
            new("Scroll of Sand Shield", ItemType.Scroll, EquipSlot.None, 0, 0, 0, 0, 0, 0,
                "Teaches the Sand Shield defensive jutsu.",                          3,  8),
            new("Scroll of Summoning",   ItemType.Scroll, EquipSlot.None, 0, 0, 0, 0, 0, 0,
                "Teaches the Summoning Jutsu.",                                      5, 10),
        };

        public static IEnumerable<ItemTemplate> ForFloor(int floor)
        {
            foreach (var t in All)
                if (floor >= t.FloorMin && floor <= t.FloorMax)
                    yield return t;
        }
    }
}
