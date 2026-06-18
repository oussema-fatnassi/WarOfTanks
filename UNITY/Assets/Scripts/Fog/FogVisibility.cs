using UnityEngine;
using UnityEngine.UI;

namespace WarOfTanks.Fog
{
/// <summary>
/// Fades a tank's visible presentation without changing gameplay state.
/// Colliders, rigidbodies, commands, and AI remain active while visuals fade.
/// </summary>
[DisallowMultipleComponent]
public class FogVisibility : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.25f;
    [SerializeField] private bool _visibleOnAwake = true;
    [SerializeField] private bool _includeInactiveChildren = true;
    [SerializeField] private bool _addCanvasGroupsToChildCanvases = true;

    private SpriteRenderer[] _spriteRenderers;
    private float[] _spriteBaseAlpha;
    private CanvasGroup[] _canvasGroups;
    private float[] _canvasGroupBaseAlpha;
    private Graphic[] _graphics;
    private float[] _graphicBaseAlpha;
    private float _currentVisibility;
    private float _targetVisibility;

    private void Awake()
    {
        CacheVisuals();
        _currentVisibility = _visibleOnAwake ? 1f : 0f;
        _targetVisibility = _currentVisibility;
        ApplyVisibility(_currentVisibility);
    }

    private void Update()
    {
        if (Mathf.Approximately(_currentVisibility, _targetVisibility))
            return;

        if (_fadeDuration <= 0f)
        {
            _currentVisibility = _targetVisibility;
        }
        else
        {
            float step = Time.deltaTime / _fadeDuration;
            _currentVisibility = Mathf.MoveTowards(_currentVisibility, _targetVisibility, step);
        }

        ApplyVisibility(_currentVisibility);
    }

    /// <summary>
    /// Sets whether the tank visuals should fade toward visible or hidden.
    /// </summary>
    public void SetVisible(bool visible)
    {
        _targetVisibility = visible ? 1f : 0f;
    }

    /// <summary>
    /// Sets the tank visuals immediately, bypassing the fade.
    /// </summary>
    public void SetVisibleImmediate(bool visible)
    {
        _targetVisibility = visible ? 1f : 0f;
        _currentVisibility = _targetVisibility;
        ApplyVisibility(_currentVisibility);
    }

    private void CacheVisuals()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(_includeInactiveChildren);
        _spriteBaseAlpha = new float[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _spriteBaseAlpha[i] = _spriteRenderers[i].color.a;
        }

        if (_addCanvasGroupsToChildCanvases)
        {
            AddMissingCanvasGroups();
        }

        _canvasGroups = GetComponentsInChildren<CanvasGroup>(_includeInactiveChildren);
        _canvasGroupBaseAlpha = new float[_canvasGroups.Length];
        for (int i = 0; i < _canvasGroups.Length; i++)
        {
            _canvasGroupBaseAlpha[i] = _canvasGroups[i].alpha;
        }

        if (_canvasGroups.Length > 0)
        {
            _graphics = new Graphic[0];
            _graphicBaseAlpha = new float[0];
            return;
        }

        _graphics = GetComponentsInChildren<Graphic>(_includeInactiveChildren);
        _graphicBaseAlpha = new float[_graphics.Length];
        for (int i = 0; i < _graphics.Length; i++)
        {
            _graphicBaseAlpha[i] = _graphics[i].color.a;
        }
    }

    private void AddMissingCanvasGroups()
    {
        Canvas[] canvases = GetComponentsInChildren<Canvas>(_includeInactiveChildren);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.GetComponent<CanvasGroup>() == null)
            {
                canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void ApplyVisibility(float visibility)
    {
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] == null) continue;

            Color color = _spriteRenderers[i].color;
            color.a = _spriteBaseAlpha[i] * visibility;
            _spriteRenderers[i].color = color;
        }

        for (int i = 0; i < _canvasGroups.Length; i++)
        {
            if (_canvasGroups[i] == null) continue;

            _canvasGroups[i].alpha = _canvasGroupBaseAlpha[i] * visibility;
        }

        for (int i = 0; i < _graphics.Length; i++)
        {
            if (_graphics[i] == null) continue;

            Color color = _graphics[i].color;
            color.a = _graphicBaseAlpha[i] * visibility;
            _graphics[i].color = color;
        }
    }
}
}
