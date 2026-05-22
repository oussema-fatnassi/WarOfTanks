using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    [Header("Debug")]
    [SerializeField] private bool _enableLogs = false;
    [SerializeField] private List<Tank> _allTanks = new List<Tank>();

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

    public void RegisterTank(Tank tank)
    {
        if(tank == null) return;
        if(_allTanks.Contains(tank)) return;

        _allTanks.Add(tank);
    }

    public void UnregisterTank(Tank tank)
    {
        if(tank == null) return;
        _allTanks.Remove(tank);
    }

    public List<Tank> GetAllTanks()
    {
        return new List<Tank>(_allTanks);
    }
}
