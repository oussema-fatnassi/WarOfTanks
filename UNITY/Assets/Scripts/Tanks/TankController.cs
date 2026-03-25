using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    #region Fields
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 180f;

    private Rigidbody2D _rigidbody;
    private Vector2 _desiredPosition;
    private float _desiredRotation;
    #endregion


    #region Testing Getters
    public float MoveSpeed => _moveSpeed;
    #endregion

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start() { }
    private void Update() { }

    void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _desiredPosition * _moveSpeed * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(_rigidbody.rotation + _desiredRotation * _rotationSpeed * Time.fixedDeltaTime);
    }
    public void Move(Vector2 direction)
    {
        _desiredPosition = direction.normalized;
    }

    public void Rotate(float angle)
    {
        _desiredRotation = angle;
    }

    public void Stop()
    {
        _desiredPosition = Vector2.zero;
        _desiredRotation = 0;
    }
}
