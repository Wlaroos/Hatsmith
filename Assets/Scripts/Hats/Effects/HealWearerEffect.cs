using UnityEngine;

[System.Serializable]
public class HealWearerEffect : HatEffect
{
    public int healAmount = 1;

    public override void Execute(HatInstance hat, GameObject target = null)
    {
        if (hat.Wearer.TryGetComponent<PlayerHealth>(out var health))
        {
            health.Heal(healAmount);
            Debug.Log($"Healed {hat.Wearer.name} for {healAmount} health.");
        }
    }
}