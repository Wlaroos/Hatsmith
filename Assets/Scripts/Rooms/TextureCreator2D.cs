using System.IO;
using UnityEngine;

public class TextureCreator2D : MonoBehaviour
{
    [Header("Texture Settings")]
    [SerializeField]private int width = 16;
    [SerializeField]private int height = 16;
    [SerializeField]private Color currentColor = Color.red;

    [Header("Display Target")]
    [SerializeField]private SpriteRenderer displayRenderer;
    [SerializeField]private BoxCollider2D drawAreaCollider;

    private Texture2D drawnTexture;

    void Start()
    {
        CreateNewTexture();
    }

    public void CreateNewTexture()
    {
        drawnTexture = new Texture2D(width, height)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        drawnTexture.SetPixels(pixels);
        drawnTexture.Apply();

        // Convert Texture2D to a 2D Sprite dynamically
        Sprite newSprite = Sprite.Create(
            drawnTexture, 
            new Rect(0, 0, width, height), 
            new Vector2(0.5f, 0.5f), 
            16 // Pixels Per Unit (PPU)
        );

        if (displayRenderer != null)
            displayRenderer.sprite = newSprite;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            PaintAtMousePosition();
        }
    }

    private void PaintAtMousePosition()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider == drawAreaCollider)
        {
            // Calculate 2D UV position relative to the collider bounds
            Bounds bounds = drawAreaCollider.bounds;
            float u = (mouseWorldPos.x - bounds.min.x) / bounds.size.x;
            float v = (mouseWorldPos.y - bounds.min.y) / bounds.size.y;

            int x = Mathf.Clamp(Mathf.FloorToInt(u * width), 0, width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(v * height), 0, height - 1);

            drawnTexture.SetPixel(x, y, currentColor);
            drawnTexture.Apply();
        }
    }

    public void SaveTextureToPNG(string fileName)
    {
        byte[] bytes = drawnTexture.EncodeToPNG();
        string path = Application.dataPath + "/" + fileName + ".png";
        File.WriteAllBytes(path, bytes);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        string relativePath = "Assets/" + fileName + ".png";
        UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(relativePath) as UnityEditor.TextureImporter;
        if (importer != null)
        {
            importer.textureType = UnityEditor.TextureImporterType.Sprite;
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
#endif
        Debug.Log("Saved 2D map texture to: " + path);
    }
}