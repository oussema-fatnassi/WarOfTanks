using System.Collections.Generic;
using UnityEngine;

public interface ITankComponents
{
    TankController Controller { get; }
    TurretController Turret { get; }
    NavigationStrategy Navigation { get; }
    float FiringRange { get; }
    HashSet<Vector2Int> GetBlockedCells(Vector2 near);
}
