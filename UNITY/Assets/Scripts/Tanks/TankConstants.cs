/// <summary>
/// Shared numeric constants used across all tank systems to avoid magic numbers.
/// Centralising them here means tuning one value affects every command and controller uniformly.
/// </summary>
public static class TankConstants
{
    /// <summary>
    /// World-unit radius within which a tank considers itself to have reached a waypoint.
    /// Used by MoveCommand, AttackCommand, and AttackZoneCommand to advance the waypoint index.
    /// </summary>
    public const float WAYPOINT_ARRIVAL_THRESHOLD = 0.1f;

    /// <summary>
    /// Minimum displacement the tank must cover in <see cref="STALL_CHECK_INTERVAL"/> seconds
    /// before MoveCommand's stall detector fires a path recompute. Intentionally larger than
    /// <see cref="WAYPOINT_ARRIVAL_THRESHOLD"/>: two Dynamic Rigidbody2D tanks pressing against
    /// each other via MovePosition can produce 0.1–0.3 f of net oscillation per interval without
    /// making real progress, which would fool the smaller threshold.
    /// </summary>
    public const float STALL_DISTANCE_THRESHOLD = 0.5f;

    /// <summary>
    /// Maximum angular error in degrees between the turret's current heading and the target
    /// direction before TurretController.IsAimedAt returns true and Fire() is allowed.
    /// </summary>
    public const float TURRET_TOLERANCE_ANGLE = 5f;

    /// <summary>
    /// How often MoveCommand samples the tank's position to check for a stall, in seconds.
    /// Shorter intervals increase responsiveness at the cost of more frequent A* calls.
    /// </summary>
    public const float STALL_CHECK_INTERVAL = 0.5f;

    /// <summary>
    /// Distance an attack target must move from its last recorded position before AttackCommand
    /// considers the cached path stale and recomputes. Avoids running A* every frame for targets
    /// that drift slightly in place.
    /// </summary>
    public const float STALE_THRESHOLD = 0.5f;
}
