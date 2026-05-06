using UnityEngine;
using UnityEngine.InputSystem;
using WarOfTanks.Navigation;

/// <summary>
/// Translates Unity Input System events into selection and command operations on the player's tanks.
/// Owns the drag-selection box UI, routes right-click context actions to CommandDispatcher,
/// and guards move commands against non-walkable click targets using the NavigationGrid.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    #region Fields
    [SerializeField] private SelectionBox _selectionBox;
    [SerializeField] private LayerMask _tankLayer;
    [SerializeField] private NavigationGrid _grid;

    private PlayerInputActions _actions;
    private CommandDispatcher _commandDispatcher;
    private Camera _mainCamera;
    private bool _isDragging;
    private Vector2 _pendingClickPosition;
    // Shift+LMB fires both MultiSelect.performed and Select.canceled. This flag tells
    // OnSelectReleased to skip its deselect-all logic when a multi-select already handled the press.
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

    /// <summary>
    /// LMB release handler. Finalises a box-selection drag if one was in progress; otherwise
    /// selects the friendly tank under the cursor or deselects all when clicking empty space.
    /// </summary>
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
            SelectionManager.Instance.RemoveFromSelection(selectable);
        else
            SelectionManager.Instance.AddToSelection(selectable);
    }

    /// <summary>
    /// RMB handler. Issues AttackCommand on an enemy hit, or MoveCommand on empty walkable space.
    /// A+RMB fires both ContextAction and AttackZone callbacks simultaneously; the IsPressed guard
    /// ensures a MoveCommand is not also issued on every AttackZone input.
    /// Move commands are silently rejected on non-walkable cells (Cover, Obstacle) to avoid
    /// issuing unreachable destinations to the pathfinder.
    /// </summary>
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
        {
            if (!IsWalkableAt(worldClick)) return;
            _commandDispatcher.IssueMoveCommand(selected, worldClick);
        }
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

    /// <summary>
    /// Returns true if the given world position maps to a walkable NavigationGrid cell.
    /// Returns true when <c>_grid</c> is not assigned so the system degrades gracefully
    /// rather than silently blocking all move commands if the Inspector wire is missing.
    /// </summary>
    private bool IsWalkableAt(Vector3 worldPos)
    {
        if (_grid == null) return true;
        Vector2Int cell = _grid.WorldToGridPosition(worldPos);
        PathNode node = _grid.GetNode(cell.x, cell.y);
        return node != null && node.IsWalkable;
    }
    #endregion
}
