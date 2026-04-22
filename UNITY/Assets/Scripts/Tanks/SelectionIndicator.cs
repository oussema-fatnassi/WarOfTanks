using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private Tank _tank;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _tank = GetComponentInParent<Tank>();
        
        InitializeLineRenderer();
        _lineRenderer.enabled = false;
    }
    void Start()
    {
        _tank.OnSelected += HandleSelected;
    }
    private void OnDestroy()
    {
        _tank.OnSelected -= HandleSelected;
    }
    private void HandleSelected(bool isSelected)
    {
        _lineRenderer.enabled = isSelected;
    }
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
