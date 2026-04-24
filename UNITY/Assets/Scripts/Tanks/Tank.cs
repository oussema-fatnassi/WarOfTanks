using System;
using System.Collections;
using System.Collections.Generic;
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

}
