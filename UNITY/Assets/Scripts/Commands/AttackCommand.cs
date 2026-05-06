using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Command that drives a tank to pursue and fire at a specific enemy target. The tank navigates
/// to within <see cref="ITankComponents.FiringRange"/> using A* pathfinding, then stops, aims,
/// and fires. The path is recomputed whenever the target moves significantly or the current path
/// is exhausted. The command completes automatically when the target is dead.
/// </summary>
public class AttackCommand : ICommand
{
    private ITankComponents _tank;
    private ISelectable _target;
    private List<Vector2> _path;
    private int _waypointIndex;
    private Vector2 _lastTargetPos;
    private bool _isComplete;

    public bool IsComplete => _isComplete;

    /// <param name="tank">Tank component bundle providing controller, turret, and navigation.</param>
    /// <param name="target">The enemy unit to pursue and fire at.</param>
    public AttackCommand(ITankComponents tank, ISelectable target)
    {
        _tank = tank;
        _target = target;
    }

    /// <summary>
    /// Records the target's current position and computes the initial A* path toward it,
    /// including blocked cells from nearby friendly tanks.
    /// </summary>
    public void Start()
    {
        _lastTargetPos = (Vector2)_target.GetWorldPosition();
        _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _lastTargetPos, _tank.GetBlockedCells(_tank.Controller.transform.position));
        _waypointIndex = 0;
    }

    /// <summary>
    /// Each frame: verifies the target is still alive, updates the path when the target has moved
    /// beyond <see cref="TankConstants.STALE_THRESHOLD"/>, fires when in range, or advances along
    /// the current waypoint list. Recomputes A* when the path is exhausted.
    /// </summary>
    public void Tick()
    {
        // Death check must precede any call on _target to avoid MissingReferenceException
        // if the target MonoBehaviour was destroyed earlier in the same frame.
        Tank targetTank = _target as Tank;
        if (targetTank != null && !targetTank.IsAlive)
        {
            _isComplete = true;
            Cancel();
            return;
        }

        Vector2 currentTargetPos = (Vector2)_target.GetWorldPosition();
        Vector2 currentTankPosition = _tank.Controller.transform.position;

        // Recompute at most ~2x/s for moving targets; avoids running A* every frame for targets drifting slightly.
        if (Vector2.Distance(currentTargetPos, _lastTargetPos) > TankConstants.STALE_THRESHOLD)
        {
            _lastTargetPos = currentTargetPos;
            _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _lastTargetPos, _tank.GetBlockedCells(_tank.Controller.transform.position));
            _waypointIndex = 0;
        }

        if (Vector2.Distance(currentTankPosition, _lastTargetPos) <= _tank.FiringRange)
        {
            _tank.Controller.Stop();
            _tank.Turret.RotateTo(_lastTargetPos);
            if (_tank.Turret.CanFire && _tank.Turret.IsAimedAt(_lastTargetPos, TankConstants.TURRET_TOLERANCE_ANGLE)) _tank.Turret.Fire();
            return;
        }

        // Target may have moved far enough to exhaust the path without triggering the stale check;
        // recompute from the target's live position to continue pursuit.
        if (_waypointIndex >= _path.Count)
        {
            _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, (Vector2)_target.GetWorldPosition(), _tank.GetBlockedCells(_tank.Controller.transform.position));
            _waypointIndex = 0;
            return;
        }

        Vector2 currentWaypoint = _path[_waypointIndex];
        if (Vector2.Distance(currentTankPosition, currentWaypoint) < TankConstants.WAYPOINT_ARRIVAL_THRESHOLD)
        {
            _waypointIndex++;
            return;
        }

        _tank.Controller.Move(currentWaypoint - currentTankPosition);
        _tank.Controller.RotateToward(_path[_waypointIndex] - currentTankPosition);
    }

    /// <summary>Cancels the attack by stopping the tank's controller.</summary>
    public void Cancel()
    {
        _tank.Controller.Stop();
    }
}
