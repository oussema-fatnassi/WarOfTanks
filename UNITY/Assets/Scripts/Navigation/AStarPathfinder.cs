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
            if (startNode == null || targetNode == null || !startNode.IsWalkable || !targetNode.IsWalkable)
            {
                return null;
            }

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

        private float CalculateStepCost(PathNode from, PathNode to)
        {
            int dx = Mathf.Abs(from.GridPosition.x - to.GridPosition.x);
            int dy = Mathf.Abs(from.GridPosition.y - to.GridPosition.y);
            return dx == 1 && dy == 1 ? 1.4f : 1f;
        }

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
