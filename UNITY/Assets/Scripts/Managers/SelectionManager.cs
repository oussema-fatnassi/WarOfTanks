using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : SingletonBehaviour<SelectionManager>
{
    #region Fields
    private List<ISelectable> _selectedTanks; 
    private List<ISelectable> _allFriendlyTanks;
    private Camera _mainCamera;
    #endregion

    #region Unity Methods
    protected override void Awake()
    {
        base.Awake();
        _selectedTanks = new List<ISelectable>();
        _allFriendlyTanks = new List<ISelectable>();
        _mainCamera = Camera.main;
    }
    #endregion

    #region Selection Management Methods
    public void RegisterFriendlyTank(ISelectable tank)
    {
        _allFriendlyTanks.Add(tank);
        Tank tankMono = tank as Tank;
        if (tankMono != null) tankMono.OnTankDied += () => RemoveFromSelection(tank);
    }
    public void SelectSingle(ISelectable tank) {
        DeselectAll();
        AddToSelection(tank);
    }
    public void AddToSelection(ISelectable tank) { 
        _selectedTanks.Add(tank);
        tank.SetSelected(true);
    }
    public void RemoveFromSelection(ISelectable tank) 
    {
        _selectedTanks.Remove(tank);
        tank.SetSelected(false);
    }
    public void DeselectAll() 
    {
        foreach (var tank in _selectedTanks)
        {
            tank.SetSelected(false);
        }
        _selectedTanks.Clear();
    }
    public bool IsSelected(ISelectable tank) 
    {
        return _selectedTanks.Contains(tank); 
    }
    public List<ISelectable> GetSelectedTanks() 
    {
        return new List<ISelectable>(_selectedTanks);
    }
    public void SelectMultiple(List<ISelectable> tanks) { 
        DeselectAll();
        foreach (var tank in tanks)
        {
            AddToSelection(tank);
        }
    }
    public void SelectInRect(Rect screenRect) 
    {
        DeselectAll();
        Collider2D[] hits = GetHitsInRect(screenRect);

        foreach (Collider2D hit in hits) 
        {
            if (hit.TryGetComponent<ISelectable>(out ISelectable selectable) && _allFriendlyTanks.Contains(selectable)) 
            {
                AddToSelection(selectable);
            }
        }
    }
    #endregion

    private Collider2D[] GetHitsInRect(Rect screenRect) 
    {
        Vector2 worldMin = _mainCamera.ScreenToWorldPoint(screenRect.min);
        Vector2 worldMax = _mainCamera.ScreenToWorldPoint(screenRect.max);
        return Physics2D.OverlapAreaAll(worldMin, worldMax);
    }
}
