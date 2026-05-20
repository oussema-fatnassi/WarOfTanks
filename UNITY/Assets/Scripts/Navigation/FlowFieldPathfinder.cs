using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    public class FlowFieldPathfinder : BasePathfinder
    {
        private IntegrationField _integrationField;
        private FlowField _flowField;

        public FlowFieldPathfinder(NavigationGrid grid) : base(grid) { }

        public void ComputeFlowField(Vector2Int target)
        {
            NavigationGrid grid = GetGrid();
            _integrationField = new IntegrationField(grid.Width, grid.Height);
            _integrationField.Compute(target, grid);
            _flowField = new FlowField(grid.Width, grid.Height);
            _flowField.Build(_integrationField, grid);
        }

        public Vector2 GetDirectionAtWorldPos(Vector3 worldPos)
        {
            if (_flowField == null) return Vector2.zero;
            Vector2Int gridPos = GetGrid().WorldToGridPosition(worldPos);
            return _flowField.GetDirection(gridPos.x, gridPos.y);
        }

        public Vector2 GetDirectionAtGridPos(int x, int y)
        {
            if (_flowField == null) return Vector2.zero;
            return _flowField.GetDirection(x, y);
        }

        public override List<PathNode> FindPath(Vector2Int startPos, Vector2Int targetPos,
            HashSet<Vector2Int> blockedPositions = null)
        {
            NavigationGrid grid = GetGrid();
            if (grid == null) return null;

            PathNode startNode = grid.GetNode(startPos.x, startPos.y);
            PathNode targetNode = grid.GetNode(targetPos.x, targetPos.y);
            if (startNode == null || targetNode == null || !startNode.IsWalkable || !targetNode.IsWalkable)
                return null;

            if (startNode == targetNode) return new List<PathNode>();

            ComputeFlowField(targetPos);

            var path = new List<PathNode>();
            PathNode current = startNode;
            int maxIterations = grid.Width * grid.Height;

            while (current != targetNode && maxIterations-- > 0)
            {
                Vector2 dir = _flowField.GetDirection(current.GridPosition.x, current.GridPosition.y);
                if (dir == Vector2.zero) return null;

                int nx = current.GridPosition.x + Mathf.RoundToInt(dir.x);
                int ny = current.GridPosition.y + Mathf.RoundToInt(dir.y);
                PathNode next = grid.GetNode(nx, ny);
                if (next == null || !next.IsWalkable) return null;

                path.Add(next);
                current = next;
            }

            return maxIterations < 0 ? null : path;
        }

        // ─── Internal class: Integration Field ───────────────────────────────────
        // Holds the cost-to-target for every cell. Computed once via backwards Dijkstra from the target.

        private class IntegrationField
        {
            private readonly float[,] _costs;

            public int Width { get; }
            public int Height { get; }

            public IntegrationField(int width, int height)
            {
                Width = width;
                Height = height;
                _costs = new float[width, height];
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        _costs[x, y] = float.MaxValue;
            }

            public float GetCost(int x, int y) => _costs[x, y];

            public void Compute(Vector2Int target, NavigationGrid grid)
            {
                PathNode targetNode = grid.GetNode(target.x, target.y);
                if (targetNode == null || !targetNode.IsWalkable) return;

                _costs[target.x, target.y] = 0f;
                var queue = new Queue<PathNode>();
                queue.Enqueue(targetNode);

                while (queue.Count > 0)
                {
                    PathNode current = queue.Dequeue();
                    float currentCost = _costs[current.GridPosition.x, current.GridPosition.y];

                    foreach (PathNode neighbor in grid.GetNeighbors(current))
                    {
                        if (!neighbor.IsWalkable) continue;

                        int dx = Mathf.Abs(neighbor.GridPosition.x - current.GridPosition.x);
                        int dy = Mathf.Abs(neighbor.GridPosition.y - current.GridPosition.y);
                        float stepCost = dx == 1 && dy == 1 ? 1.4f : 1f;
                        float newCost = currentCost + stepCost + neighbor.MovementCost;

                        if (newCost < _costs[neighbor.GridPosition.x, neighbor.GridPosition.y])
                        {
                            _costs[neighbor.GridPosition.x, neighbor.GridPosition.y] = newCost;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        // ─── Internal class: Flow Field ───────────────────────────────────────────
        // Holds a direction vector per cell. Each cell points toward the neighbor with the lowest integration cost.

        private class FlowField
        {
            private readonly Vector2[,] _vectors;

            public FlowField(int width, int height)
            {
                _vectors = new Vector2[width, height];
            }

            public Vector2 GetDirection(int x, int y)
            {
                if (x < 0 || y < 0 || x >= _vectors.GetLength(0) || y >= _vectors.GetLength(1))
                    return Vector2.zero;
                return _vectors[x, y];
            }

            public void Build(IntegrationField integrationField, NavigationGrid grid)
            {
                for (int x = 0; x < integrationField.Width; x++)
                {
                    for (int y = 0; y < integrationField.Height; y++)
                    {
                        PathNode node = grid.GetNode(x, y);
                        if (node == null || !node.IsWalkable)
                        {
                            _vectors[x, y] = Vector2.zero;
                            continue;
                        }

                        float bestCost = integrationField.GetCost(x, y);
                        Vector2 bestDir = Vector2.zero;

                        foreach (PathNode neighbor in grid.GetNeighbors(node))
                        {
                            float cost = integrationField.GetCost(neighbor.GridPosition.x, neighbor.GridPosition.y);
                            if (cost < bestCost)
                            {
                                bestCost = cost;
                                bestDir = new Vector2(
                                    neighbor.GridPosition.x - x,
                                    neighbor.GridPosition.y - y
                                ).normalized;
                            }
                        }

                        _vectors[x, y] = bestDir;
                    }
                }
            }
        }
    }
}
