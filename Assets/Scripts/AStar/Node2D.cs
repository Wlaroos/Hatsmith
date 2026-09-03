using UnityEngine;

public class Node2D 
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public Node2D parent;

    public int fCost => gCost + hCost;

    public Node2D(bool walkable, Vector3 worldPosition, int gridX, int gridY) 
    {
        this.walkable = walkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}