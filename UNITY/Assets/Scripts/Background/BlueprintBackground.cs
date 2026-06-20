using UnityEngine;

/// <summary>
/// Procedurally renders the blueprint-style grid background entirely in code
/// (no image asset). Builds a single quad sized to the map and drives the
/// WarOfTanks/BlueprintGrid shader. Runs in the editor too so you can tweak it live.
///
/// Usage: create an empty GameObject (e.g. "BlueprintBackground"), add this
/// component, set Size to your map size, then tune the colours/grid in the Inspector.
/// It replaces the old solid-colour Background tilemap — you can disable that.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BlueprintBackground : MonoBehaviour
{
    [Header("Coverage")]
    [Tooltip("World-space size of the background quad. Match (or slightly exceed) your map bounds.")]
    [SerializeField] private Vector2 _size = new Vector2(64f, 64f);
    [Tooltip("Z position. Positive = further from a 2D camera so it stays behind everything.")]
    [SerializeField] private float _depth = 10f;

    [Header("Sorting (2D)")]
    [SerializeField] private string _sortingLayer = "Background";
    [SerializeField] private int _sortingOrder = -1000;

    [Header("Material")]
    [Tooltip("Assign the BlueprintGrid.mat asset to edit it as a project asset. " +
             "Leave empty to create a runtime-only material from the shader. " +
             "When assigned, the Color/Grid/Effect fields below are ignored (edit the material asset instead).")]
    [SerializeField] private Material _materialAsset;

    [Header("Colors")]
    [SerializeField] private Color _backgroundColor = new Color(0.043f, 0.082f, 0.133f, 1f);
    [SerializeField] private Color _backgroundEdgeColor = new Color(0.016f, 0.035f, 0.063f, 1f);
    [SerializeField] private Color _lineColor = new Color(0.45f, 0.62f, 0.78f, 0.30f);
    [SerializeField] private Color _crossColor = new Color(0.70f, 0.85f, 1f, 0.65f);

    [Header("Grid (world units)")]
    [SerializeField] private float _cellSize = 4f;
    [SerializeField] private float _lineThickness = 0.035f;
    [SerializeField] private float _crossArmLength = 0.18f;
    [SerializeField] private float _crossThickness = 0.045f;

    [Header("Effects")]
    [Range(0f, 1f)][SerializeField] private float _scanlineStrength = 0.10f;
    [SerializeField] private float _scanlineDensity = 240f;
    [Range(0f, 1f)][SerializeField] private float _vignette = 0.6f;

    private MeshRenderer _renderer;
    private MeshFilter _filter;
    private Material _material;

    private void OnEnable()  => Rebuild();
    private void OnValidate() => Rebuild();

    private void Rebuild()
    {
        _renderer = GetComponent<MeshRenderer>();
        _filter = GetComponent<MeshFilter>();

        EnsureMesh();
        EnsureMaterial();
        ApplyProperties();

        // Keep the quad behind everything in a 2D scene.
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, _depth);
        _renderer.sortingLayerName = _sortingLayer;
        _renderer.sortingOrder = _sortingOrder;
        _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _renderer.receiveShadows = false;
    }

    private void EnsureMesh()
    {
        // A unit quad centered on origin; scaled by _size below.
        float hx = _size.x * 0.5f;
        float hy = _size.y * 0.5f;

        var mesh = _filter.sharedMesh;
        if (mesh == null || mesh.name != "BlueprintQuad")
        {
            mesh = new Mesh { name = "BlueprintQuad" };
            _filter.sharedMesh = mesh;
        }

        mesh.vertices = new[]
        {
            new Vector3(-hx, -hy, 0f),
            new Vector3( hx, -hy, 0f),
            new Vector3(-hx,  hy, 0f),
            new Vector3( hx,  hy, 0f),
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 1f), new Vector2(1f, 1f),
        };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        mesh.RecalculateBounds();
    }

    private void EnsureMaterial()
    {
        // Prefer the assigned project asset so the material is editable/shared.
        if (_materialAsset != null)
        {
            _material = _materialAsset;
            _renderer.sharedMaterial = _material;
            return;
        }

        if (_material == null || _material.hideFlags != HideFlags.DontSave)
        {
            Shader shader = Shader.Find("WarOfTanks/BlueprintGrid");
            if (shader == null)
            {
                Debug.LogError("[BlueprintBackground] Shader 'WarOfTanks/BlueprintGrid' not found.");
                return;
            }
            _material = new Material(shader) { name = "BlueprintGrid (runtime)" };
            _material.hideFlags = HideFlags.DontSave;
        }
        _renderer.sharedMaterial = _material;
    }

    private void ApplyProperties()
    {
        // When using a shared project asset, respect the values authored on the asset.
        if (_material == null || _materialAsset != null) return;
        _material.SetColor("_BgColor", _backgroundColor);
        _material.SetColor("_BgColorEdge", _backgroundEdgeColor);
        _material.SetColor("_LineColor", _lineColor);
        _material.SetColor("_CrossColor", _crossColor);
        _material.SetFloat("_GridSize", _cellSize);
        _material.SetFloat("_LineThickness", _lineThickness);
        _material.SetFloat("_CrossSize", _crossArmLength);
        _material.SetFloat("_CrossThickness", _crossThickness);
        _material.SetFloat("_ScanlineStrength", _scanlineStrength);
        _material.SetFloat("_ScanlineDensity", _scanlineDensity);
        _material.SetFloat("_Vignette", _vignette);
    }
}
