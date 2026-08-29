using UnityEngine;

[System.Serializable]
public class OnKillTrigger : HatTrigger
{
    public override void Initialize(HatInstance hat) => hat.OnKillEvent += OnKill;
    public override void Terminate(HatInstance hat) => hat.OnKillEvent -= OnKill;

    private void OnKill(HatInstance hat, GameObject victim) => hat.TriggerEffect(this, victim);
}