using UnityEngine;

[System.Serializable]
public class OnDamagedTrigger : HatTrigger
{
    public override void Initialize(HatInstance hat) => hat.OnDamagedEvent += OnDamaged;
    public override void Terminate(HatInstance hat) => hat.OnDamagedEvent -= OnDamaged;

    private void OnDamaged(HatInstance hat, GameObject attacker) => hat.TriggerEffect(this, attacker);
}
