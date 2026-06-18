using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.AI.BehaviourTree;
using WarOfTanks.Enums;
using WarOfTanks.Fog;

namespace WarOfTanks.AI
{
    public partial class TankAI
    {
        /// <summary>
        /// Moves the tank toward the capture zone center.
        /// Returns <see cref="NodeStatus.Success"/> once the tank is inside the zone,
        /// <see cref="NodeStatus.Running"/> while pathing, and
        /// <see cref="NodeStatus.Failure"/> when the zone or navigation grid is missing.
        /// </summary>
        private NodeStatus MoveToZone()
        {
            if (_zone == null || _grid == null) 
                return NodeStatus.Failure;

            if (IsInZone())
            {
                return NodeStatus.Success;
            }

            Vector2Int targetGrid = _grid.WorldToGridPosition(_zone.transform.position);

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;
            
            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Retreats the tank to its spawn point and restores health when it arrives.
        /// Returns success after the spawn grid is reached so the role tree can resume
        /// normal behaviour on the next tick.
        /// </summary>
        private NodeStatus MoveToSpawn()
        {
            if (_tank == null || _grid == null) 
                return NodeStatus.Failure;
            
            Vector2Int targetGrid = _grid.WorldToGridPosition(_tank.SpawnPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
            {
                HealthSystem healthSystem = _tank.GetComponent<HealthSystem>();
                healthSystem?.RestoreHealth();
                return NodeStatus.Success;
            }
            
            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Moves the tank toward the closest visible enemy until it is inside firing range.
        /// Returns success when the target is close enough for the next sequence node
        /// to run <see cref="AttackClosestEnemy"/>.
        /// </summary>
        private NodeStatus MoveToFiringRange()
        {
            if (_blackboard == null || _blackboard.closestEnemy == null || _blackboard.closestEnemy.target == null || _grid == null || _tank == null)
                return NodeStatus.Failure;

            Tank enemy = _blackboard.closestEnemy.target;
            Vector2 enemyPosition = enemy.transform.position;
            Vector2 currentPosition = transform.position;

            if (Vector2.Distance(currentPosition, enemyPosition) <= _tank.FiringRange)
            {
                ClearPath();
                return NodeStatus.Success;
            }

            Vector2Int targetGrid = _grid.WorldToGridPosition(enemy.transform.position);

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Moves the tank to a team-specific perimeter point near the capture zone.
        /// The offset keeps defenders from stacking on the same center cell as captors.
        /// </summary>
        private NodeStatus MoveToZonePerimeter()
        {
            if (_zone == null || _grid == null || _blackboard == null)
                return NodeStatus.Failure;

            Vector3 offset = _blackboard.teamId == ETankTeam.PLAYER
                ? Vector3.left * 2f
                : Vector3.right * 2f;

            Vector3 perimeterPosition = _zone.transform.position + offset;
            Vector2Int targetGrid = _grid.WorldToGridPosition(perimeterPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Moves the tank toward the closest visible enemy so it can intercept and attack.
        /// Returns success when the tank reaches firing range, allowing the defender
        /// sequence to continue into <see cref="AttackClosestEnemy"/>.
        /// </summary>
        private NodeStatus MoveToIntercept()
        {
            if (_blackboard == null || _blackboard.closestEnemy == null || _blackboard.closestEnemy.target == null || _grid == null || _tank == null )
                return NodeStatus.Failure;
            
            Tank enemy = _blackboard.closestEnemy.target;
            Vector3 interceptPosition = enemy.transform.position;
            Vector2Int targetGrid = _grid.WorldToGridPosition(interceptPosition);

            if (Vector2.Distance(transform.position, enemy.transform.position) <= _tank.FiringRange)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Patrols toward the spawn point of an enemy tank.
        /// This gives attackers useful movement while the vision system stub returns
        /// no visible targets.
        /// </summary>
        private NodeStatus PatrolToEnemySpawn()
        {
            if (_blackboard == null || _grid == null || _tank == null || GameManager.Instance == null)
                return NodeStatus.Failure;

            List<Tank> allTanks = GameManager.Instance.GetAllTanks();

            Tank enemyTank = null;
            foreach (Tank tank in allTanks)
            {
                if (tank == null)
                    continue;

                if (tank.TeamId == _blackboard.enemyTeamId)
                {
                    enemyTank = tank;
                    break;
                }
            }
            if (enemyTank == null)
                return NodeStatus.Failure;

            Vector3 enemySpawnPosition = enemyTank.SpawnPosition;
            Vector2Int targetGrid = _grid.WorldToGridPosition(enemySpawnPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Patrols between the capture zone perimeter and the tank spawn point.
        /// This is the defender fallback when there is no visible threat and no
        /// higher-priority zone action is active.
        /// </summary>
        private NodeStatus PatrolZoneToSpawn()
        {
            if (_blackboard == null || _tank == null || _grid == null || _zone == null)
                return NodeStatus.Failure;

            Vector3 offset = _blackboard.teamId == ETankTeam.PLAYER
                ? Vector3.left * 2f
                : Vector3.right * 2f;

            Vector3 targetPosition = IsInZone()
                ? _tank.SpawnPosition
                : _zone.transform.position + offset;

            Vector2Int targetGrid = _grid.WorldToGridPosition(targetPosition);
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);

            if (currentGrid == targetGrid)
                return NodeStatus.Success;

            if (IsAlreadyMovingTo(targetGrid))
                return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Rotates the turret toward the closest visible enemy and fires when aimed and ready.
        /// The tank body keeps following its current path; this action only controls
        /// turret rotation and firing.
        /// </summary>
        private NodeStatus AttackClosestEnemy()
        {
            if (_blackboard == null || _blackboard.closestEnemy == null || _blackboard.closestEnemy.target == null || _tank == null || _tank.Turret == null)
                return NodeStatus.Failure;

            Tank enemy = _blackboard.closestEnemy.target;
            if (_tank.TeamId == ETankTeam.PLAYER && !FogOfWarManager.CanTarget(enemy))
                return NodeStatus.Failure;

            Vector2 targetPosition = enemy.transform.position;

            _tank.Turret.RotateTo(targetPosition);

            if (_tank.Turret.CanFire && _tank.Turret.IsAimedAt(targetPosition, TankConstants.TURRET_TOLERANCE_ANGLE))
            {
                _tank.Turret.Fire();
            }

            return NodeStatus.Success;
        }

        /// <summary>
        /// Patrols back and forth between own spawn and the nearest enemy spawn.
        /// Toggles direction each time the tank reaches the current waypoint.
        /// Falls back to patrol when the attacker loses line of sight on all enemies.
        /// </summary>
        private NodeStatus PatrolBetweenSpawns()
        {
            if (_blackboard == null || _grid == null || _tank == null || GameManager.Instance == null)
                return NodeStatus.Failure;

            List<Tank> allTanks = GameManager.Instance.GetAllTanks();

            Tank enemyTank = null;
            foreach (Tank tank in allTanks)
            {
                if (tank != null && tank.TeamId == _blackboard.enemyTeamId)
                {
                    enemyTank = tank;
                    break;
                }
            }

            if (enemyTank == null) return NodeStatus.Failure;

            Vector3 enemySpawn = enemyTank.SpawnPosition;
            Vector3 ownSpawn = _tank.SpawnPosition;
            Vector3 currentWaypoint = _patrolTowardEnemy ? enemySpawn : ownSpawn;

            if (Vector2.Distance(transform.position, currentWaypoint) < 1f)
            {
                _patrolTowardEnemy = !_patrolTowardEnemy;
                currentWaypoint = _patrolTowardEnemy ? enemySpawn : ownSpawn;
            }

            Vector2Int targetGrid = _grid.WorldToGridPosition(currentWaypoint);

            if (IsAlreadyMovingTo(targetGrid)) return NodeStatus.Running;

            SetDestination(targetGrid);
            return NodeStatus.Running;
        }

        /// <summary>
        /// Executes the full aggression order by attacking a visible enemy or patrolling between spawn points.
        /// </summary>
        /// <returns>
        /// <see cref="NodeStatus.Success"/> when the attack action completes,
        /// <see cref="NodeStatus.Running"/> while moving, or
        /// <see cref="NodeStatus.Failure"/> when no valid aggressive action is available.
        /// </returns>
        private NodeStatus ExecuteFullAggressionOrder()
        {
            bool hasVisibleEnemy =
                _blackboard != null &&
                _blackboard.closestEnemy != null &&
                _blackboard.closestEnemy.target != null &&
                _blackboard.closestEnemy.isInLineOfSight;

            if (!hasVisibleEnemy)
            {
                return PatrolBetweenSpawns();
            }

            NodeStatus moveResult = MoveToFiringRange();

            if (moveResult != NodeStatus.Success)
            {
                return moveResult;
            }

            return AttackClosestEnemy();
        }

        /// <summary>
        /// Placeholder action for future commander signalling when an enemy is visible.
        /// Returns success so the captor keeps the signal branch active until the
        /// commander AI is implemented.
        /// </summary>
        private NodeStatus SignalEnemyVisible()
        {
            return NodeStatus.Success;
        }

        /// <summary>
        /// Executes the current commander order and clears it once the ordered action succeeds.
        /// </summary>
        /// <returns>
        /// The status returned by the action mapped to the current strategic order,
        /// or <see cref="NodeStatus.Failure"/> when there is no active order.
        /// </returns>
        private NodeStatus ExecuteStrategicOrder()
        {
            NodeStatus result;

            switch (_strategicOrder)
            {
                case EStrategicOrder.CAPTUREZONE:
                    result = MoveToZone();
                    break;

                case EStrategicOrder.DEFENDZONE:
                    result = MoveToZonePerimeter();
                    break;

                case EStrategicOrder.FALLBACK:
                    result = MoveToSpawn();
                    break;

                case EStrategicOrder.FULLAGGRESSION:
                    result = ExecuteFullAggressionOrder();
                    break;

                case EStrategicOrder.NONE:
                default:
                    return NodeStatus.Failure;
            }

            if (result == NodeStatus.Success)
            {
                _strategicOrder = EStrategicOrder.NONE;
            }

            return result;
        }
    }
}
