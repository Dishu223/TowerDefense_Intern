using System;

namespace Core.Combat
{
    /// <summary>
    /// Contract for entities that can receive damage and track mortality.
    /// </summary>
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