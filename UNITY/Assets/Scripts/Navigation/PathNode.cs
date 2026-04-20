using UnityEngine;

namespace WarOfTanks.Navigation
{
    /// <summary>
    /// Represents a single cell in the navigation grid used by pathfinding algorithms.
    /// </summary>
    public class PathNode
    {
        private Vector2Int _gridPosition;
        private bool _isWalkable;
        private bool _isHazard;
        private float _movementCost;
        private float _gCost;
        private float _hCost;
        private PathNode _parentNode;

        /// <summary>Grid coordinates of this node.</summary>
        public Vector2Int GridPosition => _gridPosition;

        /// <summary>Whether tanks can traverse this node.</summary>
        public bool IsWalkable => _isWalkable;

        /// <summary>Whether this node is a hazard tile (higher movement cost, reduces tank speed).</summary>
        public bool IsHazard { get => _isHazard; set => _isHazard = value; }

        /// <summary>Terrain cost multiplier applied when entering this node.</summary>
        public float MovementCost { get => _movementCost; set => _movementCost = value; }

        /// <summary>Accumulated cost from the start node to this node.</summary>
        public float GCost { get => _gCost; set => _gCost = value; }

        /// <summary>Estimated cost from this node to the target node (heuristic).</summary>
        public float HCost { get => _hCost; set => _hCost = value; }

        /// <summary>Total estimated path cost (GCost + HCost).</summary>
        public float FCost => _gCost + _hCost;

        /// <summary>Previous node in the path, used to retrace the path once the target is reached.</summary>
        public PathNode ParentNode { get => _parentNode; set => _parentNode = value; }

        /// <param name="gridPosition">Grid coordinates of this node.</param>
        /// <param name="isWalkable">Whether tanks can traverse this node.</param>
        public PathNode(Vector2Int gridPosition, bool isWalkable)
        {
            _gridPosition = gridPosition;
            _isWalkable = isWalkable;
            _movementCost = 1f;
            ResetCosts();
        }

        /// <summary>
        /// Resets pathfinding costs before each new search to avoid stale data from previous calls.
        /// </summary>
        public void ResetCosts()
        {
            _gCost = float.PositiveInfinity;
            _hCost = 0f;
            _parentNode = null;
        }
    }
}
