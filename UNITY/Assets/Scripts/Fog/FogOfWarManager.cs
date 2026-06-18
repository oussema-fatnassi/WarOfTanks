using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.AI;
using WarOfTanks.Enums;

namespace WarOfTanks.Fog
{
/// <summary>
/// Coordinates player-facing fog visibility from detection results and blocks
/// player targeting against enemies that remain hidden by fog.
/// </summary>
public class FogOfWarManager : MonoBehaviour
{
    private const string AUTO_MANAGER_NAME = "Fog Of War Manager";

    public static FogOfWarManager Instance { get; private set; }

    [SerializeField] private ETankTeam _friendlyTeam = ETankTeam.PLAYER;
    [SerializeField] private float _refreshInterval = 0.1f;
    [SerializeField] private float _hideGracePeriod = 0.4f;
    [SerializeField] private bool _autoAddFogVisibility = true;
    [SerializeField] private bool _autoCreateMapOverlay = true;
    [SerializeField] private FogOfWarOverlay _mapOverlay;
    [SerializeField] private bool _showDebugLogs;

    private readonly HashSet<Tank> _visibleEnemies = new HashSet<Tank>();
    private readonly Dictionary<Tank, float> _lastSeenTimeByEnemy = new Dictionary<Tank, float>();
    private readonly Dictionary<Tank, FogVisibility> _visibilityByTank = new Dictionary<Tank, FogVisibility>();
    private readonly List<Tank> _fallbackTanks = new List<Tank>();
    private readonly List<Tank> _friendlyTanks = new List<Tank>();
    private float _refreshTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureSceneManagerExists()
    {
        if (FindObjectOfType<FogOfWarManager>() != null)
            return;

        new GameObject(AUTO_MANAGER_NAME).AddComponent<FogOfWarManager>();
    }

    private void Start()
    {
        FindOrCreateMapOverlay();
        RefreshVisibility(true);
    }

    private void Update()
    {
        _refreshTimer += Time.deltaTime;
        if (_refreshTimer < _refreshInterval)
            return;

        _refreshTimer = 0f;
        RefreshVisibility(false);
    }

    private void RefreshVisibility(bool immediate)
    {
        List<Tank> allTanks = GetAllTanks();
        _visibleEnemies.Clear();
        _friendlyTanks.Clear();

        foreach (Tank tank in allTanks)
        {
            if (!CanScanFrom(tank))
                continue;

            _friendlyTanks.Add(tank);
            CollectVisibleEnemiesFrom(tank, allTanks);
        }

        ApplyVisibility(allTanks, immediate);
        if (_mapOverlay != null)
        {
            _mapOverlay.UpdateVisibility(_friendlyTanks);
        }
        DebugLogger.Log(_showDebugLogs, $"[FogOfWar] Visible enemies: {_visibleEnemies.Count}");
    }

    private void FindOrCreateMapOverlay()
    {
        if (_mapOverlay != null)
            return;

        _mapOverlay = FindObjectOfType<FogOfWarOverlay>();
        if (_mapOverlay != null || !_autoCreateMapOverlay)
            return;

        GameObject overlayObject = new GameObject("Fog Of War Overlay");
        _mapOverlay = overlayObject.AddComponent<FogOfWarOverlay>();
    }

    private List<Tank> GetAllTanks()
    {
        if (GameManager.Instance != null)
        {
            List<Tank> registeredTanks = GameManager.Instance.GetAllTanks();
            if (registeredTanks.Count > 0)
                return registeredTanks;
        }

        _fallbackTanks.Clear();
        _fallbackTanks.AddRange(FindObjectsOfType<Tank>());
        return _fallbackTanks;
    }

    private bool CanScanFrom(Tank tank)
    {
        return tank != null
            && tank.IsAlive
            && tank.TeamId == _friendlyTeam;
    }

    private void CollectVisibleEnemiesFrom(Tank tank, List<Tank> allTanks)
    {
        TankAI tankAI = tank.GetComponent<TankAI>();
        if (tankAI != null && tankAI.isActiveAndEnabled)
        {
            AddVisibleResults(tankAI.EnemyResults);
            return;
        }

        VisionSystem visionSystem = tank.GetComponent<VisionSystem>();
        if (visionSystem == null)
            return;

        AddVisibleResults(visionSystem.Scan(allTanks, tank.TeamId));
    }

    private void AddVisibleResults(List<DetectionResult> results)
    {
        if (results == null)
            return;

        foreach (DetectionResult result in results)
        {
            if (result == null || result.target == null) continue;
            if (!result.target.IsAlive) continue;
            if (!result.isInLineOfSight) continue;

            _visibleEnemies.Add(result.target);
            _lastSeenTimeByEnemy[result.target] = Time.time;
        }
    }

    private void ApplyVisibility(List<Tank> allTanks, bool immediate)
    {
        foreach (Tank tank in allTanks)
        {
            if (tank == null)
                continue;

            FogVisibility fogVisibility = GetFogVisibility(tank);
            if (fogVisibility == null)
                continue;

            bool visible = ShouldBeVisible(tank);
            if (immediate)
            {
                fogVisibility.SetVisibleImmediate(visible);
            }
            else
            {
                fogVisibility.SetVisible(visible);
            }
        }
    }

    private FogVisibility GetFogVisibility(Tank tank)
    {
        if (_visibilityByTank.TryGetValue(tank, out FogVisibility cachedVisibility) && cachedVisibility != null)
            return cachedVisibility;

        FogVisibility fogVisibility = tank.GetComponent<FogVisibility>();
        if (fogVisibility == null && _autoAddFogVisibility)
        {
            fogVisibility = tank.gameObject.AddComponent<FogVisibility>();
        }

        if (fogVisibility != null)
        {
            _visibilityByTank[tank] = fogVisibility;
        }

        return fogVisibility;
    }

    private bool ShouldBeVisible(Tank tank)
    {
        if (tank.TeamId == _friendlyTeam)
            return true;

        if (!tank.IsAlive)
            return false;

        if (!IsRevealedByOverlay(tank))
            return false;

        if (_visibleEnemies.Contains(tank))
            return true;

        return _lastSeenTimeByEnemy.TryGetValue(tank, out float lastSeenTime)
            && Time.time - lastSeenTime <= _hideGracePeriod;
    }

    /// <summary>
    /// Returns whether the local friendly team is allowed to target this tank right now.
    /// This is stricter than visual fade grace: enemies in fog cannot be attacked.
    /// </summary>
    public bool CanFriendlyTarget(Tank target)
    {
        if (target == null || !target.IsAlive)
            return false;

        if (target.TeamId == _friendlyTeam)
            return true;

        return _visibleEnemies.Contains(target) && IsRevealedByOverlay(target);
    }

    /// <summary>
    /// Allows gameplay code to respect fog without hard-requiring a fog manager in every scene.
    /// </summary>
    public static bool CanTarget(Tank target)
    {
        return Instance == null || Instance.CanFriendlyTarget(target);
    }

    private bool IsRevealedByOverlay(Tank tank)
    {
        return _mapOverlay == null || _mapOverlay.IsWorldPointVisible(tank.transform.position, _friendlyTanks);
    }
}
}
