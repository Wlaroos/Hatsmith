using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding2D : MonoBehaviour 
{
    Grid2D grid;

    void Awake() 
    {
        grid = GetComponent<Grid2D>();
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos) 
    {
        Node2D startNode = grid.NodeFromWorldPoint(startPos);
        Node2D targetNode = grid.NodeFromWorldPoint(targetPos);

        if (!startNode.walkable) startNode = GetNearestWalkableNode(startNode);
        if (!targetNode.walkable) targetNode = GetNearestWalkableNode(targetNode);

        if (startNode == null || targetNode == null) return null;

        List<Node2D> openSet = new List<Node2D>();
        HashSet<Node2D> closedSet = new HashSet<Node2D>();
        openSet.Add(startNode);

        while (openSet.Count > 0) 
        {
            Node2D currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) 
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost) 
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) 
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node2D neighbor in grid.GetNeighbors(currentNode)) 
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) 
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    List<Vector3> RetracePath(Node2D startNode, Node2D endNode) 
    {
        List<Node2D> path = new List<Node2D>();
        Node2D currentNode = endNode;

        while (currentNode != startNode) 
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        // Smooth the raw node list into direct waypoints
        return SmoothPath(startNode, path);
    }

    List<Vector3> SmoothPath(Node2D startNode, List<Node2D> rawPath)
    {
        List<Vector3> waypoints = new List<Vector3>();
        if (rawPath == null || rawPath.Count == 0) return waypoints;

        Vector3 currentPoint = startNode.worldPosition;

        for (int i = 0; i < rawPath.Count - 1; i++)
        {
            // If there's an obstacle between currentPoint and the target node
            if (!HasLineOfSight(currentPoint, rawPath[i + 1].worldPosition))
            {
                currentPoint = rawPath[i].worldPosition;
                waypoints.Add(currentPoint);
            }
        }

        // Always append the final target destination
        waypoints.Add(rawPath[rawPath.Count - 1].worldPosition);
        return waypoints;
    }

    bool HasLineOfSight(Vector3 start, Vector3 end)
    {
        Vector2 dir = end - start;
        float distance = dir.magnitude;

        // Raycast using the unwalkable layer mask from Grid2D
        if (distance <= 0.001f) return true;

        RaycastHit2D hit = Physics2D.CircleCast(start, grid.nodeRadius * 0.4f, dir.normalized, distance, grid.unwalkableMask);
        
        if (hit.collider != null && hit.distance == 0)
        {
            return true;
        }

        return hit.collider == null;
    }

    Node2D GetNearestWalkableNode(Node2D node)
    {
        if (node.walkable) return node;

        List<Node2D> neighbors = grid.GetNeighbors(node);
        foreach (Node2D neighbor in neighbors)
        {
            if (neighbor.walkable) return neighbor;
        }

        return node;
    }

    int GetDistance(Node2D nodeA, Node2D nodeB) 
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}