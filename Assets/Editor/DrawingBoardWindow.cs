using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DrawingBoardWindow : EditorWindow
{
    private enum ToolType { Pencil, Bucket, Line, Rectangle }
    private ToolType selectedTool = ToolType.Pencil;

    private Texture2D targetTexture;
    private TilePalette palette;
    private Color activeColor = Color.white;

    private bool showGrid = true;
    private Color gridColor = new Color(1f, 1f, 1f, 0.2f);

    private int textureWidth = 24;
    private int textureHeight = 24;
    private float canvasZoom = 15f; 

    private string fileName = "RoomMap_01";
    private Vector2 scrollPosition;

    // Shape Drawing Drag State
    private bool isDraggingShape = false;
    private bool isRightClickAction = false;
    private Vector2Int shapeStartPos;
    private Vector2Int shapeEndPos;

    [MenuItem("Tools/Drawing Board")]
    public static void ShowWindow()
    {
        GetWindow<DrawingBoardWindow>("Drawing Board");
    }

    private void OnEnable()
    {
        if (targetTexture == null)
        {
            CreateNewTexture();
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        try
        {
            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(15, 15, 15, 15) });

            try
            {
                DrawControlsGUI();
                EditorGUILayout.Space(15);
                DrawCanvasGUI();
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawControlsGUI()
    {
        EditorGUILayout.LabelField("Drawing Controls", EditorStyles.boldLabel);

        palette = (TilePalette)EditorGUILayout.ObjectField("Tile Palette", palette, typeof(TilePalette), false);

        // Color Picker Control
        EditorGUILayout.Space(5);
        activeColor = EditorGUILayout.ColorField("Active Color", activeColor);

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        textureWidth = EditorGUILayout.IntSlider("Width", textureWidth, 4, 128);
        textureHeight = EditorGUILayout.IntSlider("Height", textureHeight, 4, 128);
        EditorGUILayout.EndHorizontal();

        canvasZoom = EditorGUILayout.Slider("Canvas Zoom", canvasZoom, 2f, 30f);
        showGrid = EditorGUILayout.Toggle("Show Grid Overlay", showGrid);

        if (GUILayout.Button("Reset / New Blank Texture", GUILayout.Height(25)))
        {
            CreateNewTexture();
        }

        // Tool Selector
        EditorGUILayout.Space(10);
        GUILayout.Label("Drawing Tools (Right-Click to Erase with active tool)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(selectedTool == ToolType.Pencil, "Pencil", "Button")) selectedTool = ToolType.Pencil;
        if (GUILayout.Toggle(selectedTool == ToolType.Bucket, "Fill Bucket", "Button")) selectedTool = ToolType.Bucket;
        if (GUILayout.Toggle(selectedTool == ToolType.Line, "Line Wall", "Button")) selectedTool = ToolType.Line;
        if (GUILayout.Toggle(selectedTool == ToolType.Rectangle, "Square Wall", "Button")) selectedTool = ToolType.Rectangle;
        EditorGUILayout.EndHorizontal();

        // Palette Quick Select
        if (palette != null && palette.mappings != null && palette.mappings.Count > 0)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Palette Quick Select", EditorStyles.boldLabel);

            foreach (TileMapping mapping in palette.mappings)
            {
                EditorGUILayout.BeginHorizontal();

                Rect colorRect = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(24));
                EditorGUI.DrawRect(colorRect, mapping.Color);

                bool isActive = ColorEquals(activeColor, mapping.Color);
                string buttonText = string.IsNullOrEmpty(mapping.Tag) ? "Unnamed Tile" : mapping.Tag;

                if (isActive) GUI.backgroundColor = new Color(0.6f, 0.9f, 1f);

                if (GUILayout.Button(buttonText, GUILayout.Height(20)))
                {
                    activeColor = mapping.Color;
                    selectedTool = ToolType.Pencil;
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space(10);
        fileName = EditorGUILayout.TextField("Room File Name", fileName);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Save PNG to Assets/Resources/Rooms", GUILayout.Height(30)))
        {
            SaveTextureToFolder();
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawCanvasGUI()
    {
        if (targetTexture == null) return;

        GUILayout.Label("Canvas", EditorStyles.boldLabel);

        float maxAllowedSize = 512f; 
        float rawWidth = targetTexture.width * canvasZoom;
        float rawHeight = targetTexture.height * canvasZoom;

        float scale = Mathf.Min(1f, maxAllowedSize / Mathf.Max(rawWidth, rawHeight));
        float canvasDisplayWidth = rawWidth * scale;
        float canvasDisplayHeight = rawHeight * scale;

        float effectiveZoomX = canvasDisplayWidth / targetTexture.width;
        float effectiveZoomY = canvasDisplayHeight / targetTexture.height;

        Rect canvasRect = GUILayoutUtility.GetRect(canvasDisplayWidth, canvasDisplayHeight, GUILayout.ExpandWidth(false));

        EditorGUI.DrawTextureAlpha(canvasRect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
        GUI.DrawTexture(canvasRect, targetTexture, ScaleMode.StretchToFill);

        if (showGrid)
        {
            DrawGridOverlay(canvasRect);
        }

        Event e = Event.current;

        // Process Mouse Input
        if (canvasRect.Contains(e.mousePosition))
        {
            float localX = e.mousePosition.x - canvasRect.x;
            float localY = e.mousePosition.y - canvasRect.y;

            int pixelX = Mathf.Clamp(Mathf.FloorToInt(localX / effectiveZoomX), 0, targetTexture.width - 1);
            int pixelY = Mathf.Clamp(Mathf.FloorToInt((canvasDisplayHeight - localY) / effectiveZoomY), 0, targetTexture.height - 1);

            // Left-Click (0) = Draw with activeColor, Right-Click (1) = Erase with Color.black
            if (e.button == 0 || e.button == 1)
            {
                Color drawColor = (e.button == 1) ? Color.black : activeColor;
                bool isShapeTool = selectedTool == ToolType.Line || selectedTool == ToolType.Rectangle;

                if (e.type == EventType.MouseDown)
                {
                    if (isShapeTool)
                    {
                        isDraggingShape = true;
                        isRightClickAction = (e.button == 1);
                        shapeStartPos = new Vector2Int(pixelX, pixelY);
                        shapeEndPos = shapeStartPos;
                    }
                    else
                    {
                        ApplyToolAction(pixelX, pixelY, drawColor);
                        targetTexture.Apply();
                    }
                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.MouseDrag && isDraggingShape)
                {
                    shapeEndPos = new Vector2Int(pixelX, pixelY);
                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.MouseDrag && !isShapeTool)
                {
                    ApplyToolAction(pixelX, pixelY, drawColor);
                    targetTexture.Apply();
                    e.Use();
                    Repaint();
                }
                else if (e.type == EventType.MouseUp && isDraggingShape)
                {
                    shapeEndPos = new Vector2Int(pixelX, pixelY);
                    Color commitColor = isRightClickAction ? Color.black : activeColor;
                    CommitShape(shapeStartPos, shapeEndPos, commitColor);
                    isDraggingShape = false;
                    targetTexture.Apply();
                    e.Use();
                    Repaint();
                }
            }
        }

        // Render dynamic drag preview overlay for shapes
        if (isDraggingShape)
        {
            DrawShapePreviewOverlay(canvasRect, effectiveZoomX, effectiveZoomY);
        }
    }

    private void DrawShapePreviewOverlay(Rect canvasRect, float cellWidth, float cellHeight)
    {
        Handles.BeginGUI();
        Color previewColor = isRightClickAction ? Color.black : activeColor;

        List<Vector2Int> previewPixels = GetShapePixels(shapeStartPos, shapeEndPos);

        foreach (var pixel in previewPixels)
        {
            float guiX = canvasRect.x + (pixel.x * cellWidth);
            float guiY = canvasRect.y + ((targetTexture.height - 1 - pixel.y) * cellHeight);

            Rect pixelRect = new Rect(guiX, guiY, cellWidth, cellHeight);
            EditorGUI.DrawRect(pixelRect, new Color(previewColor.r, previewColor.g, previewColor.b, 0.6f));
        }

        Handles.EndGUI();
    }

    private void CommitShape(Vector2Int start, Vector2Int end, Color color)
    {
        List<Vector2Int> pixels = GetShapePixels(start, end);
        foreach (var p in pixels)
        {
            targetTexture.SetPixel(p.x, p.y, color);
        }
    }

    private List<Vector2Int> GetShapePixels(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        if (selectedTool == ToolType.Line)
        {
            int x0 = start.x, y0 = start.y;
            int x1 = end.x, y1 = end.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                points.Add(new Vector2Int(x0, y0));
                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
        else if (selectedTool == ToolType.Rectangle)
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);

            for (int x = minX; x <= maxX; x++)
            {
                points.Add(new Vector2Int(x, minY));
                points.Add(new Vector2Int(x, maxY));
            }
            for (int y = minY + 1; y < maxY; y++)
            {
                points.Add(new Vector2Int(minX, y));
                points.Add(new Vector2Int(maxX, y));
            }
        }

        return points;
    }

    private void DrawGridOverlay(Rect canvasRect)
    {
        Handles.BeginGUI();
        Handles.color = gridColor;

        float cellWidth = canvasRect.width / targetTexture.width;
        float cellHeight = canvasRect.height / targetTexture.height;

        for (int x = 0; x <= targetTexture.width; x++)
        {
            float posX = canvasRect.x + (x * cellWidth);
            Handles.DrawLine(new Vector3(posX, canvasRect.y, 0), new Vector3(posX, canvasRect.y + canvasRect.height, 0));
        }

        for (int y = 0; y <= targetTexture.height; y++)
        {
            float posY = canvasRect.y + (y * cellHeight);
            Handles.DrawLine(new Vector3(canvasRect.x, posY, 0), new Vector3(canvasRect.x + canvasRect.width, posY, 0));
        }

        Handles.EndGUI();
    }

    private void CreateNewTexture()
    {
        targetTexture = new Texture2D(textureWidth, textureHeight)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black;

        targetTexture.SetPixels(pixels);
        targetTexture.Apply();
    }

    private void ApplyToolAction(int x, int y, Color drawColor)
    {
        switch (selectedTool)
        {
            case ToolType.Pencil:
                targetTexture.SetPixel(x, y, drawColor);
                break;

            case ToolType.Bucket:
                Color targetColor = targetTexture.GetPixel(x, y);
                FloodFill(targetTexture, x, y, targetColor, drawColor);
                break;
        }
    }

    private void FloodFill(Texture2D tex, int startX, int startY, Color targetColor, Color replacementColor)
    {
        if (targetColor == replacementColor) return;

        int w = tex.width;
        int h = tex.height;
        Queue<Vector2Int> pixels = new Queue<Vector2Int>();
        pixels.Enqueue(new Vector2Int(startX, startY));

        while (pixels.Count > 0)
        {
            Vector2Int pt = pixels.Dequeue();
            if (pt.x < 0 || pt.x >= w || pt.y < 0 || pt.y >= h) continue;

            if (tex.GetPixel(pt.x, pt.y) == targetColor)
            {
                tex.SetPixel(pt.x, pt.y, replacementColor);
                pixels.Enqueue(new Vector2Int(pt.x + 1, pt.y));
                pixels.Enqueue(new Vector2Int(pt.x - 1, pt.y));
                pixels.Enqueue(new Vector2Int(pt.x, pt.y + 1));
                pixels.Enqueue(new Vector2Int(pt.x, pt.y - 1));
            }
        }
    }

    private bool ColorEquals(Color c1, Color c2, float tolerance = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance &&
               Mathf.Abs(c1.a - c2.a) < tolerance;
    }

    private void SaveTextureToFolder()
    {
        if (targetTexture == null) return;

        string folderPath = Path.Combine(Application.dataPath, "Resources", "Rooms");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string currentName = string.IsNullOrWhiteSpace(fileName)
            ? "RoomMap_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss")
            : fileName.Trim();

        if (currentName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
        {
            currentName = currentName.Substring(0, currentName.Length - 4);
        }

        string fullPath = Path.Combine(folderPath, currentName + ".png");

        // Loop handles potential name collisions recursively if a newly entered name also exists
        while (File.Exists(fullPath))
        {
            int option = EditorUtility.DisplayDialogComplex(
                "File Already Exists",
                $"A room map named \"{currentName}.png\" already exists in Assets/Resources/Rooms.\nWhat would you like to do?",
                "Overwrite",    // Option 0
                "Rename File",  // Option 1
                "Cancel"       // Option 2
            );

            if (option == 2) // Cancel
            {
                return;
            }
            else if (option == 1) // Open Custom Input Window
            {
                string newNameEntered = null;
                RenameFileDialog.ShowWindow(currentName, (enteredName) =>
                {
                    newNameEntered = enteredName;
                });

                // If user closed dialog or provided empty string, abort save
                if (string.IsNullOrWhiteSpace(newNameEntered))
                {
                    return;
                }

                currentName = newNameEntered.Trim();
                if (currentName.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                {
                    currentName = currentName.Substring(0, currentName.Length - 4);
                }

                fileName = currentName; // Update main UI text field
                fullPath = Path.Combine(folderPath, currentName + ".png");
                // Loop continues to re-check if the custom entered name ALSO exists
            }
            else if (option == 0) // Overwrite
            {
                break;
            }
        }

        string finalFileName = currentName + ".png";
        byte[] bytes = targetTexture.EncodeToPNG();
        File.WriteAllBytes(fullPath, bytes);

        AssetDatabase.Refresh();

        string relativePath = "Assets/Resources/Rooms/" + finalFileName;
        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Debug.Log($"<color=green>Saved Room Texture to:</color> {relativePath}");
    }
}

// Dialog window for asking the user to type a new file name
public class RenameFileDialog : EditorWindow
{
    private string inputName;
    private System.Action<string> onConfirm;

    public static void ShowWindow(string currentName, System.Action<string> onConfirmAction)
    {
        RenameFileDialog window = CreateInstance<RenameFileDialog>();
        window.titleContent = new GUIContent("Rename Room File");
        window.inputName = currentName + "_Copy";
        window.onConfirm = onConfirmAction;

        Vector2 windowSize = new Vector2(350, 110);
        window.minSize = windowSize;
        window.maxSize = windowSize;

        // Center relative to the focused EditorWindow or main screen
        Rect positionRect = new Rect(Vector2.zero, windowSize);
        if (EditorWindow.focusedWindow != null)
        {
            Rect mainPos = EditorWindow.focusedWindow.position;
            positionRect.x = mainPos.x + (mainPos.width - windowSize.x) * 0.5f;
            positionRect.y = mainPos.y + (mainPos.height - windowSize.y) * 0.5f;
        }
        else
        {
            Resolution res = Screen.currentResolution;
            positionRect.x = (res.width - windowSize.x) * 0.5f;
            positionRect.y = (res.height - windowSize.y) * 0.5f;
        }

        window.position = positionRect;
        window.ShowModalUtility();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Enter a new file name for your room map:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        GUI.SetNextControlName("RenameField");
        inputName = EditorGUILayout.TextField("New Name", inputName);
        EditorGUI.FocusTextInControl("RenameField");

        EditorGUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save with New Name", GUILayout.Height(25)))
        {
            if (!string.IsNullOrWhiteSpace(inputName))
            {
                onConfirm?.Invoke(inputName);
                Close();
            }
        }

        if (GUILayout.Button("Cancel", GUILayout.Height(25)))
        {
            onConfirm?.Invoke(null);
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}