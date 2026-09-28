using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

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
    private LevelGenerator2D roomGenerator;

    [Header("Item Drop Settings")]
    private int _ingredientID ;
    private int[] _ingredientCounts = new int[6];
    [SerializeField] private GameObject _ingredientPrefab;
    [SerializeField] private float _ingredientDropChance = 0.25f;

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

        // Find the LevelGenerator2D in the scene even after scene reloads
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
        SpawnIngredient(enemy.transform);
    }

    public void AddIngredientCount(int ingredientId, int count)
    {
        if (ingredientId < 0 || ingredientId >= _ingredientCounts.Length)
        {
            return;
        }

        _ingredientCounts[ingredientId] += count;
    }

    private void SpawnIngredient(Transform parent)
    {
        if (_ingredientPrefab != null && UnityEngine.Random.value <= _ingredientDropChance)
        {
            GameObject ingredient = Instantiate(_ingredientPrefab, parent.position, Quaternion.identity);
            int id = UnityEngine.Random.Range(0, 6);
            ingredient.GetComponent<IngredientPickup>().SetIngredientType((IngredientPickup.IngredientType)id);

            Debug.Log("Spawned ingredient: " + id);
        }
    }
}
