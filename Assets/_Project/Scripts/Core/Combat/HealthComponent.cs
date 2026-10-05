using System;
using UnityEngine;

namespace Core.Combat
{
    /// <summary>
    /// Standalone health receiver component implementing IDamageable.
    /// Manages vitality, damage calculations, and mortality dispatching.
    /// </summary>
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Vitality Configuration")]
        [SerializeField] private int baseMaxHealth = 100;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => baseMaxHealth;
        public bool IsDead => CurrentHealth <= 0;

        public event Action<DamageData> OnDamaged;
        public event Action<DamageData> OnKilled;
        public event Action<int, int> OnHealthChanged; // (current, max)

        private void Awake()
        {
            ResetHealth();
        }

        public void SetMaxHealth(int newMax, bool resetCurrent = true)
        {
            baseMaxHealth = Mathf.Max(1, newMax);
            if (resetCurrent)
            {
                ResetHealth();
            }
            else
            {
                CurrentHealth = Mathf.Clamp(CurrentHealth, 0, baseMaxHealth);
                OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        public void ResetHealth()
        {
            CurrentHealth = baseMaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void ApplyDamage(DamageData damageData)
        {
            if (IsDead) return;

            int effectiveDamage = Mathf.Max(0, damageData.Amount);
            CurrentHealth = Mathf.Max(0, CurrentHealth - effectiveDamage);

            OnDamaged?.Invoke(damageData);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth <= 0)
            {
                OnKilled?.Invoke(damageData);
            }
        }

        public void Heal(int healAmount)
        {
            if (IsDead || healAmount <= 0) return;

            CurrentHealth = Mathf.Min(baseMaxHealth, CurrentHealth + healAmount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}