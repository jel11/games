using System.Collections.Generic;
using System.Text.Json.Serialization;
using NarutoRoguelike.Entities;

namespace NarutoRoguelike.Data
{
    public record EnemyTemplate(
        [property: JsonPropertyName("name")]       string     Name,
        [property: JsonPropertyName("rank")]       EnemyRank  Rank,
        [property: JsonPropertyName("hp")]         int        HP,
        [property: JsonPropertyName("attack")]     int        Attack,
        [property: JsonPropertyName("defense")]    int        Defense,
        [property: JsonPropertyName("xpValue")]    int        XPValue,
        [property: JsonPropertyName("textureKey")] string     TextureKey,
        [property: JsonPropertyName("floorMin")]   int        FloorMin,
        [property: JsonPropertyName("floorMax")]   int        FloorMax);

    public static class EnemyDatabase
    {
        public static readonly IReadOnlyList<EnemyTemplate> All = new List<EnemyTemplate>
        {
            // ── Genin ─────────────────────────────────────────────────────────────
            new("Konoha Genin",       EnemyRank.Genin,    30,  8, 2, 20,  "genin",    1, 3),
            new("Sound Genin",        EnemyRank.Genin,    32,  9, 2, 22,  "genin",    1, 4),
            new("Mist Genin",         EnemyRank.Genin,    28, 10, 1, 20,  "genin",    1, 3),

            // ── Chunin ────────────────────────────────────────────────────────────
            new("Konoha Chunin",      EnemyRank.Chunin,   55, 14, 5, 50,  "chunin",   2, 6),
            new("Rain Chunin",        EnemyRank.Chunin,   58, 15, 4, 55,  "chunin",   2, 6),
            new("Sand Chunin",        EnemyRank.Chunin,   60, 13, 6, 50,  "chunin",   3, 7),

            // ── Jonin ─────────────────────────────────────────────────────────────
            new("Konoha Jonin",       EnemyRank.Jonin,   100, 22,10,120,  "jonin",    5, 9),
            new("ANBU Black Ops",     EnemyRank.Jonin,   110, 25, 8,130,  "jonin",    5, 9),
            new("Rogue Jonin",        EnemyRank.Jonin,   105, 20,12,120,  "jonin",    4, 10),

            // ── Akatsuki ──────────────────────────────────────────────────────────
            new("Kisame Hoshigaki",   EnemyRank.Akatsuki,200, 35,15,300,  "akatsuki", 8, 10),
            new("Itachi Uchiha",      EnemyRank.Akatsuki,180, 40,12,320,  "akatsuki", 8, 10),
            new("Deidara",            EnemyRank.Akatsuki,160, 38,10,310,  "akatsuki", 7, 10),
            new("Pain (Nagato)",      EnemyRank.Akatsuki,250, 45,18,400,  "akatsuki", 9, 10),
        };

        /// <summary>Returns enemies valid for the given floor depth.</summary>
        public static IEnumerable<EnemyTemplate> ForFloor(int floor)
        {
            foreach (var t in All)
                if (floor >= t.FloorMin && floor <= t.FloorMax)
                    yield return t;
        }
    }
}
