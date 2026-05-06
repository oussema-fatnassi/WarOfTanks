using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Navigation;

/// <summary>
/// NavigationStrategy implementation that delegates to <see cref="AStarPathfinder"/> for
/// grid-based optimal pathfinding. Translates world-space Vector2 coordinates to and from
/// grid space, applies edge-case guards before calling FindPath, and returns an ordered list
/// of world-space waypoints ready for consumption by ICommand implementations.
/// </summary>
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

    /// <summary>Convenience overload — delegates to the blocked-cells version with null.</summary>
    public override List<Vector2> ComputePath(Vector2 from, Vector2 to)
      => ComputePath(from, to, null);

    /// <summary>
    /// Computes an A* path from <paramref name="from"/> to <paramref name="to"/> in world space.
    /// Applies three guards before calling FindPath:
    /// <list type="bullet">
    ///   <item><b>Same-cell guard</b> — returns an empty list when start and target share a grid cell, signalling the caller that the tank is already at the destination.</item>
    ///   <item><b>Start guard</b> — redirects a non-walkable start cell to its nearest walkable neighbour (border cells near the edge of Ground_Walkable can map as non-walkable when cellCheckRadiusMultiplier is small).</item>
    ///   <item><b>Target guard</b> — redirects a non-walkable target cell to its nearest walkable neighbour (formation offsets can land inside Cover or Obstacle tiles).</item>
    /// </list>
    /// Falls back to a straight-line path (<c>{ to }</c>) when no route exists.
    /// </summary>
    /// <param name="from">World-space start position.</param>
    /// <param name="to">World-space target position.</param>
    /// <param name="blocked">Optional set of grid positions to treat as impassable (e.g. friendly tanks). Null means no dynamic obstacles.</param>
    /// <returns>
    /// Ordered list of world-space waypoints ending at <paramref name="to"/>, or a single-element
    /// list pointing directly at <paramref name="to"/> when pathfinding fails.
    /// </returns>
    public override List<Vector2> ComputePath(Vector2 from, Vector2 to, HashSet<Vector2Int> blocked)
    {
        if (_pathfinder == null)
        {
            DebugLogger.LogWarning("AStarPathfinder is not initialized or grid reference is missing.");
            return new List<Vector2> { to };
        }

        Vector2Int startGrid = _grid.WorldToGridPosition(from);
        Vector2Int targetGrid = _grid.WorldToGridPosition(to);

        // Empty list signals the caller the tank is already at the destination cell;
        // MoveCommand's completion check (waypointIndex >= Count → 0 >= 0) fires immediately
        // without a FindPath call.
        if (startGrid == targetGrid)
            return new List<Vector2>();

        // Border cells near the edge of Ground_Walkable can map as non-walkable when
        // cellCheckRadiusMultiplier is small, even if the tank is physically standing there.
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

        // Tracked to skip the final waypoint snap (path[Count-1] = to) when the target was
        // redirected — otherwise it would overwrite the walkable destination with the original wall position.
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
            DebugLogger.LogWarning($"No path found from {from} to {to} using A* Pathfinder. {this}");
            return new List<Vector2> { to };
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

    /// <summary>
    /// Searches outward from <paramref name="center"/> in expanding shells until a walkable
    /// cell is found, up to a maximum radius of 5 cells. Shell-only iteration avoids
    /// re-checking inner cells already scanned at smaller radii.
    /// </summary>
    /// <param name="center">Grid coordinates of the non-walkable cell to search from.</param>
    /// <returns>Grid coordinates of the nearest walkable cell, or (-1, -1) if none found within radius 5.</returns>
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
