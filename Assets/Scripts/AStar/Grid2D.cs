using UnityEngine;
using System.Collections.Generic;

public class Grid2D : MonoBehaviour 
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    
    [Header("Gizmo Settings")]
    [SerializeField] private bool showGridGizmos = true;

    Node2D[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    // Public property so Pathfinding2D can assign the current calculated path for visualization
    public List<Node2D> Path { get; set; }

    void Awake() 
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    public void CreateGrid() 
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        grid = new Node2D[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++) 
        {
            for (int y = 0; y < gridSizeY; y++) 
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.85f, unwalkableMask);
                grid[x, y] = new Node2D(walkable, worldPoint, x, y);
            }
        }
    }

    public List<Node2D> GetNeighbors(Node2D node) 
    {
        List<Node2D> neighbors = new List<Node2D>();

        for (int x = -1; x <= 1; x++) 
        {
            for (int y = -1; y <= 1; y++) 
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) 
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    public Node2D NodeFromWorldPoint(Vector3 worldPosition) 
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (!showGridGizmos || grid == null) return;

        foreach (Node2D n in grid)
        {
            // Color unwalkable nodes red, walkable nodes white
            Gizmos.color = n.walkable ? new Color(1, 1, 1, 0.2f) : new Color(1, 0, 0, 0.4f);

            if (Path != null && Path.Contains(n))
            {
                Gizmos.color = Color.black;
            }

            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.05f));
        }
    }
}