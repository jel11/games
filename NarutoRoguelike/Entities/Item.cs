using Microsoft.Xna.Framework;

namespace NarutoRoguelike.Entities
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Scroll,         // teaches a new jutsu
        QuestItem
    }

    public enum EquipSlot
    {
        None = 0,
        Head,
        Body,
        Legs,
        Feet,
        Weapon,
        Offhand
    }

    public class Item : Entity
    {
        public ItemType  Type       { get; }
        public EquipSlot Slot       { get; }
        public bool      IsEquipped { get; set; }

        // Stat deltas applied when equipped / consumed
        public int AttackBonus  { get; init; }
        public int DefenseBonus { get; init; }
        public int ChakraBonus  { get; init; }
        public int HPBonus      { get; init; }

        // Consumable specifics
        public int  HealAmount   { get; init; }
        public int  ChakraAmount { get; init; }

        public string Description { get; init; } = "";

        public Item(string name, ItemType type, EquipSlot slot, int x = 0, int y = 0)
            : base(name, x, y)
        {
            Type           = type;
            Slot           = slot;
            BlocksMovement = false;
            RenderColor    = Color.Yellow;
        }
    }
}
