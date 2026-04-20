using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    public class AStarPathfinder : BasePathfinder
    {
        public override List<PathNode> FindPath(Vector2Int startPos, Vector2Int targetPos)
        {
            Grid grid = GetGrid();
            if (grid == null)
            {
                return null;
            }

            PathNode startNode = grid.GetNode(startPos.x, startPos.y);
            PathNode targetNode = grid.GetNode(targetPos.x, targetPos.y);
            if (startNode == null || targetNode == null || !startNode.isWalkable || !targetNode.isWalkable)
            {
                return null;
            }

            List<PathNode> openSet = new List<PathNode>();
            HashSet<PathNode> closedSet = new HashSet<PathNode>();

            foreach (var node in grid.GetAllNodes())
            {
                node.ResetCosts();
            }

            startNode.gCost = 0f;
            startNode.hCost = CalculateHeuristic(startNode, targetNode);

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || 
                        (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
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

                foreach (PathNode neighbor in grid.GetNeighbors(currentNode))
                {
                    if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float newMovementCostToNeighbor = currentNode.gCost + CalculateStepCost(currentNode, neighbor) + neighbor.movementCost;
                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = CalculateHeuristic(neighbor, targetNode);
                        neighbor.parentNode = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null; // No path found
        }

        private float CalculateStepCost(PathNode fromNode, PathNode toNode)
        {
            int xDistance = Mathf.Abs(fromNode.gridPosition.x - toNode.gridPosition.x);
            int yDistance = Mathf.Abs(fromNode.gridPosition.y - toNode.gridPosition.y);

            return xDistance == 1 && yDistance == 1 ? 1.4f : 1f;
        }

        private float CalculateHeuristic(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
            int yDistance = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
            int diagonalMoves = Mathf.Min(xDistance, yDistance);
            int straightMoves = Mathf.Abs(xDistance - yDistance);

            return diagonalMoves * 1.4f + straightMoves;
        }
    }
}
