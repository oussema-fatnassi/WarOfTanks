using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    #region Fields
    [Header("Bullet Settings")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _cannonRotationSpeed = 10f;
    [SerializeField] private float _falloffDistance = 80f;

    private Rigidbody2D _rigidbody;
    private Vector3 _startPosition;
    private ETankTeam _ownerTeam;
    #endregion

    #region Properties
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    #endregion

    public void Launch(Vector2 direction)
    {
        _startPosition = transform.position;
        _rigidbody.velocity = direction.normalized * _cannonRotationSpeed;
        float lifetime = _falloffDistance / _cannonRotationSpeed;
        Destroy(gameObject,lifetime);
    }

    public void SetTeam(ETankTeam team)
    {
        _ownerTeam = team;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (TryGetComponent<Tank>(out Tank tank) && tank.Team == _ownerTeam) { return; }
        //TODO : Can projectiles collide ?
        float damageDealt = CalculateDamage();

        HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damageDealt);
        }
        Destroy(gameObject);
    }

    private float CalculateDamage()
    {
        float distanceTraveled = Vector3.Distance(_startPosition, transform.position);
        float damageMultiplier = Mathf.Clamp01(1 - (distanceTraveled / _falloffDistance));
        return _damage * damageMultiplier;
    }
}
