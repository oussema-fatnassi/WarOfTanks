using System.Collections.Generic;
using UnityEngine;

public class AttackCommand : ICommand
{
    private ITankComponents _tank;
    private ISelectable _target;
    private List<Vector2> _path;
    private int _waypointIndex;
    private Vector2 _lastTargetPos;
    private const float STALE_THRESHOLD = 0.5f;

    public AttackCommand(ITankComponents tank, ISelectable target)
    {
        _tank = tank;
        _target = target;
    }
    public void Start() 
    {
        _lastTargetPos = (Vector2)_target.GetWorldPosition();
        _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _lastTargetPos);
        _waypointIndex = 0;
    }

    public void Tick()
    {
        // Reference all variables needed in the frame loop
        Tank targetTank = _target as Tank;
        Vector2 currentTargetPos = (Vector2)_target.GetWorldPosition();
        Vector2 currentTankPosition = _tank.Controller.transform.position;



        //Check if the target is Dead
        if (targetTank != null && !targetTank.IsAlive)
        {
            Cancel();
            return;
        }

        // Check if target has moved significantly
        if (Vector2.Distance(currentTargetPos, _lastTargetPos) > STALE_THRESHOLD)
        {
            _lastTargetPos = currentTargetPos;
            _path = _tank.Navigation.ComputePath(_tank.Controller.transform.position, _lastTargetPos);
            _waypointIndex = 0;
        }

        // Check if is in firing range
        if (Vector2.Distance(currentTankPosition, _lastTargetPos) <= _tank.FiringRange)
        {
            _tank.Controller.Stop();
            _tank.Turret.RotateTo(_lastTargetPos);
            if (_tank.Turret.CanFire) _tank.Turret.Fire();
            return;
        }

        // Move towards target if not in range
        if (_waypointIndex >= _path.Count)
        {
            Cancel();
            return;
        }
        Vector2 currentWaypoint = _path[_waypointIndex];
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

    public void Cancel() 
    {
        _tank.Controller.Stop();
    }
}
