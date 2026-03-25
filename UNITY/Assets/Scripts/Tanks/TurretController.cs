using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    #region Fields
    [Header("References")]
    [SerializeField] private Transform _turretTransform;
    [SerializeField] private Transform _cannonTipTransform;

    [Header("Turret Settings")]
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private float _damage = 10f;
    
    private float _lastFireTime;
    #endregion

    #region Getters
    public bool CanFire => Time.time >= _lastFireTime + 1f / _fireRate;
    #endregion

    #region Unity Methods
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region Public Methods
    public void RotateTo(Vector2 targetPosition)
    {
    }

    public void Fire()
    {
    }
    #endregion
}
