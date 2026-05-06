using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Navigation;

public class AStarStrategy : NavigationStrategy
{
    [SerializeField] private NavigationGrid _grid;

    private AStarPathfinder _pathfinder;

    private void Awake()
    {
        if (_grid == null)
        {
            DebugLogger.LogError("NavigationGrid reference is missing on AStarStrategy.");
            _grid = FindObjectOfType<NavigationGrid>();
        }
        _pathfinder = new AStarPathfinder(_grid);
    }

    public override List<Vector2> ComputePath(Vector2 from, Vector2 to)
      => ComputePath(from, to, null);
    public override List<Vector2> ComputePath(Vector2 from, Vector2 to, HashSet<Vector2Int> blocked)
    {
        if (_pathfinder == null) 
        {
            DebugLogger.LogWarning("AStarPathfinder is not initialized or grid reference is missing.");
            //return new List<Vector2> { to }; 
            return new List<Vector2>();
        }

        Vector2Int startGrid = _grid.WorldToGridPosition(from);
        Vector2Int targetGrid = _grid.WorldToGridPosition(to);

        if (startGrid == targetGrid)
            return new List<Vector2>();

        PathNode startNode = _grid.GetNode(startGrid.x, startGrid.y);
        if (startNode != null && !startNode.IsWalkable)
        {
            startGrid = FindNearestWalkableGrid(startGrid);
            if (startGrid.x == -1)
            {
                DebugLogger.LogWarning($"No walkable cell near start {from}");
                //return new List<Vector2> { to };
                return new List<Vector2>();
            }
        }

        bool targetRedirected = false;
        PathNode targetNode = _grid.GetNode(targetGrid.x, targetGrid.y);
        if (targetNode != null && !targetNode.IsWalkable)
        {
            targetGrid = FindNearestWalkableGrid(targetGrid);
            targetRedirected = true;
            if (targetGrid.x == -1)
            {
                DebugLogger.LogWarning($"No walkable cell near {to}");
                //return new List<Vector2> { to };
                return new List<Vector2>();
            }
        }

        List<PathNode> pathNodes = _pathfinder.FindPath(startGrid, targetGrid, blocked);
        
        if (pathNodes == null || pathNodes.Count == 0)
        { 
            DebugLogger.LogWarning($"No path found from {from} to {to} using A* Pathfinder. {this}");
            //return new List<Vector2> { to };
            return new List<Vector2>();
        }

        List<Vector2> path = new List<Vector2>();
        foreach (var node in pathNodes)
        {
            Vector2 worldPos = _grid.GridToWorldPosition(node.GridPosition);
            path.Add(worldPos);
        }
        if (!targetRedirected)
            path[path.Count - 1] = to;
        return path;
    }

    private Vector2Int FindNearestWalkableGrid(Vector2Int center)
    {
        for (int radius = 1; radius <= 5; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;
                    int nx = center.x + dx;
                    int ny = center.y + dy;
                    if (!_grid.IsValidPosition(nx, ny)) continue;
                    PathNode node = _grid.GetNode(nx, ny);
                    if (node != null && node.IsWalkable)
                        return new Vector2Int(nx, ny);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
}
