using System;

namespace Core.Combat
{
    /// Contract for entities that can receive damage and track mortality.
    public interface IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }

        void ApplyDamage(DamageData damageData);

        event Action<DamageData> OnDamaged;
        event Action<DamageData> OnKilled;
    }
}