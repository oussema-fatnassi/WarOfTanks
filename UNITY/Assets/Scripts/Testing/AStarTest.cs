using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Navigation;

/// <summary>
/// Test script to visualize A* pathfinding in the Unity editor. Allows setting start and target
/// positions via Transforms and draws the resulting path using Gizmos. The script retrieves the grid from the AStarPathfinder instance
/// and converts world positions to grid coordinates for pathfinding. It also includes error handling to ensure that the path is only drawn when valid data is available.
/// This script is intended for testing and debugging the A* pathfinding implementation by providing a visual
/// representation of the calculated path in the Unity editor. It can be attached to any GameObject, and the start and target Transforms can be assigned via the inspector.
/// The path will be drawn as yellow cubes at the center of each grid cell along the calculated path from the start to the target position.
/// </summary>
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
