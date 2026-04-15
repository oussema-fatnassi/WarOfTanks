using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    #region Fields
    [Header("Tank Settings")]
    [SerializeField] private ETankTeam _team;
    [SerializeField] private float _respawnDelay = 5f;

    [Header("References")]
    [SerializeField] private GameObject _tankBody;
    [SerializeField] private Transform _spawnPoint;

    private bool _isAlive = true;

    private HealthSystem _healthSystem;
    private Collider2D _collider;
    #endregion

    #region Events
    public event Action OnTankDied;
    public event Action OnTankRespawned;
    #endregion

    #region Properties
    public ETankTeam Team => _team;
    public bool IsAlive => _isAlive;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _collider = GetComponent<Collider2D>();
    }
    private void Start()
    {
        _healthSystem.OnDeath += Die;
    }

    private void OnDestroy()
    {
        _healthSystem.OnDeath -= Die;
    }
    #endregion

    public void Die()
    {
        if (!_isAlive) return;
        _isAlive = false;
        _tankBody.SetActive(false);
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
        _collider.enabled = true);
        _isAlive = true;
        OnTankRespawned?.Invoke();
    }
}
