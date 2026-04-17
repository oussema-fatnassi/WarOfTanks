using UnityEngine;

namespace WarOfTanks.Navigation
{
    public class PathNode
    {
        public Vector2Int gridPosition;
        public bool isWalkable;
        public float movementCost;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
        public PathNode parentNode;

        public PathNode(Vector2Int gridPosition, bool isWalkable)
        {
            this.gridPosition = gridPosition;
            this.isWalkable = isWalkable;
        }

        public void ResetCosts()
        {
            gCost = 0;
            hCost = 0;
        }
    }
}
