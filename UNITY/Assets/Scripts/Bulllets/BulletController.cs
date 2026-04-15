using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    #region Fields
    [Header("Bullet Settings")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _speed = 10f;
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
        //For testingPurposes, we will set the owner team to Player for now. We will change this later when we implement the tank controller and turret controller.
    }
    #endregion

    public void Launch(Vector2 direction)
    {
        _startPosition = transform.position;
        _rigidbody.velocity = direction.normalized * _speed;
        float lifetime = _falloffDistance / _speed;
        Destroy(gameObject,lifetime);
    }

    public void SetTeam(ETankTeam team)
    {
        _ownerTeam = team;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Bullet hit {collision.gameObject.name}");
        Destroy(gameObject);
        float damageDealt = CalculateDamage();

        //TODO: See how health system works
            HealthSystem healthSystem = collision.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damageDealt);
            }
    }

    private float CalculateDamage()
    {
        float distanceTraveled = Vector3.Distance(_startPosition, transform.position);
        float damageMultiplier = Mathf.Clamp01(1 - (distanceTraveled / _falloffDistance));
        return _damage * damageMultiplier;
    }
}
