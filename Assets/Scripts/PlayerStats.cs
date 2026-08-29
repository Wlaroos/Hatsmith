using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseFireDelay = 0.5f;
    [SerializeField] private float baseBulletSize = 1.0f;
    [SerializeField] private int baseMaxHealth = 3;

    // Runtime Calculated Properties
    public float MoveSpeed { get; private set; }
    public float FireDelay { get; private set; }
    public int MaxHealth { get; private set; }
    public float BulletSize { get; private set; }
    public float DamageBonus { get; private set; }

    // Dictionary tracking active stat modifier totals
    private readonly Dictionary<HatStatType, float> _statModifiers = new();

    private void Awake()
    {
        RecalculateStats();
    }

    public void AddModifier(HatStatType stat, float value)
    {
        if (!_statModifiers.ContainsKey(stat))
            _statModifiers[stat] = 0f;

        _statModifiers[stat] += value;
        RecalculateStats();
    }

    public void RemoveModifier(HatStatType stat, float value)
    {
        if (_statModifiers.ContainsKey(stat))
        {
            _statModifiers[stat] -= value;
            RecalculateStats();
        }
    }

    public void RecalculateStats()
    {
        float moveSpeedBonus = GetModifierValue(HatStatType.MoveSpeed);
        float fireRateBonus = GetModifierValue(HatStatType.FireDelay);
        float healthBonus = GetModifierValue(HatStatType.MaxHealth);
        float bulletSizeBonus = GetModifierValue(HatStatType.BulletSize);
        
        DamageBonus = GetModifierValue(HatStatType.Damage);

        MoveSpeed = Mathf.Max(0.5f, baseMoveSpeed + moveSpeedBonus);
        FireDelay = Mathf.Max(0.02f, baseFireDelay - fireRateBonus);
        MaxHealth = Mathf.Max(1, baseMaxHealth + Mathf.RoundToInt(healthBonus));
        BulletSize = Mathf.Max(0.1f, baseBulletSize + bulletSizeBonus);
    }

    private float GetModifierValue(HatStatType stat)
    {
        return _statModifiers.TryGetValue(stat, out float val) ? val : 0f;
    }

    public float GetDamageBonus() => DamageBonus;
}