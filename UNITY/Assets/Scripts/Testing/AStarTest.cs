using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Navigation;

public class AStarTest : MonoBehaviour
{
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _target;
    [SerializeField] private AStarPathfinder _pathfinder;

    private List<PathNode> _path;

    private void Awake()
    {
        if (_pathfinder == null)
        {
            _pathfinder = FindObjectOfType<AStarPathfinder>();
        }
    }

    private void Update()
    {
        if (_pathfinder == null || _start == null || _target == null || _pathfinder.GetGrid() == null)
        {
            return;
        }

        Vector2Int startGrid = _pathfinder.GetGrid().WorldToGridPosition(_start.position);
        Vector2Int targetGrid = _pathfinder.GetGrid().WorldToGridPosition(_target.position);

        _path = _pathfinder.FindPath(startGrid, targetGrid);
    }

    private void OnDrawGizmos()
    {
        if (_path == null || _pathfinder == null || _pathfinder.GetGrid() == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        foreach (var node in _path)
        {
            Vector3 worldPos = _pathfinder.GetGrid().GridToWorldPosition(node.GridPosition);
            Gizmos.DrawCube(worldPos, Vector3.one * 0.5f);
        }
    }
}
