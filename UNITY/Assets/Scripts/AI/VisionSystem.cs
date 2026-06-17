using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.Enums;

namespace WarOfTanks.AI
{
    /// <summary>
    /// Detection component — scans for enemy tanks within radius and verifies line of sight via raycasts.
    /// </summary>
    public class VisionSystem : MonoBehaviour, IVisionSystem
    {
        [SerializeField] private float _detectionRadius;
        [SerializeField] private float _fieldOfViewAngle;
        [SerializeField] private LayerMask _obstacleLayerMask;
        [SerializeField] private bool _showDebugLogs;

        /// <summary>
        /// Scans all tanks and returns detection results for enemies of the owner team.
        /// </summary>
        public List<DetectionResult> Scan(List<Tank> allTanks, ETankTeam ownerTeamId)
        {
            List<DetectionResult> results = new List<DetectionResult>();

            if (allTanks == null) return results;

            foreach (Tank tank in allTanks)
            {
                if (tank == null || !tank.IsAlive) continue;
                if (tank.gameObject == gameObject) continue;
                if (tank.TeamId == ownerTeamId) continue;

                float distance = Vector2.Distance(transform.position, tank.transform.position);
                if (distance > _detectionRadius) continue;

                Vector2 direction = ((Vector2)(tank.transform.position - transform.position)).normalized;
                float angle = Vector2.Angle(transform.up, direction);

                bool blocked = Physics2D.Linecast(transform.position, tank.transform.position, _obstacleLayerMask);
                bool isInLineOfSight = !blocked;

                results.Add(new DetectionResult(tank, distance, angle, isInLineOfSight));

                DebugLogger.Log(_showDebugLogs, $"[VisionSystem] {gameObject.name} → {tank.name} | dist: {distance:F1} | LoS: {isInLineOfSight}");
            }

            DebugLogger.Log(_showDebugLogs, $"[VisionSystem] {gameObject.name} scan done — {results.Count} enemies in range");

            return results;
        }


        /// <summary>
        /// Returns the closest target from a list of detection results.
        /// </summary>
        public DetectionResult GetClosestTarget(List<DetectionResult> results)
        {
            if (results == null || results.Count == 0) return null;

            DetectionResult closest = null;

            foreach (DetectionResult result in results)
            {
                if (closest == null || result.distance < closest.distance)
                    closest = result;
            }

            return closest;
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
    }
}
