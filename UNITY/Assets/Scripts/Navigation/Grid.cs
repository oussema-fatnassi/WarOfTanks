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
        private PathNode[,] _nodes;

        private void Awake()
        {
           InitializeGrid();
        }

        public PathNode GetNode(int x, int y)
        {
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

                    var neighborNode = GetNode(node.gridPosition.x + i, node.gridPosition.y + j);
                    if (neighborNode != null)
                    {
                        neighbors.Add(neighborNode);
                    }
                }
            }
            return neighbors;
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 gridOrigin = transform.position - new Vector3(_width * _cellSize / 2f, _height * _cellSize / 2f, 0);
            int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / _cellSize);
            int y = Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / _cellSize);
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
                    _nodes[x, y] = new PathNode(new Vector2Int(x, y), isWalkable);
                }
            }
        }

        private void OnDrawGizmos()
        {
        #if UNITY_EDITOR
            if (_nodes == null) return;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var node = _nodes[x, y];
                    Gizmos.color = node.isWalkable ? Color.green : Color.red;
                    Vector3 worldPosition = GridToWorldPosition(node.gridPosition);
                    Gizmos.DrawCube(worldPosition, Vector3.one * (_cellSize - 0.1f));
                }
            }
        #endif
        }
    }
}
