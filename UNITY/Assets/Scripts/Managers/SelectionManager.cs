using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : SingletonBehaviour<SelectionManager>
{
    private List<ISelectable> _selectedTanks; 
    private List<ISelectable> _allFriendlyTanks;

    protected override void Awake()
    {
        base.Awake();
        _selectedTanks = new List<ISelectable>();
        _allFriendlyTanks = new List<ISelectable>();
    }

    public static void RegisterFriendlyTank(ISelectable tank)
    {
        Instance._allFriendlyTanks.Add(tank);
    }
}
