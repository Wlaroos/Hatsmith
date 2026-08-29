using UnityEngine;

[System.Serializable]
public class TickIntervalTrigger : HatTrigger
{
    public float interval = 1.0f;
    private float timer;

    public override void Initialize(HatInstance hat) => hat.OnUpdateEvent += OnUpdate;
    public override void Terminate(HatInstance hat) => hat.OnUpdateEvent -= OnUpdate;

    private void OnUpdate(HatInstance hat)
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            hat.TriggerEffect(this, hat.Wearer);
        }
    }
}
