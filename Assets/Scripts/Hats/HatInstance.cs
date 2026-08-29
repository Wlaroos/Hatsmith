using System;
using UnityEngine;

public class HatInstance : MonoBehaviour
{
    public HatData Data { get; private set; }
    public GameObject Wearer { get; private set; }

    public event Action<HatInstance> OnUpdateEvent;
    public event Action<HatInstance, GameObject> OnDamagedEvent;
    public event Action<HatInstance, GameObject> OnKillEvent;
    public event Action<HatInstance, GameObject> OnShootEvent;

    public void Initialize(GameObject wearer, HatData data)
    {
        Wearer = wearer;
        Data = data;

        // Hook up global GameManager events to this hat instance
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDamageEvent += HandlePlayerDamaged;
            GameManager.Instance.EnemyKilledEvent += HandleEnemyKilled;
            GameManager.Instance.PlayerShootEvent += HandlePlayerShoot;
        }

        foreach (var rule in Data.rules)
        {
            rule.trigger?.Initialize(this);
        }
    }

    private void Update()
    {
        OnUpdateEvent?.Invoke(this);
    }

    private void HandlePlayerDamaged()
    {
        // Only trigger if this hat is equipped on the player
        if (Wearer != null && Wearer.CompareTag("Player"))
        {
            OnDamagedEvent?.Invoke(this, Wearer);
        }
    }

    private void HandleEnemyKilled(GameObject victim)
    {
        if (Wearer != null && Wearer.CompareTag("Player"))
        {
            OnKillEvent?.Invoke(this, victim);
        }
    }

    private void HandlePlayerShoot(GameObject weapon)
    {
        if (Wearer != null && Wearer.CompareTag("Player"))
        {
            OnShootEvent?.Invoke(this, weapon);
        }
    }

    public void TriggerEffect(HatTrigger triggerSource, GameObject target = null)
    {
        foreach (var rule in Data.rules)
        {
            if (rule.trigger == triggerSource)
            {
                rule.effect?.Execute(this, target);
            }
        }
    }

    public void Remove()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDamageEvent -= HandlePlayerDamaged;
            GameManager.Instance.EnemyKilledEvent -= HandleEnemyKilled;
            GameManager.Instance.PlayerShootEvent -= HandlePlayerShoot;
        }

        foreach (var rule in Data.rules)
        {
            rule.trigger?.Terminate(this);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Remove();
    }
}