using UnityEngine;

namespace Core.Combat
{
    public enum DamageType
    {
        Physical,
        Elemental
    }

    /// Immutable value object containing information about a combat damage event.
    public readonly struct DamageData
    {
        public int Amount { get; }
        public DamageType Type { get; }
        public GameObject Source { get; }

        public DamageData(int amount, DamageType type = DamageType.Physical, GameObject source = null)
        {
            Amount = amount;
            Type = type;
            Source = source;
        }
    }
}