using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackZoneCommand : ICommand
{
    private ITankComponents _tank;
    private Vector3 _targetPoint;
    private Vector3 _moveDestination;
    private List<Vector2> _path;
    private int _waypointIndex;
    private bool _isMoving;
    public AttackZoneCommand(ITankComponents tank, Vector3 targetPoint, Vector3? moveDestination)
    {
        _tank = tank;
        _targetPoint = targetPoint;
        _isMoving = moveDestination.HasValue;
        if (_isMoving) { _moveDestination = moveDestination.Value; }
    }

    public void Start()
    {
        if (_isMoving)
        {
            _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _moveDestination);
        }
        else
        {
            if (Vector2.Distance((Vector2)_tank.Controller.transform.position, (Vector2)_targetPoint) <= _tank.FiringRange)
            {
                _path = new List<Vector2>();
            }
            else 
            {
                _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _targetPoint);
            }
        }
        _waypointIndex = 0;
    }

    public void Tick()
    {
        TurretController turret = _tank.Turret;
        
        if (_isMoving)
        {
            MoveAlongPath(_tank.Controller.transform.position);

            turret.RotateTo((Vector2)_targetPoint);
            if (turret.CanFire) turret.Fire();
        }
        else
        {
            Vector2 currentTankPosition = _tank.Controller.transform.position;
            if (Vector2.Distance(currentTankPosition, (Vector2)_targetPoint) <= _tank.FiringRange)
            {
                Cancel();
                turret.RotateTo((Vector2)_targetPoint);
                if (turret.CanFire) turret.Fire();
            }
            else 
            { 
                MoveAlongPath(currentTankPosition);
            }
        }
    }

    public void Cancel()
    {
        _tank.Controller.Stop();
    }

    private void MoveAlongPath(Vector2 currentTankPosition)
    {
        Vector2 currentWaypoint = _path[_waypointIndex];
        if (_waypointIndex >= _path.Count)
        {
            Cancel();
            return;
        }
        if (Vector2.Distance(currentTankPosition, currentWaypoint) < TankConstants.WaypointArrivalThreshold)
        {
            _waypointIndex++;
            return;
        }
        else
        {
            _tank.Controller.Move(currentWaypoint - currentTankPosition);
        }
    }
}
