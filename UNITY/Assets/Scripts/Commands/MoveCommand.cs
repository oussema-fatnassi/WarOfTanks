using System.Collections.Generic;
using UnityEngine;

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
    public Vector3 Destination => _destination;

    public MoveCommand(ITankComponents tank, Vector3 destination)
    {
        _tank = tank;
        _destination = destination;
        _isComplete = false;
    }

    public void Start() 
    { 
        _waypoints = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _destination, _tank.GetBlockedCells(_tank.Controller.transform.position));
        _waypointIndex = 0;
        _lastProgressTime = Time.time;
        _lastCheckedPosition = _tank.Controller.transform.position;
    }
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
            if (Vector2.Distance(currentPosition, _lastCheckedPosition) < TankConstants.WAYPOINT_ARRIVAL_THRESHOLD)
            {
                _waypoints = _tank.Navigation.ComputePath(currentPosition, _destination, _tank.GetBlockedCells(currentPosition));
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
    public void Cancel() { _tank.Controller.Stop(); }
}