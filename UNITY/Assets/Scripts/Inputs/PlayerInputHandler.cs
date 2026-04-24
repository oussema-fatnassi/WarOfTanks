using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    #region Fields
    [SerializeField] private SelectionBox _selectionBox;
    [SerializeField] private LayerMask _tankLayer;

    private PlayerInputActions _actions;
    private CommandDispatcher _commandDispatcher;
    private Camera _mainCamera;
    private bool _isDragging;
    private Vector2 _pendingClickPosition;
    private bool _multiSelectHandled;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _mainCamera = Camera.main;
        _commandDispatcher = new CommandDispatcher();
        _actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _actions.Player.Enable();
        
        _actions.Player.Select.performed += OnSelectPressed;
        _actions.Player.Select.canceled += OnSelectReleased;

        _actions.Player.BoxSelect.performed += OnBoxSelectStarted;
        
        _actions.Player.MultiSelect.performed += OnMultiSelect;

        _actions.Player.ContextAction.performed += OnContextAction;

        _actions.Player.AttackZone.performed += OnAttackZone;

        _actions.Player.StopCommand.performed += OnStopCommand;
    }

    private void OnDisable()
    {
        
        _actions.Player.Select.performed -= OnSelectPressed;
        _actions.Player.Select.canceled -= OnSelectReleased;
        
        _actions.Player.BoxSelect.performed -= OnBoxSelectStarted;
        
        _actions.Player.MultiSelect.performed -= OnMultiSelect;
        
        _actions.Player.ContextAction.performed -= OnContextAction;
        
        _actions.Player.AttackZone.performed -= OnAttackZone;
        
        _actions.Player.StopCommand.performed -= OnStopCommand;
        
        _actions.Player.Disable();
    }

    private void Update()
    {
        if (_isDragging) _selectionBox.UpdateDrag(Mouse.current.position.ReadValue());
    }
    #endregion

    #region Input Callbacks
    private void OnSelectPressed(InputAction.CallbackContext context)
    {
        _pendingClickPosition = Mouse.current.position.ReadValue();
    }

    private void OnSelectReleased(InputAction.CallbackContext context)
    {
        if (_multiSelectHandled) { _multiSelectHandled = false; return; }
        if (_isDragging)
        {
            Rect r = _selectionBox.EndDrag();
            SelectionManager.Instance.SelectInRect(r);
            _isDragging = false;
            return;
        }

        Vector3 worldClick = ScreenToWorld(_pendingClickPosition);
        Collider2D hit = Physics2D.OverlapPoint(worldClick, _tankLayer);
        ISelectable selectable = hit != null ? hit.GetComponentInParent<ISelectable>() : null;
        if (selectable != null && !selectable.IsEnemy())
            SelectionManager.Instance.SelectSingle(selectable);
        else
            SelectionManager.Instance.DeselectAll();
    }

    private void OnBoxSelectStarted(InputAction.CallbackContext context) 
    {
        _isDragging = true;
        _selectionBox.BeginDrag(_pendingClickPosition);
    }

    private void OnMultiSelect(InputAction.CallbackContext context)
    {
        _multiSelectHandled = true;
        Vector3 worldClick = ScreenToWorld(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(worldClick, _tankLayer);
        ISelectable selectable = hit != null ? hit.GetComponentInParent<ISelectable>() : null;
        if (selectable == null || selectable.IsEnemy()) return;
        if (SelectionManager.Instance.IsSelected(selectable))
        {
            SelectionManager.Instance.RemoveFromSelection(selectable);
        }
        else
        {
            SelectionManager.Instance.AddToSelection(selectable);
        }
    }

    private void OnContextAction(InputAction.CallbackContext context)
    {
        if (_actions.Player.AttackZone.IsPressed()) return;

        Vector3 worldClick = ScreenToWorld(Mouse.current.position.ReadValue());
        var selected = SelectionManager.Instance.GetSelectedTanks();
        if (selected.Count == 0) return;
        Collider2D hit = Physics2D.OverlapPoint(worldClick, _tankLayer);
        ISelectable selectable = hit != null ? hit.GetComponentInParent<ISelectable>() : null;
        if (selectable != null && selectable.IsEnemy())
            _commandDispatcher.IssueAttackCommand(selected, selectable);
        else
            _commandDispatcher.IssueMoveCommand(selected, worldClick);
    }

    private void OnAttackZone(InputAction.CallbackContext context)
    {
        Vector3 worldClick = ScreenToWorld(Mouse.current.position.ReadValue());
        var selected = SelectionManager.Instance.GetSelectedTanks();
        if (selected.Count == 0) return;
        _commandDispatcher.IssueAttackZoneCommand(selected, worldClick);
    }

    private void OnStopCommand(InputAction.CallbackContext context)
    {
        var selected = SelectionManager.Instance.GetSelectedTanks();
        if (selected.Count == 0) return;
        _commandDispatcher.IssueStopCommand(selected);
    }
    #endregion

    #region Helper Methods
    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 world = _mainCamera.ScreenToWorldPoint(screenPos);
        world.z = 0f;
        return world;
    }
    #endregion
}
