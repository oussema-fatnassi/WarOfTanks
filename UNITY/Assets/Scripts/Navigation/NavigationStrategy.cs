using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationStrategy : MonoBehaviour
{
    public abstract List<Vector2> ComputePath(Vector2 from, Vector2 to);
    public virtual List<Vector2> ComputePath(Vector2 from, Vector2 to, HashSet<Vector2Int> blocked)
        => ComputePath(from, to);
}
