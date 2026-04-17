using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Navigation
{
    public abstract class BasePathfinder : MonoBehaviour, INavigable
    {
        private Grid _grid;

        protected virtual void Awake()
        {
            _grid = FindObjectOfType<Grid>();
        }

        public Grid GetGrid()
        {
            return _grid;
        }

        public abstract List<PathNode> FindPath(Vector2Int startPosition, Vector2Int targetPosition);

        protected List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
        {
            if (startNode == null || endNode == null)
            {
                return null;
            }

            var path = new List<PathNode>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                if (currentNode == null)
                {
                    return null;
                }

                path.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            path.Reverse();
            return path;
        }
    }
}
