using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    #region Fields
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _rotationSpeed = 180f;

    [SerializeField] private Transform _tankBody;

    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private float _desiredRotation;
    #endregion


    #region Testing Getters
    public float MoveSpeed => _moveSpeed;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);
        _tankBody.Rotate(0f, 0f, _desiredRotation * _rotationSpeed * Time.fixedDeltaTime);
    }
    #endregion

    #region Public Methods
    public void Move(Vector2 direction)
    {
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _moveDirection = direction.normalized;
    }

    public void Rotate(float angle)
    {
        _desiredRotation = angle;
    }

    public void Stop()
    {
        _moveDirection = Vector2.zero;
        _desiredRotation = 0;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void RotateToward(Vector2 targetDirection)
    {
        float targetAngle = -Vector2.SignedAngle(targetDirection, Vector2.up);
        float currentAngle = _tankBody.eulerAngles.z;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
        _desiredRotation = Mathf.Abs(angleDiff) > 3f ? Mathf.Clamp(angleDiff / 15f, -1f, 1f) : 0f;
    }
    #endregion
}
