using UnityEngine;

public class LevelGenerator2D : MonoBehaviour
{
    [Header("Generator Data")]
    [SerializeField] private TilePalette palette;
    [SerializeField] private float tileSize = 1f;

    [Header("Runtime Spawn Targets")]
    [SerializeField] private Transform roomParent;
    [SerializeField] private Transform playerTransform;

    private Grid2D _grid;

    private void Awake()
    {
        _grid = FindFirstObjectByType<Grid2D>();

        if (playerTransform == null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) playerTransform = player.transform;
        }
    }

    public void GenerateLevelFromTexture(Texture2D mapTexture)
    {
        ClearLevel();

        if (mapTexture == null || palette == null)
        {
            Debug.LogError("Missing Texture2D or TilePalette reference!");
            return;
        }

        Transform container = roomParent != null ? roomParent : transform;

        if (playerTransform == null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) playerTransform = player.transform;
        }

        float roomWidth = mapTexture.width * tileSize;
        float roomHeight = mapTexture.height * tileSize;

        Vector3 centerOffset = new Vector3(
            (roomWidth / 2f) - (tileSize / 2f),
            (roomHeight / 2f) - (tileSize / 2f),
            0f
        );

        for (int x = 0; x < mapTexture.width; x++)
        {
            for (int y = 0; y < mapTexture.height; y++)
            {
                Color pixelColor = mapTexture.GetPixel(x, y);

                if (pixelColor.a == 0) continue;

                foreach (TileMapping mapping in palette.mappings)
                {
                    if (ColorEquals(mapping.Color, pixelColor))
                    {
                        Vector3 rawPosition = new Vector3(x * tileSize, y * tileSize, 0f);
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
        if (playerTransform != null)
        {
            Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }

            playerTransform.position = spawnPosition;
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

        Transform container = roomParent != null ? roomParent : transform;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(container.GetChild(i).gameObject);
            else
                DestroyImmediate(container.GetChild(i).gameObject);
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