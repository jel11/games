using Microsoft.Xna.Framework;
using NarutoRoguelike.Components;
using NarutoRoguelike.Core;

namespace NarutoRoguelike.Entities
{
    public enum EnemyRank
    {
        Genin,
        Chunin,
        Jonin,
        Akatsuki
    }

    public class Enemy : Entity
    {
        public EnemyRank       Rank    { get; }
        public int             XPValue { get; }
        public HealthComponent Health  { get; }
        public FighterComponent Fighter { get; }
        public AIComponent     AI      { get; }

        public Enemy(string name, EnemyRank rank, int x, int y)
            : base(name, x, y)
        {
            Rank = rank;

            // Stats by rank
            (int hp, int atk, int def, int spd, int xp) = rank switch
            {
                EnemyRank.Genin    => (Constants.GENIN_HP,    Constants.GENIN_ATTACK,    Constants.GENIN_DEFENSE,    6,  Constants.GENIN_XP),
                EnemyRank.Chunin   => (Constants.CHUNIN_HP,   Constants.CHUNIN_ATTACK,   Constants.CHUNIN_DEFENSE,   8,  Constants.CHUNIN_XP),
                EnemyRank.Jonin    => (Constants.JONIN_HP,    Constants.JONIN_ATTACK,    Constants.JONIN_DEFENSE,    10, Constants.JONIN_XP),
                EnemyRank.Akatsuki => (Constants.AKATSUKI_HP, Constants.AKATSUKI_ATTACK, Constants.AKATSUKI_DEFENSE, 12, Constants.AKATSUKI_XP),
                _                  => (Constants.GENIN_HP,    Constants.GENIN_ATTACK,    Constants.GENIN_DEFENSE,    6,  Constants.GENIN_XP)
            };

            XPValue = xp;

            Health  = new HealthComponent(hp);
            Fighter = new FighterComponent(atk, def, spd);
            AI      = new AIComponent(sightRange: rank >= EnemyRank.Jonin ? 10 : 8);

            Health.OnDeath += () => IsAlive = false;

            AddComponent(Health);
            AddComponent(Fighter);
            AddComponent(AI);

            // Colour by rank
            RenderColor = rank switch
            {
                EnemyRank.Genin    => Color.LimeGreen,
                EnemyRank.Chunin   => Color.CornflowerBlue,
                EnemyRank.Jonin    => Color.MediumPurple,
                EnemyRank.Akatsuki => Constants.AkatsukiRed,
                _                  => Color.White
            };
        }
    }
}
