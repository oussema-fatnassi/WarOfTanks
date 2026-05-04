using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Navigation;
using WarOfTanks.AI;

/// <summary>
/// Test script for TankAI path following and obstacle avoidance.
/// Press Space to send the tank to the target Transform position.
/// Draws the active path (yellow), current waypoint (magenta), and detection raycast (red) in the editor.
/// </summary>
public class TankAITest : MonoBehaviour
{
    [SerializeField] private TankAI _tankAI;
    [SerializeField] private Transform _target;
    [SerializeField] private NavigationGrid _grid;
    [SerializeField] private Transform _mockObstacle;

    private void Awake()
    {
        if (_grid == null)
            _grid = FindObjectOfType<NavigationGrid>();
    }

    private void Update()
    {
        if (_tankAI == null || _target == null || _grid == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2Int targetGrid = _grid.WorldToGridPosition(_target.position);
            _tankAI.SetDestination(targetGrid);
        }
    }

    private void OnDrawGizmos()
    {
        if (_tankAI == null || _grid == null)
            return;

        List<PathNode> path = _tankAI.CurrentPath;
        int pathIndex = _tankAI.CurrentPathIndex;

        if (path == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (PathNode node in path)
        {
            Vector3 worldPos = _grid.GridToWorldPosition(node.GridPosition);
            Gizmos.DrawCube(worldPos, Vector3.one * 0.3f);
        }

        if (pathIndex < path.Count)
        {
            Vector3 currentTarget = _grid.GridToWorldPosition(path[pathIndex].GridPosition);

            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(currentTarget, Vector3.one * 0.5f);

            Vector3 direction = (currentTarget - _tankAI.transform.position).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_tankAI.transform.position, direction * _tankAI.DetectionRange);
            Gizmos.DrawWireSphere(_tankAI.transform.position + direction * _tankAI.DetectionRange, _tankAI.TankRadius);
        }

        if (_mockObstacle != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_mockObstacle.position, 0.5f);
        }
    }
}
