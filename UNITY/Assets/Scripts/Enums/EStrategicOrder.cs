namespace WarOfTanks.Enums
{
    /// <summary>
    /// Represents high-level commander orders that can temporarily override a tank's role behaviour.
    /// </summary>
    public enum EStrategicOrder
    {
        /// <summary>No commander override is active.</summary>
        NONE,

        /// <summary>Move toward the capture zone and try to capture or hold it.</summary>
        CAPTUREZONE,

        /// <summary>Move to a defensive position near the capture zone.</summary>
        DEFENDZONE,

        /// <summary>Prioritize finding and attacking enemy tanks.</summary>
        FULLAGGRESSION,

        /// <summary>Retreat to spawn to recover health.</summary>
        FALLBACK
    }
}
