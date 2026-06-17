using System.Collections.Generic;
using UnityEditor;
using WarOfTanks.Enums;
using ZoneController = WarOfTanks.Zone.Zone;

namespace WarOfTanks.AI
{
    /// <summary>
    /// Stores the current AI tank context read by behaviour tree condition and action nodes.
    /// </summary>
    public class TankBlackboard
    {
        /// <summary>Tank controlled by this AI.</summary>
        public Tank self;

        /// <summary>Team of the controlled tank.</summary>
        public ETankTeam teamId;

        /// <summary>Opposing team of the controlled tank.</summary>
        public ETankTeam enemyTeamId;

        /// <summary>Current visible enemy detection results.</summary>
        public List<DetectionResult> enemyResults = new List<DetectionResult>();

        /// <summary>Closest currently visible enemy.</summary>
        public DetectionResult closestEnemy;

        /// <summary>Capture zone observed by the AI.</summary>
        public ZoneController zone;

        /// <summary>Current health ratio of the controlled tank.</summary>
        public float hpRatio;

        private HealthSystem _healthSystem;

        /// <summary>
        /// Refreshes the blackboard values before the behaviour tree is ticked.
        /// </summary>
        public void Update(IVisionSystem vision, List<Tank> allTanks)
        {
            if (self == null)
            {
                ClearDetectionData();
                hpRatio = 0f;
                return;
            }

            if (_healthSystem == null)
            {
                _healthSystem = self.GetComponent<HealthSystem>();
            }

            teamId = self.TeamId;
            enemyTeamId = GetEnemyTeam(teamId);
            hpRatio = GetHealthRatio();

            if (vision == null || allTanks == null)
            {
                ClearDetectionData();
                return;
            }

            enemyResults = vision.Scan(allTanks, teamId);
            if (enemyResults == null)
            {
                enemyResults = new List<DetectionResult>();
            }

            enemyResults = FilterValidEnemies(enemyResults);
            closestEnemy = vision.GetClosestTarget(enemyResults);
        }

        /// <summary>
        /// Returns the cached health percentage for the controlled tank.
        /// </summary>
        private float GetHealthRatio()
        {
            return _healthSystem != null ? _healthSystem.HealthPercentage : 0f;
        }

        /// <summary>
        /// Removes invalid, dead, or friendly detection results.
        /// </summary>
        private List<DetectionResult> FilterValidEnemies(List<DetectionResult> results)
        {
            List<DetectionResult> validEnemies = new List<DetectionResult>();

            foreach (DetectionResult result in results)
            {
                if (result?.target == null) continue;
                if (!result.target.IsAlive) continue;
                if (result.target.TeamId != enemyTeamId) continue;

                validEnemies.Add(result);
            }

            return validEnemies;
        }

        /// <summary>
        /// Clears target data when vision input is unavailable.
        /// </summary>
        private void ClearDetectionData()
        {
            enemyResults = new List<DetectionResult>();
            closestEnemy = null;
        }

        /// <summary>
        /// Returns the opposing team for the given team id.
        /// </summary>
        private ETankTeam GetEnemyTeam(ETankTeam teamId)
        {
            return teamId == ETankTeam.PLAYER ? ETankTeam.ENEMY : ETankTeam.PLAYER;
        }
    }
}
