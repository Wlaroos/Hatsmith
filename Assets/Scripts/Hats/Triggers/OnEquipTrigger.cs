using System;
using UnityEngine;

[Serializable]
public class OnEquipTrigger : HatTrigger
{
    public override void Initialize(HatInstance hat)
    {
        // Executes immediate effects when initialized on a wearer
        hat.TriggerEffect(this, hat.Wearer);
    }

    public override void Terminate(HatInstance hat)
    {
        // When hat is removed, clean up modifiers
        foreach (var rule in hat.Data.rules)
        {
            if (rule.trigger == this && rule.effect is StatModifierEffect statEffect)
            {
                statEffect.Remove(hat);
            }
        }
    }
}
