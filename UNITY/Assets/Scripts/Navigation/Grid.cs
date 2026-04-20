using UnityEngine;
using System.Collections.Generic;

namespace WarOfTanks.Navigation
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private float _cellSize;
        [SerializeField] private LayerMask _walkableLayer;
        [SerializeField] private LayerMask _unwalkableLayer;
        [SerializeField] private LayerMask _hazardLayer;
        [SerializeField] private bool _showGizmos;
        private PathNode[,] _nodes;

        private void Awake()
        {
           InitializeGrid();
        }

        public PathNode GetNode(int x, int y)
        {
            if (!IsValidPosition(x, y))
            {
                return null;
            }

            return _nodes[x, y];
        }

        public List<PathNode> GetNeighbors(PathNode node)
        {
            var neighbors = new List<PathNode>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborX = node.GridPosition.x + i;
                    int neighborY = node.GridPosition.y + j;
                    var neighborNode = GetNode(neighborX, neighborY);
                    if (neighborNode != null)
                        neighbors.Add(neighborNode);
                }
            }
            return neighbors;
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 gridOrigin = transform.position - new Vector3(_width * _cellSize / 2f, _height * _cellSize / 2f, 0);
            int x = Mathf.Clamp(Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / _cellSize), 0, _width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / _cellSize), 0, _height - 1);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            Vector3 gridOrigin = transform.position - new Vector3(_width * _cellSize / 2f, _height * _cellSize / 2f, 0);
            float x = gridPosition.x * _cellSize + _cellSize / 2f;
            float y = gridPosition.y * _cellSize + _cellSize / 2f;
            return gridOrigin + new Vector3(x, y, 0);
        }

        public void InitializeGrid()
        {
            _nodes = new PathNode[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector3 worldPosition = GridToWorldPosition(new Vector2Int(x, y));
                    bool isWalkable = Physics2D.OverlapCircle(worldPosition, _cellSize * 0.1f, _walkableLayer)
                                   && !Physics2D.OverlapCircle(worldPosition, _cellSize * 0.1f, _unwalkableLayer);

                    bool isHazard = Physics2D.OverlapCircle(worldPosition, _cellSize * 0.1f, _hazardLayer);
                    var node = new PathNode(new Vector2Int(x, y), isWalkable);
                    node.IsHazard = isHazard;
                    node.MovementCost = isHazard ? 3f : 1f;
                    _nodes[x, y] = node;
                }
            }
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public IEnumerable<PathNode> GetAllNodes()
        {
            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    yield return _nodes[x, y];
        }

        private void OnDrawGizmos()
        {
        #if UNITY_EDITOR
            if (_nodes == null || !_showGizmos) return;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var node = _nodes[x, y];
                    Gizmos.color = !node.IsWalkable ? Color.red : node.IsHazard ? Color.yellow : Color.green;
                    Vector3 worldPosition = GridToWorldPosition(node.GridPosition);
                    Gizmos.DrawCube(worldPosition, Vector3.one * (_cellSize - 0.1f));
                }
            }
        #endif
        }
    }
}
