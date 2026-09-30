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
    public event Action RoomChangeEvent = delegate { };
    public event Action RoomClearEvent = delegate { };
    public event Action<IngredientData> IngredientSpawnedEvent = delegate { };
    public event Action<HatData> HatCraftedEvent = delegate { };
    public event Action IngredientPickedUpEvent = delegate { };
    private LevelGenerator2D roomGenerator;

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

    private void Start()
    {
        roomGenerator = FindFirstObjectByType<LevelGenerator2D>();

        roomGenerator.StartRoomLoad();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            roomGenerator.ClearLevel();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            roomGenerator = FindFirstObjectByType<LevelGenerator2D>();
            roomGenerator.StartRoomLoad();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            roomGenerator.SpawnRandomRoom();
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
        CraftingManager.Instance.SpawnIngredient(enemy.transform);
    }

    public void InvokeIngredientSpawnedEvent(IngredientData ingredient)
    {
        IngredientSpawnedEvent.Invoke(ingredient);
    }

    public void InvokeIngredientPickedUpEvent()
    {
        IngredientPickedUpEvent.Invoke();
    }

    public void InvokeRoomChangeEvent()
    {
        RoomChangeEvent.Invoke();
    }

    public void InvokeRoomClearEvent()
    {
        RoomClearEvent.Invoke();
    }

    public void InvokeHatCraftedEvent(HatData hatData)
    {
        HatCraftedEvent.Invoke(hatData);
    }
}
