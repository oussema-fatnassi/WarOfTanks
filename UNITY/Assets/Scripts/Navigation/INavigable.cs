using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    /// <summary>
    /// Contract for all pathfinding algorithms. Allows TankAI and PathfinderFactory
    /// to remain decoupled from specific implementations (A*, Dijkstra, FlowField).
    /// </summary>
    public interface INavigable
    {
        /// <summary>
        /// Finds a path between two grid positions.
        /// </summary>
        /// <param name="startPosition">Grid coordinates of the start cell.</param>
        /// <param name="targetPosition">Grid coordinates of the target cell.</param>
        /// <returns>Ordered list of nodes from start to target, or null if no path exists.</returns>
        List<PathNode> FindPath(Vector2Int startPosition, Vector2Int targetPosition, HashSet<Vector2Int> blockedPositions = null);

        /// <summary>Returns the navigation grid used by this pathfinder.</summary>
        NavigationGrid GetGrid();
    }
}
