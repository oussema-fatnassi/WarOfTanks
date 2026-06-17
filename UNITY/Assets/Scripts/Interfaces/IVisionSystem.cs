using System.Collections.Generic;
using WarOfTanks.AI;
using WarOfTanks.Enums;

/// <summary>
/// Contract for a component that detects enemy tanks within a radius and verifies line of sight.
/// </summary>
public interface IVisionSystem
{
    /// <summary>
    /// Scans the provided tank list and returns detection results for enemies of the owner team.
    /// </summary>
    List<DetectionResult> Scan(List<Tank> allTanks, ETankTeam ownerTeamId);

    /// <summary>
    /// Returns the detection result with the smallest distance, or null if the list is empty.
    /// </summary>
    DetectionResult GetClosestTarget(List<DetectionResult> results);
}