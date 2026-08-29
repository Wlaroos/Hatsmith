using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event Action PlayerDamageEvent = delegate { };
    public event Action PlayerHealEvent = delegate { };
    public event Action PlayerDownedEvent = delegate { };
    public event Action PlayerKilledEvent = delegate { };
    public event Action<GameObject> PlayerShootEvent = delegate { };
    public event Action<GameObject> EnemyHitEvent = delegate { };
    public event Action<GameObject> EnemyKilledEvent = delegate { };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void InvokePlayerDamageEvent()
    {
        PlayerDamageEvent.Invoke();
    }

    public void InvokePlayerHealEvent()
    {
        PlayerHealEvent.Invoke();
    }

    public void InvokePlayerDownedEvent()
    {
        PlayerDownedEvent.Invoke();
    }

    public void InvokePlayerKilledEvent()
    {
        PlayerKilledEvent.Invoke();
    }

    public void InvokePlayerShootEvent(GameObject player)
    {
        PlayerShootEvent.Invoke(player);
    }

    public void InvokeEnemyHitEvent(GameObject enemy)
    {
        EnemyHitEvent.Invoke(enemy);
    }

    public void InvokeEnemyKilledEvent(GameObject enemy)
    {
        EnemyKilledEvent.Invoke(enemy);
    }
}
