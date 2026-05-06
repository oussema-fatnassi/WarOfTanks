using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Command that directs a tank to fire at a fixed world-space point. Behaviour depends on
/// whether the tank was already moving when the command was issued:
/// <list type="bullet">
///   <item><b>Moving:</b> the tank continues to its original destination while the turret rotates toward the zone point and fires without interrupting movement.</item>
///   <item><b>Stopped:</b> if the point is already within firing range, the tank stays and fires in place; otherwise it navigates to range first, then fires.</item>
/// </list>
/// This command never completes on its own — it must be cancelled explicitly via StopCommand (S key).
/// </summary>
public class AttackZoneCommand : ICommand
{
    private ITankComponents _tank;
    private Vector3 _targetPoint;
    private Vector3 _moveDestination;
    private List<Vector2> _path;
    private int _waypointIndex;
    // True when the tank had an active MoveCommand at issue time; the movement is preserved
    // and only the turret engages the zone point.
    private bool _isMoving;
    // Prevents Cancel()/Stop() from being called every frame once the path is exhausted,
    // since IsComplete is always false and Tick() continues running indefinitely.
    private bool _arrived;

    /// <summary>
    /// Always false — AttackZone is an indefinite command terminated only by StopCommand.
    /// </summary>
    public bool IsComplete => false;

    /// <param name="tank">Tank component bundle providing controller, turret, and navigation.</param>
    /// <param name="targetPoint">World-space point to aim and fire at.</param>
    /// <param name="moveDestination">
    /// If set, the tank's in-progress move destination; the tank continues navigating there
    /// while the turret fires at <paramref name="targetPoint"/>. Null means the tank was stationary.
    /// </param>
    public AttackZoneCommand(ITankComponents tank, Vector3 targetPoint, Vector3? moveDestination)
    {
        _tank = tank;
        _targetPoint = targetPoint;
        _isMoving = moveDestination.HasValue;
        if (_isMoving) { _moveDestination = moveDestination.Value; }
    }

    /// <summary>
    /// Computes the initial path. When moving, paths to the original move destination so movement
    /// is not interrupted. When stopped, paths to the zone point or skips pathing if already in range.
    /// </summary>
    public void Start()
    {
        if (_isMoving)
        {
            _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _moveDestination, _tank.GetBlockedCells(_tank.Controller.transform.position));
        }
        else
        {
            if (Vector2.Distance((Vector2)_tank.Controller.transform.position, (Vector2)_targetPoint) <= _tank.FiringRange)
            {
                _path = new List<Vector2>();
            }
            else
            {
                _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _targetPoint, _tank.GetBlockedCells(_tank.Controller.transform.position));
            }
        }
        _waypointIndex = 0;
    }

    /// <summary>
    /// Each frame: when moving, advances along the path and fires simultaneously; when stopped,
    /// fires in place if in range or moves toward the zone point until in range.
    /// </summary>
    public void Tick()
    {
        TurretController turret = _tank.Turret;

        if (_isMoving)
        {
            MoveAlongPath(_tank.Controller.transform.position);
            turret.RotateTo((Vector2)_targetPoint);
            if (turret.CanFire && turret.IsAimedAt((Vector2)_targetPoint, TankConstants.TURRET_TOLERANCE_ANGLE)) turret.Fire();
        }
        else
        {
            Vector2 currentTankPosition = _tank.Controller.transform.position;
            if (Vector2.Distance(currentTankPosition, (Vector2)_targetPoint) <= _tank.FiringRange)
            {
                _tank.Controller.Stop();
                turret.RotateTo((Vector2)_targetPoint);
                if (turret.CanFire && turret.IsAimedAt((Vector2)_targetPoint, TankConstants.TURRET_TOLERANCE_ANGLE)) turret.Fire();
            }
            else
            {
                MoveAlongPath(currentTankPosition);
            }
        }
    }

    /// <summary>Stops the tank's movement. The turret continues to aim and fire via Tick().</summary>
    public void Cancel()
    {
        _tank.Controller.Stop();
    }

    /// <summary>
    /// Advances the tank one step along the waypoint list. Calls <see cref="Cancel"/> once when
    /// the path is exhausted, then sets <c>_arrived</c> to prevent repeated Stop() calls on
    /// subsequent ticks while the command remains active.
    /// </summary>
    /// <param name="currentTankPosition">The tank's world-space position this frame.</param>
    private void MoveAlongPath(Vector2 currentTankPosition)
    {
        if (_waypointIndex >= _path.Count)
        {
            if (!_arrived) { Cancel(); _arrived = true; }
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
}
