using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    public class DijkstraPathfinder : BasePathfinder
    {
        public DijkstraPathfinder(NavigationGrid grid) : base(grid) { }

        public override List<PathNode> FindPath(Vector2Int startPos, Vector2Int targetPos, HashSet<Vector2Int> blockedPositions = null)
        {
            // Implement Dijkstra pathfinding logic here
            return null;
        }
    }
}
