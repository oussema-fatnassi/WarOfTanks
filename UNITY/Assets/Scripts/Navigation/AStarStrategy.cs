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
            return new List<Vector2> { to }; 
        }
        
        Vector2Int startGrid = _grid.WorldToGridPosition(from);
        Vector2Int targetGrid = _grid.WorldToGridPosition(to);
        List<PathNode> pathNodes = _pathfinder.FindPath(startGrid, targetGrid, blocked);
        
        if (pathNodes == null || pathNodes.Count == 0)
        { 
            DebugLogger.LogWarning($"No path found from {from} to {to} using A* Pathfinder.");
            return new List<Vector2> { to };
        }
        
        List<Vector2> path = new List<Vector2>();
        foreach (var node in pathNodes)
        {
            Vector2 worldPos = _grid.GridToWorldPosition(node.GridPosition);
            path.Add(worldPos);
        }
        path[path.Count - 1] = to;
        return path;
    }
}
