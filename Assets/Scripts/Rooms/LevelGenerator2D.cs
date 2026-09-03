using UnityEngine;

public class LevelGenerator2D : MonoBehaviour
{
    [Header("Generator Data")]
    [SerializeField]private TilePalette palette;
    [SerializeField]private float tileSize = 1f;

    [Header("Runtime Spawn Targets")]
    [SerializeField]private Transform roomParent;
    [SerializeField]private Transform playerTransform; // Assign in inspector or auto-find at runtime

    private void Awake()
    {
        // Auto-locate references if missing
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

        // Auto-locate player if reference is missing
        if (playerTransform == null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null) playerTransform = player.transform;
        }

        // Calculate total world dimensions of the room
        float roomWidth = mapTexture.width * tileSize;
        float roomHeight = mapTexture.height * tileSize;

        // Center offset relative to roomParent
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

                        // Process tile based on its configured type
                        ProcessTileMapping(mapping, targetPosition, container);
                        break;
                    }
                }
            }
        }
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

            case TileType.CustomAction:
                HandleCustomAction(mapping.Tag, worldPosition, container);
                break;
        }
    }

    private void TeleportPlayer(Vector3 spawnPosition)
    {
        if (playerTransform != null)
        {
            // Reset velocity if Rigidbody2D is attached to prevent physics slide after teleport
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

    private void HandleCustomAction(string actionTag, Vector3 position, Transform container)
    {
        // Examples for now until I find a better system for custom actions
        switch (actionTag)
        {
            case "RoomExit":
                Debug.Log($"Spawned Room Exit Trigger at {position}");
                break;
            default:
                Debug.Log($"Executed Custom Action '{actionTag}' at {position}");
                break;
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
        Texture2D roomTexture = Resources.Load<Texture2D>("SpecialRooms/Start_Room"); // Load the start room texture from Resources

        if (roomTexture != null)
        {
            GenerateLevelFromTexture(roomTexture);
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