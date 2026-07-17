using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement _player;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _startPoolSize = 10;
    [SerializeField] private float _spawnPadding = 1.5f;

    private readonly List<EnemyMovement> _enemies = new();
    private Camera _mainCamera;

    private void Awake()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerMovement>();
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

        EnemyMovement enemyMovement = enemyObject.GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            enemyMovement = enemyObject.AddComponent<EnemyMovement>();
        }

        enemyMovement.Initialize(_player);
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

        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0:
                return new Vector2(-halfWidth, Random.Range(-halfHeight, halfHeight));
            case 1:
                return new Vector2(halfWidth, Random.Range(-halfHeight, halfHeight));
            case 2:
                return new Vector2(Random.Range(-halfWidth, halfWidth), halfHeight);
            default:
                return new Vector2(Random.Range(-halfWidth, halfWidth), -halfHeight);
        }
    }

    private IEnumerator SpawnEnemiesOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            SpawnEnemy();
        }
    }
}
