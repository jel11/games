using System;

namespace NarutoRoguelike.Components
{
    public class ChakraComponent : IComponent
    {
        public int MaxChakra     { get; private set; }
        public int CurrentChakra { get; private set; }

        public ChakraComponent(int maxChakra)
        {
            MaxChakra     = maxChakra;
            CurrentChakra = maxChakra;
        }

        public bool HasEnough(int cost) => CurrentChakra >= cost;

        /// <summary>Deducts chakra; returns false and does nothing if insufficient.</summary>
        public bool Spend(int cost)
        {
            if (!HasEnough(cost)) return false;
            CurrentChakra -= cost;
            return true;
        }

        public void Regenerate(int amount)
        {
            CurrentChakra = Math.Min(MaxChakra, CurrentChakra + amount);
        }

        public void RestoreFull() => CurrentChakra = MaxChakra;

        public void IncreaseMax(int amount)
        {
            MaxChakra     += amount;
            CurrentChakra += amount;
        }

        public float Fraction => MaxChakra > 0 ? (float)CurrentChakra / MaxChakra : 0f;
    }
}
