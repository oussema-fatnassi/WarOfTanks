using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.Enums;

namespace WarOfTanks.AI
{
    /// <summary>
    /// Stub detection component used by tank behaviour trees until the real vision logic is implemented.
    /// </summary>
    public class VisionSystem : MonoBehaviour 
    {
        [SerializeField] private float _detectionRadius;
        [SerializeField] private float _fieldOfViewAngle;

        /// <summary>
        /// Scans all tanks and returns detection results for enemies of the owner team.
        /// </summary>
        public List<DetectionResult> Scan(List<Tank> allTanks, ETankTeam ownerTeamId)
        {
            // TODO: implement real detection in issue #19.
            return new List<DetectionResult>();
        }


        /// <summary>
        /// Returns the closest target from a list of detection results.
        /// </summary>
        public DetectionResult GetClosestTarget(List<DetectionResult> results)
        {
            // TODO: implement real target selection in issue #19.
            return null;
        }
    }
}
