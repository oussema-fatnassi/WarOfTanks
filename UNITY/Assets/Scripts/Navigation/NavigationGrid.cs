using UnityEngine;
using System.Collections.Generic;

namespace WarOfTanks.Navigation
{
    /// <summary>
    /// Builds and owns the navigation grid. Samples Physics2D layers on Awake to determine
    /// walkability and hazard status for each cell.
    /// </summary>
    public class NavigationGrid : MonoBehaviour
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private float _cellSize;
        [SerializeField] private LayerMask _walkableLayer;
        [SerializeField] private LayerMask _unwalkableLayer;
        [SerializeField] private LayerMask _hazardLayer;
        [SerializeField, Range(0.1f, 1f)] private float _cellCheckRadiusMultiplier = 0.45f;
        [SerializeField] private bool _showGizmos;
        private PathNode[,] _nodes;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Returns the node at the given grid coordinates, or null if out of bounds.
        /// This method is used by pathfinding algorithms to access node data such as walkability and movement cost.
        /// The method includes bounds checking to prevent errors from invalid coordinates. 
        /// Pathfinding algorithms will typically call this method frequently, so it is optimized for fast access to node data.
        /// <param name="x">X coordinate of the grid cell.</param>
        /// <param name="y">Y coordinate of the grid cell.</param>
        /// <returns>The PathNode at the specified coordinates, or null if out of bounds.</returns>
        /// </summary>
        public PathNode GetNode(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return null;

            return _nodes[x, y];
        }

        /// <summary>
        /// Returns all 8-directional neighbors of the given node that are within grid bounds.
        /// This method is used by pathfinding algorithms to explore adjacent nodes when calculating paths.
        /// The method checks all 8 surrounding positions (including diagonals) and returns valid neighbors that are within the grid bounds. 
        /// Pathfinding algorithms will typically call this method for each node they evaluate, so it is optimized for fast retrieval of neighboring nodes.
        /// <param name="node">The node for which to find neighbors.</param>
        /// <returns>A list of neighboring PathNodes that are within grid bounds.</returns>
        /// </summary>
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
                    if (neighborNode == null)
                        continue;

                    if (i != 0 && j != 0)
                    {
                        PathNode adjacentX = GetNode(node.GridPosition.x + i, node.GridPosition.y);
                        PathNode adjacentY = GetNode(node.GridPosition.x, node.GridPosition.y + j);
                        if (adjacentX == null || !adjacentX.IsWalkable || adjacentY == null || !adjacentY.IsWalkable)
                            continue;
                    }

                    neighbors.Add(neighborNode);
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Converts a world position to the nearest grid coordinates.
        /// This method is used to translate between the game's world space and the grid-based navigation system.
        /// It calculates the grid coordinates by determining the offset from the grid's origin and dividing by
        /// the cell size. The resulting coordinates are clamped to ensure they fall within the grid bounds.
        /// Pathfinding algorithms and AI will typically call this method when they need to convert a target position in the world to grid coordinates for pathfinding purposes.
        /// <param name="worldPosition">The world position to convert.</param>
        /// <returns>The corresponding grid coordinates as a Vector2Int, clamped to the grid bounds.</returns>
        /// </summary>
        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3 gridOrigin = transform.position - new Vector3(_width * _cellSize / 2f, _height * _cellSize / 2f, 0);
            int x = Mathf.Clamp(Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / _cellSize), 0, _width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / _cellSize), 0, _height - 1);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Converts grid coordinates to the world position at the center of that cell.
        /// </summary>
        /// <param name="gridPosition">The grid coordinates to convert.</param>
        /// <returns>The corresponding world position.</returns>
        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            Vector3 gridOrigin = transform.position - new Vector3(_width * _cellSize / 2f, _height * _cellSize / 2f, 0);
            float x = gridPosition.x * _cellSize + _cellSize / 2f;
            float y = gridPosition.y * _cellSize + _cellSize / 2f;
            return gridOrigin + new Vector3(x, y, 0);
        }

        /// <summary>
        /// Populates the node array by sampling Physics2D layers at each cell's world position.
        /// The method iterates over the grid dimensions and calculates the world position for each cell. 
        /// It uses Physics2D.OverlapCircle to check for colliders on the walkable, unwalkable, and hazard layers to determine the properties of each node.
        /// Walkable nodes are those that have a collider on the walkable layer and no collider on the unwalkable layer. 
        /// Hazard nodes are those that have a collider on the hazard layer.
        /// </summary>
        public void InitializeGrid()
        {
            _nodes = new PathNode[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    Vector3 worldPosition = GridToWorldPosition(new Vector2Int(x, y));
                    float checkRadius = _cellSize * _cellCheckRadiusMultiplier;
                    bool isWalkable = Physics2D.OverlapCircle(worldPosition, checkRadius, _walkableLayer)
                                   && !Physics2D.OverlapCircle(worldPosition, checkRadius, _unwalkableLayer);

                    bool isHazard = Physics2D.OverlapCircle(worldPosition, checkRadius, _hazardLayer);
                    var node = new PathNode(new Vector2Int(x, y), isWalkable);
                    node.IsHazard = isHazard;
                    node.MovementCost = isHazard ? 3f : 1f;
                    _nodes[x, y] = node;
                }
            }
        }

        /// <summary>Returns true if the given grid coordinates are within bounds.</summary>
        /// This method is used to validate grid coordinates before accessing nodes. 
        /// It checks that the x and y coordinates are non-negative and less than the grid's width and height, respectively.
        /// Pathfinding algorithms will typically call this method before attempting to access a node at specific coordinates to prevent out-of-bounds errors.
        /// <param name="x">X coordinate of the grid cell.</param>
        /// <param name="y">Y coordinate of the grid cell.</param>
        /// <returns>True if the coordinates are within the grid bounds, false otherwise.</returns>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        /// <summary>Iterates over all nodes in the grid.</summary>
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
