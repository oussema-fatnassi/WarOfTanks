using System;
using System.Collections;
using System.Collections.Generic;
using WarOfTanks.Navigation;
using UnityEngine;

public class Tank : MonoBehaviour, ISelectable, ICommandReceiver, ITankComponents
{
    #region Fields
    [Header("Tank Settings")]
    [SerializeField] private ETankTeam _teamId;
    [SerializeField] private float _respawnDelay = 5f;

    [Header("References")]
    [SerializeField] private GameObject _tankBody;
    [SerializeField] private GameObject _cannon;
    [SerializeField] private Transform _spawnPoint;

    [Header("Selection & Commands")]
    [SerializeField] private NavigationStrategy _navigationStrategy;
    [SerializeField] private float _firingRange;

    [Header("Navigation")]
    [SerializeField] private LayerMask _tankLayerMask;
    [SerializeField] private float _blockScanRadius = 2f;
    private NavigationGrid _navigationGrid;

    private bool _isAlive = true;

    private Collider2D _collider; 
    private HealthSystem _healthSystem;
    private TankController _tankController;
    private TurretController _turretController;
    private ICommand _currentCommand;
    #endregion

    #region Events
    public event Action OnTankDied;
    public event Action OnTankRespawned;
    public event Action<bool> OnSelected;
    #endregion

    #region Properties
    public ETankTeam TeamId => _teamId;
    public bool IsAlive => _isAlive;
    public TankController Controller => _tankController;
    public TurretController Turret => _turretController;
    public NavigationStrategy Navigation => _navigationStrategy;
    public float FiringRange => _firingRange;
    public ICommand CurrentCommand => _currentCommand;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _collider = _tankBody.GetComponent<Collider2D>();
        _tankController = GetComponent<TankController>();
        _turretController = GetComponent<TurretController>();
        _navigationStrategy = GetComponent<NavigationStrategy>();
        _healthSystem = GetComponent<HealthSystem>();
        _navigationGrid = FindObjectOfType<NavigationGrid>();
    }
    private void Start()
    {
        _healthSystem.OnDeath += Die;
        if (SelectionManager.Instance == null)
        { 
            Debug.LogWarning("No SelectionManager found in the scene. Please add one to manage tank selection.");
            return;
        }
        if (!IsEnemy()) SelectionManager.Instance?.RegisterFriendlyTank(this);
    }

    private void OnDestroy()
    {
        _healthSystem.OnDeath -= Die;
        SelectionManager.Instance?.UnregisterFriendlyTank(this);
    }

    private void Update()
    {
        _currentCommand?.Tick();
        if (_currentCommand?.IsComplete == true) CancelCommand();
    }
    #endregion

    #region Tank Life Methods
    public void Die()
    {
        if (!_isAlive) return;
        CancelCommand();
        _isAlive = false;
        _tankBody.SetActive(false);
        _cannon.SetActive(false);
        _collider.enabled = false;
        OnTankDied?.Invoke();
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(_respawnDelay);
        Respawn();
    }

    private void Respawn()
    {
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;
        _healthSystem.RestoreHealth();
        _tankBody.SetActive(true);
        _cannon.SetActive(true);
        _collider.enabled = true;
        _isAlive = true;
        OnTankRespawned?.Invoke();
    }
    #endregion

    #region Implementation of ISelectable
    public void SetSelected(bool selected) => OnSelected?.Invoke(selected);
    public bool IsEnemy() => _teamId == ETankTeam.ENEMY;
    public Vector3 GetWorldPosition() => transform.position;
    #endregion

    #region Implementation of ICommandReceiver
    public void SetCommand(ICommand command)
    {
        _currentCommand?.Cancel();
        _currentCommand = command;
        _currentCommand?.Start();
    }

    public void CancelCommand()
    {
        _currentCommand?.Cancel();
        _currentCommand = null;
    }
    #endregion

    #region Implementation of ITankComponents
    public HashSet<Vector2Int> GetBlockedCells(Vector2 near)
    {
        var blocked = new HashSet<Vector2Int>();
        if (_navigationGrid == null)
            return blocked;

        Collider2D[] hits = Physics2D.OverlapCircleAll(near, _blockScanRadius, _tankLayerMask);
        foreach (Collider2D col in hits)
        {
            if (col.transform.root == transform)
                continue;

            Vector2Int cell = _navigationGrid.WorldToGridPosition(col.transform.root.position);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int p = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (_navigationGrid.IsValidPosition(p.x, p.y))
                        blocked.Add(p);
                }
            }
        }
        return blocked;
    }
    #endregion

}
