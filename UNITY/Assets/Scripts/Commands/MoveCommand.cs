using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Command that navigates a tank to a fixed world-space destination using A* pathfinding.
/// Advances through a waypoint list each tick and re-plans automatically when a stall is
/// detected. Completes once the final waypoint is reached or the destination is unreachable.
/// </summary>
public class MoveCommand : ICommand
{
    private ITankComponents _tank;
    private Vector3 _destination;
    private List<Vector2> _waypoints;
    private int _waypointIndex;
    private bool _isComplete;
    private float _lastProgressTime;
    private Vector2 _lastCheckedPosition;

    public bool IsComplete => _isComplete;

    /// <summary>World-space destination for this command, including any formation offset applied by CommandDispatcher.</summary>
    public Vector3 Destination => _destination;

    /// <param name="tank">Tank component bundle providing controller, turret, and navigation.</param>
    /// <param name="destination">World-space target position (may include a formation offset from CommandDispatcher).</param>
    public MoveCommand(ITankComponents tank, Vector3 destination)
    {
        _tank = tank;
        _destination = destination;
    }

    /// <summary>
    /// Computes the initial A* path from the tank's current position to the destination,
    /// including blocked cells from nearby friendly tanks. Called once when the command is assigned.
    /// </summary>
    public void Start()
    {
        _waypoints = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _destination, _tank.GetBlockedCells(_tank.Controller.transform.position));
        _waypointIndex = 0;
        _lastProgressTime = Time.time;
        _lastCheckedPosition = _tank.Controller.transform.position;
    }

    /// <summary>
    /// Advances the tank along the waypoint list each frame. Every <see cref="TankConstants.STALL_CHECK_INTERVAL"/>
    /// seconds checks whether the tank has moved enough; if not, recomputes the path. Falls back to
    /// a static-obstacle-only path if dynamic blocked cells make all routes impassable.
    /// Marks the command complete when all waypoints are consumed or the destination is unreachable.
    /// </summary>
    public void Tick()
    {
        if (_waypointIndex >= _waypoints.Count)
        {
            _isComplete = true;
            Cancel();
            return;
        }

        Vector2 currentPosition = _tank.Controller.transform.position;
        if (Time.time - _lastProgressTime >= TankConstants.STALL_CHECK_INTERVAL)
        {
            // STALL_DISTANCE_THRESHOLD is larger than WAYPOINT_ARRIVAL_THRESHOLD because two Dynamic
            // Rigidbody2D tanks pressing against each other via MovePosition can produce ~0.1–0.3 f of
            // net oscillation per interval without making real forward progress.
            if (Vector2.Distance(currentPosition, _lastCheckedPosition) < TankConstants.STALL_DISTANCE_THRESHOLD)
            {
                _waypoints = _tank.Navigation.ComputePath(currentPosition, _destination, _tank.GetBlockedCells(currentPosition));
                // A nearby friendly tank combined with a static obstacle can block every A* route.
                // Falling back to static-only pathfinding lets the tank make progress rather than giving up.
                if (_waypoints.Count == 0)
                    _waypoints = _tank.Navigation.ComputePath(currentPosition, _destination);
                _waypointIndex = 0;
                if (_waypoints.Count == 0) { _isComplete = true; Cancel(); return; }
            }
            _lastCheckedPosition = currentPosition;
            _lastProgressTime = Time.time;
        }

        Vector2 currentWaypoint = _waypoints[_waypointIndex];
        if (Vector2.Distance(currentPosition, currentWaypoint) < TankConstants.WAYPOINT_ARRIVAL_THRESHOLD)
        {
            _waypointIndex++;
            return;
        }

        _tank.Controller.Move(_waypoints[_waypointIndex] - currentPosition);
        _tank.Controller.RotateToward(_waypoints[_waypointIndex] - currentPosition);
    }

    /// <summary>Cancels movement by stopping the tank's controller.</summary>
    public void Cancel() { _tank.Controller.Stop(); }
}
