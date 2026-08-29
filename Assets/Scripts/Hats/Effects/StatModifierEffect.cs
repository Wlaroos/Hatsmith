using System;
using UnityEngine;

public enum HatStatType
{
    MoveSpeed,
    FireDelay,
    MaxHealth,
    BulletSize,
    Damage
}

[Serializable]
public class StatModifierEffect : HatEffect
{
    public HatStatType statType;
    public float modifierValue = 1.0f;

    public override void Execute(HatInstance hat, GameObject target = null)
    {
        if (hat.Wearer.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.AddModifier(statType, modifierValue);
        }
    }

    public void Remove(HatInstance hat)
    {
        if (hat.Wearer.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.RemoveModifier(statType, modifierValue);
        }
    }
}