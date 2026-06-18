using System.Collections.Generic;
using UnityEngine;

namespace WarOfTanks.Fog
{
/// <summary>
/// Darkens the map outside friendly vision using a runtime texture overlay.
/// This is WebGL-safe and does not rely on post-processing or compute shaders.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWarOverlay : MonoBehaviour
{
    [SerializeField] private Vector2 _worldCenter;
    [SerializeField] private Vector2 _worldSize = new Vector2(32f, 24f);
    [SerializeField] private bool _autoFitToSceneRenderers = true;
    [SerializeField] private Vector2 _autoFitPadding = new Vector2(1f, 1f);
    [SerializeField] private int _textureWidth = 128;
    [SerializeField] private int _textureHeight = 128;
    [SerializeField] private float _visionRadius = 4f;
    [SerializeField] private float _softEdgeWidth = 1f;
    [SerializeField] private bool _rememberExploredArea = true;
    [SerializeField, Range(0f, 1f)] private float _unexploredAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _exploredAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float _targetVisibilityThreshold = 0.25f;
    [SerializeField] private LayerMask _lineOfSightObstacleMask;
    [SerializeField] private string _sortingLayerName = "Units";
    [SerializeField] private int _sortingOrder = -100;

    private SpriteRenderer _spriteRenderer;
    private Texture2D _fogTexture;
    private Color32[] _pixels;
    private bool[] _exploredPixels;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_autoFitToSceneRenderers)
        {
            FitToSceneRenderers();
        }

        InitializeTexture();
    }

    private void OnDestroy()
    {
        if (_fogTexture != null)
        {
            Destroy(_fogTexture);
        }
    }

    /// <summary>
    /// Rebuilds the overlay alpha based on the current friendly tanks.
    /// </summary>
    public void UpdateVisibility(List<Tank> friendlyTanks)
    {
        if (_fogTexture == null || _pixels == null)
            return;

        float halfWidth = _worldSize.x * 0.5f;
        float halfHeight = _worldSize.y * 0.5f;
        float minX = _worldCenter.x - halfWidth;
        float minY = _worldCenter.y - halfHeight;
        float safeSoftEdge = Mathf.Max(0.01f, _softEdgeWidth);

        for (int y = 0; y < _textureHeight; y++)
        {
            float worldY = minY + ((y + 0.5f) / _textureHeight) * _worldSize.y;
            for (int x = 0; x < _textureWidth; x++)
            {
                int index = y * _textureWidth + x;
                float worldX = minX + ((x + 0.5f) / _textureWidth) * _worldSize.x;
                Vector2 worldPoint = new Vector2(worldX, worldY);
                float visibility = GetVisibilityAt(worldPoint, friendlyTanks, safeSoftEdge);

                if (visibility > 0f)
                {
                    _exploredPixels[index] = true;
                }

                float baseAlpha = _rememberExploredArea && _exploredPixels[index]
                    ? _exploredAlpha
                    : _unexploredAlpha;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Lerp(baseAlpha, 0f, visibility) * 255f);
                _pixels[index] = new Color32(0, 0, 0, alpha);
            }
        }

        _fogTexture.SetPixels32(_pixels);
        _fogTexture.Apply(false);
    }

    /// <summary>
    /// Returns whether gameplay should treat this position as visible to the local player.
    /// </summary>
    public bool IsWorldPointVisible(Vector2 worldPoint, List<Tank> friendlyTanks)
    {
        float safeSoftEdge = Mathf.Max(0.01f, _softEdgeWidth);
        return GetVisibilityAt(worldPoint, friendlyTanks, safeSoftEdge) >= _targetVisibilityThreshold;
    }

    private float GetVisibilityAt(Vector2 worldPoint, List<Tank> friendlyTanks, float safeSoftEdge)
    {
        if (friendlyTanks == null)
            return 0f;

        float bestVisibility = 0f;
        foreach (Tank tank in friendlyTanks)
        {
            if (tank == null || !tank.IsAlive)
                continue;

            Vector2 tankPosition = tank.transform.position;
            float distance = Vector2.Distance(tankPosition, worldPoint);
            if (distance > _visionRadius)
                continue;

            if (IsLineOfSightBlocked(tankPosition, worldPoint))
                continue;

            float fullyVisibleRadius = Mathf.Max(0f, _visionRadius - safeSoftEdge);
            float visibility = distance <= fullyVisibleRadius
                ? 1f
                : 1f - Mathf.InverseLerp(fullyVisibleRadius, _visionRadius, distance);
            if (visibility > bestVisibility)
            {
                bestVisibility = visibility;
            }
        }

        return bestVisibility;
    }

    private bool IsLineOfSightBlocked(Vector2 origin, Vector2 target)
    {
        if (_lineOfSightObstacleMask.value == 0)
            return false;

        return Physics2D.Linecast(origin, target, _lineOfSightObstacleMask);
    }

    private void InitializeTexture()
    {
        _textureWidth = Mathf.Max(16, _textureWidth);
        _textureHeight = Mathf.Max(16, _textureHeight);
        _worldSize.x = Mathf.Max(1f, _worldSize.x);
        _worldSize.y = Mathf.Max(1f, _worldSize.y);

        _fogTexture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGBA32, false);
        _fogTexture.wrapMode = TextureWrapMode.Clamp;
        _fogTexture.filterMode = FilterMode.Bilinear;
        _pixels = new Color32[_textureWidth * _textureHeight];
        _exploredPixels = new bool[_pixels.Length];

        Sprite sprite = Sprite.Create(
            _fogTexture,
            new Rect(0f, 0f, _textureWidth, _textureHeight),
            new Vector2(0.5f, 0.5f),
            _textureWidth / _worldSize.x
        );

        _spriteRenderer.sprite = sprite;
        _spriteRenderer.sortingLayerName = _sortingLayerName;
        _spriteRenderer.sortingOrder = _sortingOrder;
        transform.position = new Vector3(_worldCenter.x, _worldCenter.y, transform.position.z);
    }

    private void FitToSceneRenderers()
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        bool hasBounds = false;
        Bounds sceneBounds = new Bounds();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer == _spriteRenderer)
                continue;

            if (renderer.GetComponentInParent<Tank>() != null)
                continue;

            if (renderer.GetComponentInParent<Canvas>() != null)
                continue;

            if (!hasBounds)
            {
                sceneBounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                sceneBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            return;

        _worldCenter = sceneBounds.center;
        _worldSize = new Vector2(
            Mathf.Max(1f, sceneBounds.size.x + _autoFitPadding.x * 2f),
            Mathf.Max(1f, sceneBounds.size.y + _autoFitPadding.y * 2f)
        );
    }
}
}
