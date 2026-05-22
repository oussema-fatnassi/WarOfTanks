using System.Collections.Generic;

using WarOfTanks.Enums;
using ZoneController = WarOfTanks.Zone.Zone;

namespace WarOfTanks.AI
{
    /// <summary>
    /// Stores the current AI tank context read by behaviour tree condition and action nodes.
    /// </summary>
    public class TankBlackboard
    {
        public Tank self;
        public ETankTeam teamId;
        public ETankTeam enemyTeamId;
        public List<DetectionResult> enemyResults = new List<DetectionResult>();
        public DetectionResult closestEnemy;
        public ZoneController zone;
        public float hpRatio;

        /// <summary>
        /// Refreshes the blackboard values before the behaviour tree is ticked.
        /// </summary>
        public void Update(VisionSystem vision, List<Tank> allTanks)
        {
            if (self == null)
            {
                ClearDetectionData();
                hpRatio = 0f;
                return;
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

        private float GetHealthRatio()
        {
            HealthSystem healthSystem = self.GetComponent<HealthSystem>();
            return healthSystem != null ? healthSystem.HealthPercentage : 0f;
        }

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

        private void ClearDetectionData()
        {
            enemyResults = new List<DetectionResult>();
            closestEnemy = null;
        }

        private ETankTeam GetEnemyTeam(ETankTeam teamId)
        {
            return teamId == ETankTeam.PLAYER ? ETankTeam.ENEMY : ETankTeam.PLAYER;
        }
    }
}
