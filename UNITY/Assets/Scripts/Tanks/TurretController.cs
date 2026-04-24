using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    #region Fields
    [Header("References")]
    [SerializeField] private Transform _turretTransform;
    [SerializeField] private Transform _cannonTipTransform;
    [SerializeField] private GameObject _bulletPrefab;
    // TODO: Change the bullet pool to a more efficient pooling system, like a Queue or Stack, to avoid performance issues with Instantiate and Destroy.
    // For now, we will use a simple Transform as a parent for the instantiated bullets to keep the hierarchy organized.
    [SerializeField] private Transform _bulletPool;

    [Header("Turret Settings")]
    [SerializeField] private float _turretRotationSpeed = 360f;
    [SerializeField] private float _fireRate = 1f;

    private float _lastFireTime = 0;
    private Tank _currentTank;
    #endregion

    #region Getters
    public bool CanFire => Time.time >= _lastFireTime + 1f / _fireRate;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _currentTank = GetComponentInParent<Tank>();
    }
    #endregion

    #region Public Methods
    public void RotateTo(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)_turretTransform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _turretTransform.rotation = Quaternion.RotateTowards(_turretTransform.rotation, Quaternion.Euler(0, 0, targetAngle), _turretRotationSpeed * Time.deltaTime);
    }

    public bool IsAimedAt(Vector2 targetPosition, float toleranceDegrees)
    {
        Vector2 toTarget = (targetPosition - (Vector2)_turretTransform.position).normalized;
        float angle = Vector2.Angle(_turretTransform.right, toTarget);
        return angle <= toleranceDegrees;
    }

    public BulletController Fire(bool force = false)
    {
        if (!CanFire && !force) return null;
        _lastFireTime = Time.time;

        GameObject bulletObject = Instantiate(_bulletPrefab, _cannonTipTransform.position, _cannonTipTransform.rotation, _bulletPool);
        BulletController bulletController = bulletObject.GetComponent<BulletController>();
        Physics2D.IgnoreCollision(bulletObject.GetComponent<Collider2D>(), GetComponentInChildren<Collider2D>());


        // TODO : Maybe call initialize with damage, speed , etc. if needed in the future.
        bulletController.SetTeam(_currentTank.TeamId);
        bulletController.Launch(_cannonTipTransform.transform.right);
        return bulletController;
    }
    #endregion
}
