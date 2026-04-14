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

    private float _lifetime;
    private Rigidbody2D _rigidbody;
    private ETankTeam _ownerTeam;
    #endregion

    #region Properties
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        //For testingPurposes, we will set the owner team to Player for now. We will change this later when we implement the tank controller and turret controller.
        //TODO : Add a method to initialize the bullet with the owner team and other parameters if needed in the future.
        _ownerTeam = ETankTeam.PLAYER;
    }
    void Start()
    {
        _lifetime = _falloffDistance / _speed;
        Debug.Log($"Bullet initialized with damage {_damage}, speed {_speed}, falloff distance {_falloffDistance}, and calculated lifetime {_lifetime}");
    }
    #endregion

    public void Launch(Vector2 direction)
    {
        _rigidbody.velocity = direction.normalized * _speed;
        Debug.Log($"Bullet launched with speed {_speed} and lifetime {_lifetime}");
        Destroy(gameObject,_lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Bullet hit {collision.gameObject.name}");
        Destroy(gameObject);
    }
}
