using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    [Header("Debug")]
    [SerializeField] private bool _enableLogs = false;

    protected override void Awake()
    {
        base.Awake();
        ApplyDebugSettings();
    }

    private void OnValidate()
    {
        ApplyDebugSettings();
    }

    private void ApplyDebugSettings()
    {
        DebugLogger.IsEnabled = _enableLogs;
    }
}
