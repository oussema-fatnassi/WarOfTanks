using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    public class FlowFieldPathfinder : BasePathfinder
    {
        public FlowFieldPathfinder(NavigationGrid grid) : base(grid) { }

        public override List<PathNode> FindPath(Vector2Int startPos, Vector2Int targetPos, HashSet<Vector2Int> blockedPositions = null)
        {
            // Implement flow field pathfinding logic here
            return null;
        }
    }
}
