using UnityEngine;

/// <summary>
/// Draws a circular LineRenderer around a tank to indicate selection state. Subscribes to
/// the parent Tank's OnSelected event and enables or disables the ring accordingly.
/// The circle is built in local space (useWorldSpace = false) using 40 vertices at radius 0.6.
/// </summary>
public class SelectionIndicator : MonoBehaviour
{
    [SerializeField] private bool _showDebugLogs = false;
    private LineRenderer _lineRenderer;
    private Tank _tank;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _tank = GetComponentInParent<Tank>();
        InitializeLineRenderer();
        _lineRenderer.enabled = false;
    }

    private void Start()
    {
        _tank.OnSelected += HandleSelected;
    }

    private void OnDestroy()
    {
        _tank.OnSelected -= HandleSelected;
    }

    private void HandleSelected(bool isSelected)
    {
        DebugLogger.Log(_showDebugLogs, $"SelectionIndicator: Tank '{_tank.name}' selection state changed to {isSelected}");
        _lineRenderer.enabled = isSelected;
    }

    /// <summary>Configures the LineRenderer to draw a closed circle of 40 vertices at radius 0.6 in local space.</summary>
    private void InitializeLineRenderer()
    {
        _lineRenderer.useWorldSpace = false;
        _lineRenderer.positionCount = 40;

        for (int i = 0; i <= 39; i++)
        {
            float angle = (i / 39f) * 2f * Mathf.PI;
            _lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.6f, Mathf.Sin(angle) * 0.6f, 0f));
        }
    }
}
