using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Combat;

namespace Gameplay.Base
{
    [RequireComponent(typeof(HealthComponent))]
    public class BaseCore : MonoBehaviour, IDamageable
    {
        [Header("Base Identity")]
        [SerializeField] private int baseId = 1;
        [SerializeField] private int maxBaseHealth = 20;

        private HealthComponent healthComponent;
        private static readonly List<BaseCore> activeBases = new List<BaseCore>();

        public static IReadOnlyList<BaseCore> ActiveBases => activeBases;
        public static event Action<BaseCore> OnAnyBaseDestroyed;

        public int BaseId => baseId;
        public int CurrentHealth => healthComponent != null ? healthComponent.CurrentHealth : 0;
        public int MaxHealth => healthComponent != null ? healthComponent.MaxHealth : maxBaseHealth;
        public bool IsDead => healthComponent != null && healthComponent.IsDead;

        public event Action<DamageData> OnDamaged;
        public event Action<DamageData> OnKilled;
        public event Action<BaseCore, int, int> OnHealthUpdated;

        private void Awake()
        {
            EnsureHealthComponent();
        }

        private void EnsureHealthComponent()
        {
            if (healthComponent == null)
            {
                healthComponent = GetComponent<HealthComponent>();
                if (healthComponent == null)
                {
                    healthComponent = gameObject.AddComponent<HealthComponent>();
                }
                healthComponent.SetMaxHealth(maxBaseHealth, true);
            }
        }

        public void Initialize(int id, int health)
        {
            baseId = id;
            maxBaseHealth = health;
            EnsureHealthComponent();
            healthComponent.SetMaxHealth(health, true);
        }

        private void OnEnable()
        {
            if (!activeBases.Contains(this)) activeBases.Add(this);

            EnsureHealthComponent();
            healthComponent.OnDamaged += HandleDamaged;
            healthComponent.OnKilled += HandleKilled;
            healthComponent.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            activeBases.Remove(this);

            if (healthComponent != null)
            {
                healthComponent.OnDamaged -= HandleDamaged;
                healthComponent.OnKilled -= HandleKilled;
                healthComponent.OnHealthChanged -= HandleHealthChanged;
            }
        }

        public static BaseCore FindClosestBase(Vector3 position)
        {
            BaseCore closest = null;
            float minSqrDist = float.MaxValue;

            for (int i = 0; i < activeBases.Count; i++)
            {
                if (activeBases[i] == null || activeBases[i].IsDead) continue;
                float sqrDist = (activeBases[i].transform.position - position).sqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    closest = activeBases[i];
                }
            }
            return closest;
        }

        public void ApplyDamage(DamageData damageData)
        {
            EnsureHealthComponent();
            healthComponent.ApplyDamage(damageData);
        }

        private void HandleDamaged(DamageData data) => OnDamaged?.Invoke(data);

        private void HandleHealthChanged(int current, int max)
        {
            OnHealthUpdated?.Invoke(this, current, max);
        }

        private void HandleKilled(DamageData data)
        {
            OnKilled?.Invoke(data);

            bool allBasesDead = true;
            for (int i = 0; i < activeBases.Count; i++)
            {
                if (activeBases[i] != null && !activeBases[i].IsDead)
                {
                    allBasesDead = false;
                    break;
                }
            }

            if (allBasesDead)
            {
                OnAnyBaseDestroyed?.Invoke(this);
            }
        }
    }
}