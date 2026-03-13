using System;

namespace NarutoRoguelike.Components
{
    public class HealthComponent : IComponent
    {
        public int MaxHP     { get; private set; }
        public int CurrentHP { get; private set; }

        public bool IsDead => CurrentHP <= 0;

        public event Action? OnDeath;
        public event Action<int>? OnDamaged;  // arg = damage taken
        public event Action<int>? OnHealed;   // arg = amount healed

        public HealthComponent(int maxHP)
        {
            MaxHP     = maxHP;
            CurrentHP = maxHP;
        }

        public int TakeDamage(int amount)
        {
            int actual = Math.Max(0, amount);
            CurrentHP  = Math.Max(0, CurrentHP - actual);
            OnDamaged?.Invoke(actual);
            if (IsDead) OnDeath?.Invoke();
            return actual;
        }

        public int Heal(int amount)
        {
            int actual = Math.Min(amount, MaxHP - CurrentHP);
            CurrentHP += actual;
            if (actual > 0) OnHealed?.Invoke(actual);
            return actual;
        }

        public void HealFull() => CurrentHP = MaxHP;

        public void IncreaseMax(int amount)
        {
            MaxHP     += amount;
            CurrentHP += amount;   // also heal the increase
        }

        public float Fraction => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    }
}
