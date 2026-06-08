using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.Enums;
using ZoneController = WarOfTanks.Zone.Zone;

namespace WarOfTanks.AI
{
    public class CommanderAI : MonoBehaviour
    {
        [SerializeField] private List<TankAI> _tanks;
        [SerializeField] private ZoneController _zone;
        [SerializeField] private float _evaluationInterval = 1f;
        [SerializeField] private float _enemyNearZoneRadius = 5f;
        [SerializeField] private ETankTeam _team;
        private float _evaluationTimer;

        private void Update()
        {
            _evaluationTimer += Time.deltaTime;
            if (_evaluationTimer >= _evaluationInterval)
            {
                _evaluationTimer = 0f;
                AssignRoles();
            }
        }

        private List<DetectionResult> AggregateBattlefield()
        {
            List<DetectionResult> results = new List<DetectionResult>();
            HashSet<Tank> seen = new HashSet<Tank>();
            foreach (var tank in _tanks)
            {
                if (tank == null || !tank.isActiveAndEnabled) continue;
                foreach (var result in tank.EnemyResults)
                {
                    if (result == null || result.target == null || seen.Contains(result.target)) continue;
                    seen.Add(result.target);
                    results.Add(result);
                }
            }
            return results;
        }

        private int GetAliveTankCount(ETankTeam team)
        {
            if (GameManager.Instance == null)
                return 0;

            int count = 0;
            List<Tank> allTanks = GameManager.Instance.GetAllTanks();

            foreach (Tank tank in allTanks)
            {
                if (tank == null)
                    continue;

                if (tank.TeamId == team && tank.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        private bool HasEnemyNearZone(List<DetectionResult> enemies, float radius)
        {
            if (_zone == null || enemies == null)
                return false;

            foreach (DetectionResult enemy in enemies)
            {
                if (enemy == null || enemy.target == null)
                    continue;

                float distanceToZone = Vector2.Distance(
                    enemy.target.transform.position,
                    _zone.transform.position
                );
                if (distanceToZone <= radius)
                    return true;
            }

            return false;
        }

        private void IssueOrder(TankAI tank, EStrategicOrder order)
        {
            if (tank == null)
                return;

            tank.ReceiveOrder(order);
        }

        private void AssignRoles()
        {
            if (_tanks == null || _zone == null || GameManager.Instance == null)
                return;

            ETankTeam enemyTeam = _team == ETankTeam.PLAYER
                ? ETankTeam.ENEMY
                : ETankTeam.PLAYER;

            int ownAlive = GetAliveTankCount(_team);
            int enemyAlive = GetAliveTankCount(enemyTeam);

            int ownScore = GameManager.Instance.GetScore((int)_team);
            int enemyScore = GameManager.Instance.GetScore((int)enemyTeam);

            float remainingTime = GameManager.Instance.GetRemainingTime();

            List<DetectionResult> enemies = AggregateBattlefield();
            bool hasEnemyNearZone = HasEnemyNearZone(enemies, _enemyNearZoneRadius);

            // Scenario 1: almost dead team -> fall back
            if (ownAlive <= 1)
            {
                IssueOrderToAll(EStrategicOrder.FALLBACK);
                return;
            }

            // Scenario 2: zone contested -> captor captures, others attack
            if (_zone.IsContested())
            {
                foreach (TankAI tank in _tanks)
                {
                    if (tank == null) continue;

                    if (tank.Role == ETankRole.CAPTOR)
                        IssueOrder(tank, EStrategicOrder.CAPTUREZONE);
                    else
                        IssueOrder(tank, EStrategicOrder.FULLAGGRESSION);
                }

                return;
            }

            // Scenario 3: zone is neutral and no enemies are near -> captor goes zone, others normal
            if (!_zone.IsCaptured() && !hasEnemyNearZone)
            {
                foreach (TankAI tank in _tanks)
                {
                    if (tank == null) continue;

                    if (tank.Role == ETankRole.CAPTOR)
                        IssueOrder(tank, EStrategicOrder.CAPTUREZONE);
                    else
                        IssueOrder(tank, EStrategicOrder.NONE);
                }

                return;
            }

            // Scenario 4: winning and zone belongs to us -> defend
            if (ownScore >= enemyScore + 3 && _zone.IsCaptured() && _zone.controllingTeam == (int)_team)
            {
                IssueOrderToAll(EStrategicOrder.DEFENDZONE);
                return;
            }

            // Scenario 5: losing with little time left -> full aggression
            if (ownScore < enemyScore && remainingTime <= 30f)
            {
                IssueOrderToAll(EStrategicOrder.FULLAGGRESSION);
                return;
            }

            // Scenario 6: losing, but not urgent yet -> attacker/captor attack, defender holds
            if (ownScore < enemyScore)
            {
                foreach (TankAI tank in _tanks)
                {
                    if (tank == null) continue;

                    if (tank.Role == ETankRole.DEFENDER)
                        IssueOrder(tank, EStrategicOrder.DEFENDZONE);
                    else
                        IssueOrder(tank, EStrategicOrder.FULLAGGRESSION);
                }

                return;
            }

            // Scenario 7: default -> no commander override
            IssueOrderToAll(EStrategicOrder.NONE);
        }

        private void IssueOrderToAll(EStrategicOrder order)
        {
            foreach (TankAI tank in _tanks)
            {
                IssueOrder(tank, order);
            }
        }
    }
}
