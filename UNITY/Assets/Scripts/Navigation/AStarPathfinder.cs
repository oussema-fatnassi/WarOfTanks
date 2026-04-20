using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    /// <summary>
    /// A* pathfinding implementation. Finds the shortest weighted path between two grid positions,
    /// accounting for terrain movement cost and diagonal movement (1.4x cost).
    /// </summary>
    public class AStarPathfinder : BasePathfinder
    {
        /// <summary>
        /// Finds the optimal path from start to target using the A* algorithm.
        /// The algorithm maintains an open set of nodes to explore and a closed set of nodes already evaluated.
        /// It calculates GCost (actual cost from start to current node) and HCost (estimated cost from current node to target) for each node, using the octile distance heuristic for HCost.
        /// The algorithm prioritizes nodes with the lowest FCost (GCost + HCost) and retraces the path from target to start once the target is reached.
        /// If no path exists, it returns null. The method also handles edge cases such as invalid start/target positions and unwalkable nodes.
        /// </summary>
        /// <param name="startPos">Grid coordinates of the start cell.</param>
        /// <param name="targetPos">Grid coordinates of the target cell.</param>
        /// <returns>Ordered list of nodes forming the path, or null if no path exists.</returns>
        public override List<PathNode> FindPath(Vector2Int startPos, Vector2Int targetPos)
        {
            Grid grid = GetGrid();
            if (grid == null)
                return null;

            PathNode startNode = grid.GetNode(startPos.x, startPos.y);
            PathNode targetNode = grid.GetNode(targetPos.x, targetPos.y);
            if (startNode == null || targetNode == null || !startNode.IsWalkable || !targetNode.IsWalkable)
                return null;

            List<PathNode> openSet = new List<PathNode>();
            HashSet<PathNode> closedSet = new HashSet<PathNode>();

            foreach (var node in grid.GetAllNodes())
                node.ResetCosts();

            startNode.GCost = 0f;
            startNode.HCost = CalculateHeuristic(startNode, targetNode);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost ||
                        (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                    return RetracePath(startNode, targetNode);

                foreach (PathNode neighbor in grid.GetNeighbors(currentNode))
                {
                    if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                        continue;

                    float newG = currentNode.GCost + CalculateStepCost(currentNode, neighbor) + neighbor.MovementCost;
                    if (newG < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newG;
                        neighbor.HCost = CalculateHeuristic(neighbor, targetNode);
                        neighbor.ParentNode = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        /// <summary>Returns 1.4 for diagonal moves, 1.0 for straight moves.
        /// This accounts for the increased distance of diagonal movement on a grid.
        /// Diagonal moves are more expensive than straight moves, which encourages more natural paths.
        /// The base movement cost of the neighbor node is added to account for terrain difficulty (e.g., hazards).
        /// </summary>
        /// <param name="from">The node we are moving from.</param>
        /// <param name="to">The node we are moving to.</param>
        /// <returns>The total movement cost to move from the 'from' node to the 'to' node.</returns>
        private float CalculateStepCost(PathNode from, PathNode to)
        {
            int dx = Mathf.Abs(from.GridPosition.x - to.GridPosition.x);
            int dy = Mathf.Abs(from.GridPosition.y - to.GridPosition.y);
            return dx == 1 && dy == 1 ? 1.4f : 1f;
        }

        /// <summary>Octile distance heuristic — optimal for 8-direction grids.</summary>
        /// Calculates the estimated cost to reach the target node from the current node, using the octile distance formula.
        /// This heuristic is admissible and consistent for grids that allow diagonal movement, ensuring optimal paths.
        /// The heuristic is calculated as follows:
        /// - Calculate the absolute differences in x and y coordinates (dx and dy).
        /// - Determine the number of diagonal steps possible (the smaller of dx and dy).
        /// - Calculate the remaining straight steps after taking the diagonal steps.
        /// - The total heuristic cost is the cost of the diagonal steps (diagonal * 1.4) plus the cost of the straight steps (straight * 1).
        /// This heuristic effectively estimates the distance to the target while accounting for the possibility of diagonal movement, leading to more efficient pathfinding on grids that allow it.
        /// </summary>
        /// <param name="a">The node from which we are calculating the heuristic.</param>
        /// <param name="b">The target node we are trying to reach.</param>
        /// <returns>The estimated cost to reach the target node from the current node.</returns>
        private float CalculateHeuristic(PathNode a, PathNode b)
        {
            int dx = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
            int dy = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
            int diagonal = Mathf.Min(dx, dy);
            int straight = Mathf.Abs(dx - dy);
            return diagonal * 1.4f + straight;
        }
    }
}
