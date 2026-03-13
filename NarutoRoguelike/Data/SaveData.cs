using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NarutoRoguelike.Data;
using NarutoRoguelike.Entities;

namespace NarutoRoguelike.Data
{
    // ── Player snapshot ───────────────────────────────────────────────────────────

    public class PlayerData
    {
        [JsonPropertyName("gridX")]        public int             GridX        { get; set; }
        [JsonPropertyName("gridY")]        public int             GridY        { get; set; }
        [JsonPropertyName("level")]        public int             Level        { get; set; }
        [JsonPropertyName("xp")]           public int             XP           { get; set; }
        [JsonPropertyName("currentHp")]    public int             CurrentHP    { get; set; }
        [JsonPropertyName("maxHp")]        public int             MaxHP        { get; set; }
        [JsonPropertyName("currentChakra")]public int             CurrentChakra{ get; set; }
        [JsonPropertyName("maxChakra")]    public int             MaxChakra    { get; set; }
        [JsonPropertyName("attack")]       public int             Attack       { get; set; }
        [JsonPropertyName("defense")]      public int             Defense      { get; set; }
        [JsonPropertyName("speed")]        public int             Speed        { get; set; }
        [JsonPropertyName("knownJutsu")]   public List<JutsuType> KnownJutsu   { get; set; } = new();
        [JsonPropertyName("inventory")]    public List<ItemSave>  Inventory    { get; set; } = new();
    }

    public class ItemSave
    {
        [JsonPropertyName("name")]         public string   Name        { get; set; } = "";
        [JsonPropertyName("type")]         public ItemType Type        { get; set; }
        [JsonPropertyName("slot")]         public EquipSlot Slot       { get; set; }
        [JsonPropertyName("isEquipped")]   public bool     IsEquipped  { get; set; }
        [JsonPropertyName("attackBonus")]  public int      AttackBonus { get; set; }
        [JsonPropertyName("defenseBonus")] public int      DefenseBonus{ get; set; }
        [JsonPropertyName("hpBonus")]      public int      HPBonus     { get; set; }
        [JsonPropertyName("chakraBonus")]  public int      ChakraBonus { get; set; }
        [JsonPropertyName("healAmount")]   public int      HealAmount  { get; set; }
        [JsonPropertyName("chakraAmount")] public int      ChakraAmount{ get; set; }
        [JsonPropertyName("description")]  public string   Description { get; set; } = "";
    }

    // ── Root save object ──────────────────────────────────────────────────────────

    public class SaveGame
    {
        [JsonPropertyName("version")]    public int        Version   { get; set; } = 1;
        [JsonPropertyName("saveTime")]   public DateTime   SaveTime  { get; set; }
        [JsonPropertyName("floor")]      public int        Floor     { get; set; }
        [JsonPropertyName("mapSeed")]    public int        MapSeed   { get; set; }
        [JsonPropertyName("turnNumber")] public int        TurnNumber{ get; set; }
        [JsonPropertyName("player")]     public PlayerData Player    { get; set; } = new();

        // ── Conversion helpers ────────────────────────────────────────────────────

        public static PlayerData FromPlayer(Player p)
        {
            var data = new PlayerData
            {
                GridX         = p.GridX,
                GridY         = p.GridY,
                Level         = p.Level,
                XP            = p.XP,
                CurrentHP     = p.Health.CurrentHP,
                MaxHP         = p.Health.MaxHP,
                CurrentChakra = p.Chakra.CurrentChakra,
                MaxChakra     = p.Chakra.MaxChakra,
                Attack        = p.Fighter.Attack,
                Defense       = p.Fighter.Defense,
                Speed         = p.Fighter.Speed,
            };

            data.KnownJutsu.AddRange(p.KnownJutsu);

            foreach (var item in p.Inventory)
                data.Inventory.Add(ItemToSave(item));

            return data;
        }

        private static ItemSave ItemToSave(Item item) => new ItemSave
        {
            Name         = item.Name,
            Type         = item.Type,
            Slot         = item.Slot,
            IsEquipped   = item.IsEquipped,
            AttackBonus  = item.AttackBonus,
            DefenseBonus = item.DefenseBonus,
            HPBonus      = item.HPBonus,
            ChakraBonus  = item.ChakraBonus,
            HealAmount   = item.HealAmount,
            ChakraAmount = item.ChakraAmount,
            Description  = item.Description,
        };
    }
}
