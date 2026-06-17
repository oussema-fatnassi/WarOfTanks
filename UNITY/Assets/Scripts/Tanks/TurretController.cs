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
    /// <summary>Returns true when enough time has elapsed since the last shot to fire again.</summary>
    public bool CanFire => Time.time >= _lastFireTime + 1f / _fireRate;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _currentTank = GetComponentInParent<Tank>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Rotates the turret toward the target position at the configured rotation speed.
    /// </summary>
    public void RotateTo(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)_turretTransform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _turretTransform.rotation = Quaternion.RotateTowards(_turretTransform.rotation, Quaternion.Euler(0, 0, targetAngle), _turretRotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Returns true when the angle between the turret forward and the direction to the target is within the tolerance.
    /// </summary>
    public bool IsAimedAt(Vector2 targetPosition, float toleranceDegrees)
    {
        Vector2 toTarget = (targetPosition - (Vector2)_turretTransform.position).normalized;
        float angle = Vector2.Angle(_turretTransform.right, toTarget);
        return angle <= toleranceDegrees;
    }

    /// <summary>
    /// Spawns a bullet from the cannon tip and launches it forward. Pass <c>force = true</c> to bypass the fire rate cooldown.
    /// Returns the spawned <see cref="BulletController"/>, or null if the cooldown has not elapsed.
    /// </summary>
    public BulletController Fire(bool force = false)
    {
        if (!CanFire && !force) return null;
        _lastFireTime = Time.time;

        GameObject bulletObject = Instantiate(_bulletPrefab, _cannonTipTransform.position, _cannonTipTransform.rotation, _bulletPool);
        BulletController bulletController = bulletObject.GetComponent<BulletController>();
        Collider2D tankCollider = GetComponentInParent<Collider2D>();
        Collider2D bulletCollider = bulletObject.GetComponent<Collider2D>();
        if (tankCollider != null && bulletCollider != null)
            Physics2D.IgnoreCollision(bulletCollider, tankCollider);


        // TODO : Maybe call initialize with damage, speed , etc. if needed in the future.
        bulletController.SetTeam(_currentTank.TeamId);
        bulletController.Launch(_cannonTipTransform.transform.right);
        return bulletController;
    }
    #endregion
}
