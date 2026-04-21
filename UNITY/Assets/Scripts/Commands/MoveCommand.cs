using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : ICommand
{
    private ITankComponents _tank;
    private Vector3 _destination;
    private List<Vector2> _waypoints;
    private int _waypointIndex;
    private const float _arrivalThreshold = 0.1f;

    public MoveCommand(ITankComponents tank, Vector3 destination)
    {
        _tank = tank;
        _destination = destination;
    }

    public void Start() 
    { 
       _waypoints = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _destination);
       _waypointIndex = 0;
    }
    public void Tick() 
    {
        if (_waypointIndex >= _waypoints.Count)
        {
            _tank.Controller.Stop();
            return;
        }

        Vector2 currentPosition = _tank.Controller.transform.position;
        Vector2 currentWaypoint = _waypoints[_waypointIndex];
        if (Vector2.Distance(currentPosition, currentWaypoint) < _arrivalThreshold)
        {
            _waypointIndex++;
            return;
        }
        else
        {
            _tank.Controller.Move(_waypoints[_waypointIndex] - currentPosition); 
        }
    }
    public void Cancel() { _tank.Controller.Stop(); }
}