using UnityEngine;

public class LevelGenerator2D : MonoBehaviour
{
    public static LevelGenerator2D Instance { get; private set; }
    [Header("Generator Data")]
    [SerializeField] private TilePalette _palette;
    [SerializeField] private float _tileSize = 1f;

    [Header("Runtime Spawn Targets")]
    [SerializeField] private Transform _roomParent;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _bulletParent;
    [SerializeField] private Transform _particleParent;
    public Transform BulletParent => _bulletParent;
    public Transform ParticleParent => _particleParent;

    private Grid2D _grid;

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

        _grid = FindFirstObjectByType<Grid2D>();

        if (_playerTransform == null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) _playerTransform = player.transform;
        }
    }

    public void GenerateLevelFromTexture(Texture2D mapTexture)
    {
        ClearLevel();

        if (mapTexture == null || _palette == null)
        {
            Debug.LogError("Missing Texture2D or TilePalette reference!");
            return;
        }

        Transform container = _roomParent != null ? _roomParent : transform;

        if (_playerTransform == null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) _playerTransform = player.transform;
        }

        float roomWidth = mapTexture.width * _tileSize;
        float roomHeight = mapTexture.height * _tileSize;

        Vector3 centerOffset = new Vector3(
            (roomWidth / 2f) - (_tileSize / 2f),
            (roomHeight / 2f) - (_tileSize / 2f),
            0f
        );

        for (int x = 0; x < mapTexture.width; x++)
        {
            for (int y = 0; y < mapTexture.height; y++)
            {
                Color pixelColor = mapTexture.GetPixel(x, y);

                if (pixelColor.a == 0) continue;

                foreach (TileMapping mapping in _palette.mappings)
                {
                    if (ColorEquals(mapping.Color, pixelColor))
                    {
                        Vector3 rawPosition = new Vector3(x * _tileSize, y * _tileSize, 0f);
                        Vector3 targetPosition = container.position + rawPosition - centerOffset;

                        ProcessTileMapping(mapping, targetPosition, container);
                        break;
                    }
                }
            }
        }

        UpdatePathfindingGrid();
    }

    private void ProcessTileMapping(TileMapping mapping, Vector3 worldPosition, Transform container)
    {
        switch (mapping.Type)
        {
            case TileType.Prefab:
                if (mapping.Prefab != null)
                {
                    Instantiate(mapping.Prefab, worldPosition, Quaternion.identity, container);
                }
                break;

            case TileType.PlayerSpawn:
                TeleportPlayer(worldPosition);
                break;

            case TileType.Enemy:
                SpawnPooledEnemy(worldPosition);
                break;
        }
    }

    private void SpawnPooledEnemy(Vector3 worldPosition)
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.SpawnEnemyAtPosition(worldPosition);
        }
        else
        {
            Debug.LogWarning("Enemy tile detected, but no EnemyManager instance found in the scene!");
        }
    }

    private void TeleportPlayer(Vector3 spawnPosition)
    {
        if (_playerTransform != null)
        {
            Rigidbody2D playerRb = _playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }

            _playerTransform.position = spawnPosition;
            Debug.Log($"<color=cyan>Player teleported to spawn tile:</color> {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("PlayerSpawn tile encountered, but no Player Transform was found!");
        }
    }

    public void SpawnRandomRoom()
    {
        Texture2D[] allRooms = Resources.LoadAll<Texture2D>("Rooms");
        if (allRooms.Length > 0)
        {
            Texture2D randomRoom = allRooms[Random.Range(0, allRooms.Length)];
            GenerateLevelFromTexture(randomRoom);
        }
        else
        {
            Debug.LogError("No room textures found in Resources/Rooms");
        }
    }

    public void ClearLevel()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ClearAllActiveEnemies();
        }

        Transform container = _roomParent != null ? _roomParent : transform;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(container.GetChild(i).gameObject);
            else
                DestroyImmediate(container.GetChild(i).gameObject);
        }

        for (int i = _bulletParent.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(_bulletParent.GetChild(i).gameObject);
            else
                DestroyImmediate(_bulletParent.GetChild(i).gameObject);
        }

        for (int i = _particleParent.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(_particleParent.GetChild(i).gameObject);
            else
                DestroyImmediate(_particleParent.GetChild(i).gameObject);
        }
    }

    public void StartRoomLoad()
    {
        Texture2D roomTexture = Resources.Load<Texture2D>("SpecialRooms/Start_Room");

        if (roomTexture != null)
        {
            GenerateLevelFromTexture(roomTexture);
        }
    }

    private void UpdatePathfindingGrid()
    {
        if (_grid == null)
        {
            _grid = FindFirstObjectByType<Grid2D>();
        }

        if (_grid != null)
        {
            _grid.CreateGrid();
        }
    }

    private bool ColorEquals(Color c1, Color c2, float tolerance = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance &&
               Mathf.Abs(c1.a - c2.a) < tolerance;
    }
}