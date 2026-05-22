using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Navigation;

/// <summary>
/// NavigationStrategy implementation that delegates to <see cref="FlowFieldPathfinder"/> for
/// grid-based flow field pathfinding. Computes a direction field from the target once, then
/// traces a discrete path by following vectors cell by cell. Translates world-space Vector2
/// coordinates to and from grid space, applies the same edge-case guards as AStarStrategy,
/// and returns an ordered list of world-space waypoints ready for consumption by ICommand implementations.
/// </summary>
public class FlowFieldStrategy : NavigationStrategy
{
    [SerializeField] private NavigationGrid _grid;

    private FlowFieldPathfinder _pathfinder;

    private void Awake()
    {
        if (_grid == null)
        {
            DebugLogger.LogError("NavigationGrid reference is missing on FlowFieldStrategy.");
            _grid = FindObjectOfType<NavigationGrid>();
        }
        _pathfinder = new FlowFieldPathfinder(_grid);
        _grid.RegisterFlowFieldForDebug(_pathfinder);
    }

    /// <summary>Convenience overload — delegates to the blocked-cells version with null.</summary>
    public override List<Vector2> ComputePath(Vector2 from, Vector2 to)
        => ComputePath(from, to, null);

    /// <summary>
    /// Computes a flow field path from <paramref name="from"/> to <paramref name="to"/> in world space.
    /// Applies three guards before calling FindPath:
    /// <list type="bullet">
    ///   <item><b>Same-cell guard</b> — returns an empty list when start and target share a grid cell.</item>
    ///   <item><b>Start guard</b> — redirects a non-walkable start cell to its nearest walkable neighbour.</item>
    ///   <item><b>Target guard</b> — redirects a non-walkable target cell to its nearest walkable neighbour.</item>
    /// </list>
    /// Falls back to a straight-line path (<c>{ to }</c>) when no route exists.
    /// </summary>
    public override List<Vector2> ComputePath(Vector2 from, Vector2 to, HashSet<Vector2Int> blocked)
    {
        if (_pathfinder == null)
        {
            DebugLogger.LogWarning("FlowFieldPathfinder is not initialized or grid reference is missing.");
            return new List<Vector2> { to };
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
                return new List<Vector2> { to };
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
                return new List<Vector2> { to };
            }
        }

        List<PathNode> pathNodes = _pathfinder.FindPath(startGrid, targetGrid, blocked);

        if (pathNodes == null || pathNodes.Count == 0)
        {
            DebugLogger.LogWarning($"No path found from {from} to {to} using FlowField. {this}");
            return new List<Vector2> { to };
        }

        List<Vector2> path = new List<Vector2>();
        foreach (PathNode node in pathNodes)
            path.Add(_grid.GridToWorldPosition(node.GridPosition));

        if (!targetRedirected)
            path[path.Count - 1] = to;

        return path;
    }

    /// <summary>
    /// Searches outward from <paramref name="center"/> in expanding shells until a walkable
    /// cell is found, up to a maximum radius of 5 cells.
    /// </summary>
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
