using UnityEngine;
using WarOfTanks.Navigation;

public class TestFlowFieldGizmo : MonoBehaviour
{
    [SerializeField] private NavigationGrid _grid;
    [SerializeField] private Vector2Int _target = new Vector2Int(10, 10);

    private void Start()
    {
        if (_grid == null)
        {
            Debug.LogError("TestFlowFieldGizmo: NavigationGrid not assigned.");
            return;
        }

        var ff = new FlowFieldPathfinder(_grid);
        ff.ComputeFlowField(_target);
        _grid.RegisterFlowFieldForDebug(ff);
    }
}
