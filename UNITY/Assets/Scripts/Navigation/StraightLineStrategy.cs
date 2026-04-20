using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightLineStrategy : NavigationStrategy
{
    public override List<Vector2> ComputePath(Vector2 from, Vector2 to)
    {
        return new List<Vector2> { to };
    }
}
