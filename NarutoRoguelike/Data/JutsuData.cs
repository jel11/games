using System.Collections.Generic;
using NarutoRoguelike.Core;

namespace NarutoRoguelike.Data
{
    public enum JutsuType
    {
        Rasengan,
        ShadowClone,
        Chidori,
        Fireball,
        SandShield,
        EightTrigrams,
        Summoning
    }

    public record JutsuDefinition(
        JutsuType Type,
        string    Name,
        string    Description,
        int       ChakraCost,
        int       MinDamage,
        int       MaxDamage,
        int       Range,
        int       AOERadius,
        bool      IsSelfTarget);

    public static class JutsuDatabase
    {
        public static readonly IReadOnlyList<JutsuDefinition> All = new List<JutsuDefinition>
        {
            new(JutsuType.Rasengan,
                "Rasengan",
                "A spinning ball of chakra — deals heavy single-target damage.",
                Constants.CHAKRA_RASENGAN, 30, 45, 1, 0, false),

            new(JutsuType.ShadowClone,
                "Shadow Clone Jutsu",
                "Creates a clone to confuse enemies — deals moderate AOE damage.",
                Constants.CHAKRA_SHADOW_CLONE, 15, 25, 3, 1, false),

            new(JutsuType.Chidori,
                "Chidori",
                "A lightning-infused thrust that pierces defenses.",
                Constants.CHAKRA_CHIDORI, 35, 55, 1, 0, false),

            new(JutsuType.Fireball,
                "Great Fireball Jutsu",
                "A massive fireball that scorches all enemies in range.",
                Constants.CHAKRA_FIREBALL, 20, 35, 4, 2, false),

            new(JutsuType.SandShield,
                "Sand Shield",
                "Erects a shield of sand, reducing incoming damage for several turns.",
                Constants.CHAKRA_SAND_SHIELD, 0, 0, 0, 0, true),

            new(JutsuType.EightTrigrams,
                "Eight Trigrams 64 Palms",
                "A flurry of gentle-fist strikes that hits all adjacent enemies.",
                Constants.CHAKRA_EIGHT_TRIGRAMS, 25, 40, 1, 1, false),

            new(JutsuType.Summoning,
                "Summoning Jutsu",
                "Summons a creature to fight alongside you for several turns.",
                Constants.CHAKRA_SUMMONING, 0, 0, 0, 0, true),
        };

        public static JutsuDefinition? Get(JutsuType type)
        {
            foreach (var j in All)
                if (j.Type == type) return j;
            return null;
        }
    }
}
