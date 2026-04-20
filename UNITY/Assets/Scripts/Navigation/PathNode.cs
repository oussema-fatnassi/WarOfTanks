using UnityEngine;

namespace WarOfTanks.Navigation
{
    public class PathNode
    {
        private Vector2Int _gridPosition;
        private bool _isWalkable;
        private bool _isHazard;
        private float _movementCost;
        private float _gCost;
        private float _hCost;
        private PathNode _parentNode;

        public Vector2Int GridPosition => _gridPosition;
        public bool IsWalkable => _isWalkable;
        public bool IsHazard { get => _isHazard; set => _isHazard = value; }
        public float MovementCost { get => _movementCost; set => _movementCost = value; }
        public float GCost { get => _gCost; set => _gCost = value; }
        public float HCost { get => _hCost; set => _hCost = value; }
        public float FCost => _gCost + _hCost;
        public PathNode ParentNode { get => _parentNode; set => _parentNode = value; }

        public PathNode(Vector2Int gridPosition, bool isWalkable)
        {
            _gridPosition = gridPosition;
            _isWalkable = isWalkable;
            _movementCost = 1f;
            ResetCosts();
        }

        public void ResetCosts()
        {
            _gCost = float.PositiveInfinity;
            _hCost = 0f;
            _parentNode = null;
        }
    }
}
