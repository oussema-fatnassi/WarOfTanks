namespace WarOfTanks.AI
{
    /// <summary>Stores the result of detecting one target tank.</summary>
    public class DetectionResult
    {
        /// <summary>The detected tank.</summary>
        public Tank target;

        /// <summary>Distance from the scanner to the detected tank.</summary>
        public float distance;

        /// <summary>Angle between the scanner forward direction and the detected tank.</summary>
        public float angle;

        /// <summary>Whether the detected tank is visible without an obstacle blocking line of sight.</summary>
        public bool isInLineOfSight;

        /// <summary>
        /// Creates a detection result for one visible or scanned tank.
        /// </summary>
        public DetectionResult(Tank target, float distance, float angle, bool isInLineOfSight)
        {
            this.target = target;
            this.distance = distance;
            this.angle = angle;
            this.isInLineOfSight = isInLineOfSight;
        }
    }
}
