using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; } // Added Singleton

    [Header("Player")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerHealth _playerHealth;
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _startPoolSize = 10;
    [SerializeField] private float _spawnPadding = 1.5f;

    private readonly List<EnemyMovement> _enemies = new();
    private Camera _mainCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (_playerMovement == null)
            _playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (_playerHealth == null && _playerMovement != null)
            _playerHealth = _playerMovement.GetComponent<PlayerHealth>();

        _mainCamera = Camera.main;

        // Pre-warm the pool
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < _startPoolSize; i++)
        {
            CreateEnemy();
        }
    }

    public void SpawnEnemy()
    {
        if (_mainCamera == null || _enemyPrefab == null) return;

        EnemyMovement enemyToSpawn = GetAvailableEnemy();
        
        // Dynamic growth: Expand pool if max size exceeded
        if (enemyToSpawn == null)
        {
            enemyToSpawn = CreateEnemy();
        }

        enemyToSpawn.Spawn(GetOffScreenSpawnPosition());
    }

    public void SpawnEnemyAtPosition(Vector2 position)
    {
        if (_mainCamera == null || _enemyPrefab == null) return;

        EnemyMovement enemyToSpawn = GetAvailableEnemy();
        if (enemyToSpawn == null)
        {
            enemyToSpawn = CreateEnemy();
        }

        enemyToSpawn.Spawn(position);
    }

    public void ClearAllActiveEnemies()
    {
        foreach (EnemyMovement enemy in _enemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    private EnemyMovement GetAvailableEnemy()
    {
        foreach (EnemyMovement enemy in _enemies)
        {
            if (enemy != null && !enemy.gameObject.activeSelf)
            {
                return enemy;
            }
        }
        return null;
    }

    private EnemyMovement CreateEnemy()
    {
        GameObject enemyObject = Instantiate(_enemyPrefab, transform);
        enemyObject.SetActive(false);

        EnemyMovement enemyMovement = enemyObject.GetComponent<EnemyMovement>() ?? enemyObject.AddComponent<EnemyMovement>();
        _enemies.Add(enemyMovement);
        return enemyMovement;
    }

    private Vector2 GetOffScreenSpawnPosition()
    {
        if (_mainCamera == null) return Vector2.zero;

        float halfHeight = _mainCamera.orthographicSize + _spawnPadding;
        float halfWidth = (halfHeight * _mainCamera.aspect) + _spawnPadding;

        return Random.Range(0, 4) switch
        {
            0 => new Vector2(-halfWidth, Random.Range(-halfHeight, halfHeight)),
            1 => new Vector2(halfWidth, Random.Range(-halfHeight, halfHeight)),
            2 => new Vector2(Random.Range(-halfWidth, halfWidth), halfHeight),
            _ => new Vector2(Random.Range(-halfWidth, halfWidth), -halfHeight)
        };
    }

    private IEnumerator SpawnEnemiesOverTime()
    {
        while (_playerHealth == null || !_playerHealth.IsDowned)
        {
            yield return new WaitForSeconds(1f);
            SpawnEnemy();
        }
    }
}