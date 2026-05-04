using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    /// <summary>
    /// Abstract base class for all pathfinding algorithms. Handles grid resolution and path retracing.
    /// Subclasses implement the specific pathfinding logic in FindPath(). This design allows for easy swapping of algorithms without changing how the grid is accessed or how paths are retraced.
    /// </summary>
    public abstract class BasePathfinder : INavigable
    {
        private readonly NavigationGrid _grid;

        protected BasePathfinder(NavigationGrid grid)
        {
            _grid = grid;
        }

        /// <summary>Returns the navigation grid used by this pathfinder.</summary>
        public NavigationGrid GetGrid()
        {
            return _grid;
        }

        /// <summary>
        /// Finds a path between two grid positions. Implemented by each algorithm subclass.
        /// The method should return an ordered list of PathNodes from the start position to the target position, or null if no path exists.
        /// Implementations should handle edge cases such as invalid start/target positions and unwalkable nodes. 
        /// The pathfinding logic will typically involve maintaining open and closed sets of nodes, calculating movement costs
        /// and heuristics (for A*), and retracing the path once the target is reached. 
        /// The specific details will depend on the algorithm being implemented (e.g., Dijkstra, A*, Flow Field).
        /// </summary>
        /// <param name="startPosition">Grid coordinates of the start cell.</param>
        /// <param name="targetPosition">Grid coordinates of the target cell.</param>
        /// <returns>Ordered list of nodes from start to target, or null if no path exists.</returns>
        public abstract List<PathNode> FindPath(Vector2Int startPosition, Vector2Int targetPosition, HashSet<Vector2Int> blockedPositions = null);

        /// <summary>
        /// Reconstructs the path by walking back through each node's ParentNode from end to start.
        /// The resulting path is reversed to provide an ordered list from start to target. 
        /// This method is used by pathfinding algorithms like A* after reaching the target node.
        /// The method also includes error handling to return null if retracing fails due to null nodes, 
        /// which can occur in edge cases where the path cannot be properly constructed.
        /// </summary>
        /// <param name="startNode">The node where the path begins.</param>
        /// <param name="endNode">The node where the path ends.</param>
        /// <returns>Ordered list of nodes from start to target, or null if retracing fails.</returns>
        protected List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
        {
            if (startNode == null || endNode == null)
                return null;

            var path = new List<PathNode>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                if (currentNode == null)
                    return null;

                path.Add(currentNode);
                currentNode = currentNode.ParentNode;
            }

            path.Reverse();
            return path;
        }
    }
}
