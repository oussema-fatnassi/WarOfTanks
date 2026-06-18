using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.Enums;
using ZoneController = WarOfTanks.Zone.Zone;

namespace WarOfTanks.AI
{
    /// <summary>
    /// Coordinates a team of AI tanks by periodically evaluating the battlefield and issuing strategic orders.
    /// </summary>
    public class CommanderAI : MonoBehaviour
    {
        [SerializeField] private List<TankAI> _tanks;
        [SerializeField] private ZoneController _zone;
        [SerializeField] private float _evaluationInterval = 1f;
        [SerializeField] private float _enemyNearZoneRadius = 5f;
        [SerializeField] private ETankTeam _team;
        private float _evaluationTimer;

        /// <summary>
        /// Advances the evaluation timer and assigns strategic orders when the configured interval elapses.
        /// </summary>
        private void Update()
        {
            _evaluationTimer += Time.deltaTime;
            if (_evaluationTimer >= _evaluationInterval)
            {
                _evaluationTimer = 0f;
                AssignRoles();
            }
        }

        /// <summary>
        /// Combines all visible enemy detections from the controlled tanks into one deduplicated battlefield view.
        /// </summary>
        /// <returns>A list of unique enemy detection results, deduplicated by target tank.</returns>
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

        /// <summary>
        /// Counts all currently alive tanks for the specified team.
        /// </summary>
        /// <param name="team">The team whose living tanks should be counted.</param>
        /// <returns>The number of alive tanks on the given team.</returns>
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

        /// <summary>
        /// Checks whether any detected enemy is within the given radius of the capture zone.
        /// </summary>
        /// <param name="enemies">The aggregated enemy detections to evaluate.</param>
        /// <param name="radius">The maximum distance from the zone for an enemy to count as nearby.</param>
        /// <returns>True if at least one detected enemy is near the zone; otherwise false.</returns>
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

        /// <summary>
        /// Sends a strategic order to a single tank if the tank reference is valid.
        /// </summary>
        /// <param name="tank">The tank that should receive the order.</param>
        /// <param name="order">The strategic order to send.</param>
        private void IssueOrder(TankAI tank, EStrategicOrder order)
        {
            if (tank == null)
                return;

            tank.ReceiveOrder(order);
        }

        /// <summary>
        /// Evaluates current match state and applies the first matching strategic scenario to the controlled tanks.
        /// </summary>
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

        /// <summary>
        /// Sends the same strategic order to every tank controlled by this commander.
        /// </summary>
        /// <param name="order">The order to send to all controlled tanks.</param>
        private void IssueOrderToAll(EStrategicOrder order)
        {
            foreach (TankAI tank in _tanks)
            {
                IssueOrder(tank, order);
            }
        }
    }
}
