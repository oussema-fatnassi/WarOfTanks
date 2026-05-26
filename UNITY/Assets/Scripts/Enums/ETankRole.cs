namespace WarOfTanks.Enums
{
    /// <summary>
    /// Defines the tactical behaviour tree assigned to an AI tank.
    /// </summary>
    public enum ETankRole
    {
        /// <summary>Role focused on pursuing enemies and attacking.</summary>
        ATTACKER,

        /// <summary>Role focused on protecting the capture zone.</summary>
        DEFENDER,

        /// <summary>Role focused on capturing and holding the zone.</summary>
        CAPTOR
    }
}
