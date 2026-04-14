using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Speed & Threshold")]
    [SerializeField] private float _panSpeed = 10f;
    [SerializeField] private int _edgeThreshold = 20; // Distance in pixels from screen edge to trigger panning

    [Header("Map Bounds")]
    [SerializeField] private Bounds _mapBounds = new Bounds(Vector3.zero, new Vector3(50, 50, 0));

    [Header("Movement Smoothing")]
    [SerializeField] private float _smoothing = 0.08f; // Lower = smoother, higher = snappier

    private Vector3 _targetPosition;

    private void Start()
    {
        _targetPosition = transform.position;
    }

    private void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 direction = Vector3.zero;

        // Check horizontal edges
        if (mousePos.x <= _edgeThreshold)
            direction.x = -1f;
        else if (mousePos.x >= Screen.width - _edgeThreshold)
            direction.x = 1f;

        // Check vertical edges
        if (mousePos.y <= _edgeThreshold)
            direction.y = -1f;
        else if (mousePos.y >= Screen.height - _edgeThreshold)
            direction.y = 1f;

        _targetPosition += direction * (_panSpeed * Time.deltaTime);

        // Keep camera within map boundaries
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, _mapBounds.min.x, _mapBounds.max.x);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, _mapBounds.min.y, _mapBounds.max.y);

        // Framerate-independent exponential smoothing
        float t = 1f - Mathf.Pow(_smoothing, Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, _targetPosition, t);
    }

#if UNITY_EDITOR
    // Visualize map bounds in the Scene view when this object is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
        Gizmos.DrawCube(_mapBounds.center, _mapBounds.size);
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        Gizmos.DrawWireCube(_mapBounds.center, _mapBounds.size);
    }
#endif
}