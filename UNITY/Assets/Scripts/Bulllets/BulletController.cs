using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.Enums;

public class BulletController : MonoBehaviour
{
    #region Fields
    [Header("Bullet Settings")]
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _falloffDistance = 80f;
    [Tooltip("Radius of the explosion. Tanks take full damage at the centre, falling to zero at the edge.")]
    [SerializeField] private float _explosionRadius = 2f;
    [Tooltip("Optional VFX spawned at the impact point when the shell explodes.")]
    [SerializeField] private GameObject _explosionVfx;

    private Rigidbody2D _rigidbody;
    private ETankTeam _ownerTeam;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        // Apply player-configured match specs (defaults match the prefab, so no change unless edited).
        _damage = MatchSettings.BulletDamage;
        _explosionRadius = MatchSettings.ExplosionRadius;
    }
    #endregion

    public void Launch(Vector2 direction)
    {
        // _falloffDistance now only bounds the shell's range/lifetime.
        _rigidbody.velocity = direction.normalized * _bulletSpeed;
        float lifetime = _falloffDistance / _bulletSpeed;
        Destroy(gameObject, lifetime);
    }

    public void SetTeam(ETankTeam team)
    {
        _ownerTeam = team;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tank tank = collision.GetComponentInParent<Tank>();
        // Pass straight through friendly tanks without exploding.
        if (tank != null && tank.TeamId == _ownerTeam) { return; }

        // Explode on enemy tanks and on solid environment (wall / Cover / Obstacle).
        Explode();
    }

    /// <summary>
    /// Deals area-of-effect damage around the impact point: full <see cref="_damage"/> at the centre,
    /// falling linearly to zero at <see cref="_explosionRadius"/>. Falloff is measured from the
    /// explosion, not from where the shell was fired.
    /// </summary>
    private void Explode()
    {
        Vector2 impactPoint = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, _explosionRadius);

        HashSet<HealthSystem> damaged = new HashSet<HealthSystem>();
        foreach (Collider2D hit in hits)
        {
            Tank tank = hit.GetComponentInParent<Tank>();
            if (tank == null || tank.TeamId == _ownerTeam) { continue; }

            HealthSystem healthSystem = tank.GetComponentInChildren<HealthSystem>();
            if (healthSystem == null || !damaged.Add(healthSystem)) { continue; }

            float distance = Vector2.Distance(impactPoint, tank.transform.position);
            float multiplier = Mathf.Clamp01(1f - distance / _explosionRadius);
            healthSystem.TakeDamage(_damage * multiplier);
        }

        if (_explosionVfx != null)
        {
            Instantiate(_explosionVfx, impactPoint, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
