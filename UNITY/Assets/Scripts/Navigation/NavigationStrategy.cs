using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationStrategy : MonoBehaviour
{
    public abstract List<Vector2> ComputePath(Vector2 from, Vector2 to);
}
