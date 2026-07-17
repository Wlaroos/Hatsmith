using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
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
        if (_playerMovement == null)
        {
            _playerMovement = FindAnyObjectByType<PlayerMovement>();
        }

        if (_playerHealth == null && _playerMovement != null)
        {
            _playerHealth = _playerMovement.GetComponent<PlayerHealth>();
        }

        _mainCamera = Camera.main;

        if (_enemyPrefab == null)
        {
            Debug.LogWarning("EnemyManager: No enemy prefab assigned.");
        }
    }

    private void Start()
    {
        SpawnEnemy();
        StartCoroutine(SpawnEnemiesOverTime());
    }

    public void SpawnEnemy()
    {
        if (_mainCamera == null || _enemyPrefab == null)
        {
            return;
        }

        EnemyMovement enemyToSpawn = GetAvailableEnemy();
        if (enemyToSpawn == null)
        {
            if (_enemies.Count >= _startPoolSize)
            {
                return;
            }

            enemyToSpawn = CreateEnemy();
        }

        enemyToSpawn.Spawn(GetOffScreenSpawnPosition());
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
        enemyMovement.Initialize(_playerMovement, _playerHealth);
        _enemies.Add(enemyMovement);
        return enemyMovement;
    }

    private Vector2 GetOffScreenSpawnPosition()
    {
        if (_mainCamera == null)
        {
            return Vector2.zero;
        }

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