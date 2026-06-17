using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.AI;

/// <summary>
/// Automatically aims and fires the turret at the nearest visible enemy.
/// The player retains full control of movement. Add this alongside VisionSystem on player tank prefabs.
/// </summary>
[RequireComponent(typeof(VisionSystem))]
[RequireComponent(typeof(TurretController))]
[RequireComponent(typeof(Tank))]
public class PlayerAutoAim : MonoBehaviour
{
    [SerializeField] private float _scanInterval = 0.1f;
    [SerializeField] private float _aimTolerance = 10f;
    [SerializeField] private bool _showDebugLogs;

    private VisionSystem _visionSystem;
    private TurretController _turretController;
    private Tank _tank;
    private DetectionResult _currentTarget;

    /// <summary>Caches required components.</summary>
    private void Awake()
    {
        _visionSystem = GetComponent<VisionSystem>();
        _turretController = GetComponent<TurretController>();
        _tank = GetComponent<Tank>();
    }

    /// <summary>Starts the repeating scan coroutine.</summary>
    private void Start()
    {
        StartCoroutine(ScanRoutine());
    }

    /// <summary>
    /// Repeatedly calls <see cref="UpdateTarget"/> on the configured scan interval.
    /// </summary>
    private IEnumerator ScanRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_scanInterval);
            UpdateTarget();
        }
    }

    /// <summary>
    /// Runs a vision scan and updates <see cref="_currentTarget"/> to the closest enemy with line of sight.
    /// </summary>
    private void UpdateTarget()
    {
        List<Tank> allTanks = GameManager.Instance != null
            ? GameManager.Instance.GetAllTanks()
            : new List<Tank>();

        List<DetectionResult> results = _visionSystem.Scan(allTanks, _tank.TeamId);

        List<DetectionResult> visibleEnemies = new List<DetectionResult>();
        foreach (DetectionResult r in results)
        {
            if (r.isInLineOfSight) visibleEnemies.Add(r);
        }

        _currentTarget = _visionSystem.GetClosestTarget(visibleEnemies);
        DebugLogger.Log(_showDebugLogs, $"[PlayerAutoAim] {gameObject.name} target: {(_currentTarget != null ? _currentTarget.target.name : "none")}");
    }

    /// <summary>
    /// Each frame, rotates the turret toward the current target and fires when aimed and ready.
    /// Clears the target if it dies or goes missing.
    /// </summary>
    private void Update()
    {
        if (_currentTarget == null || _currentTarget.target == null || !_currentTarget.target.IsAlive)
        {
            _currentTarget = null;
            return;
        }

        Vector2 targetPos = _currentTarget.target.transform.position;
        _turretController.RotateTo(targetPos);

        if (_turretController.IsAimedAt(targetPos, _aimTolerance) && _turretController.CanFire)
        {
            _turretController.Fire();
            DebugLogger.Log(_showDebugLogs, $"[PlayerAutoAim] {gameObject.name} fired at {_currentTarget.target.name}");
        }
    }
}
