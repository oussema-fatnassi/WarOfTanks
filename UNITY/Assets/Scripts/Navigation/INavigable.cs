using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    public interface INavigable
    {
        public List<PathNode> FindPath(Vector2Int startPosition, Vector2Int targetPosition);

        public Grid GetGrid();
    }
}
