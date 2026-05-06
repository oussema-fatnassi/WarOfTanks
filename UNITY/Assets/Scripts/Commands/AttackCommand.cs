using System.Collections.Generic;
using UnityEngine;

public class AttackCommand : ICommand
{
    private ITankComponents _tank;
    private ISelectable _target;
    private List<Vector2> _path;
    private int _waypointIndex;
    private Vector2 _lastTargetPos;
    private bool _isComplete;
    public bool IsComplete => _isComplete;

    public AttackCommand(ITankComponents tank, ISelectable target)
    {
        _tank = tank;
        _target = target;
        _isComplete = false;
    }
    public void Start() 
    {
        _lastTargetPos = (Vector2)_target.GetWorldPosition();
        _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _lastTargetPos, _tank.GetBlockedCells(_tank.Controller.transform.position));
        _waypointIndex = 0;
    }

    public void Tick()
    {
        Tank targetTank = _target as Tank;
        Vector2 currentTargetPos = (Vector2)_target.GetWorldPosition();
        Vector2 currentTankPosition = _tank.Controller.transform.position;

        if (targetTank != null && !targetTank.IsAlive)
        {
            _isComplete = true;
            Cancel();
            return;
        }

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
        else
        {
            _tank.Controller.Move(currentWaypoint - currentTankPosition);
            _tank.Controller.RotateToward(_path[_waypointIndex] - currentTankPosition);
        }
    }

    public void Cancel() 
    {
        _tank.Controller.Stop();
    }
}
