using UnityEngine;

/// <summary>
/// Controls the physical movement and body rotation of a tank. Exposes Move, Stop, Rotate,
/// and RotateToward methods consumed by ICommand implementations. Uses Rigidbody2D.MovePosition
/// for kinematic-style movement with physics collision detection. Applies Rigidbody2D constraints
/// to prevent a stopped tank from being displaced by collision impulses from other moving tanks.
/// </summary>
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

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);
        _tankBody.Rotate(0f, 0f, _desiredRotation * _rotationSpeed * Time.fixedDeltaTime);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Begins moving the tank in the given direction. Relaxes FreezeAll (set by Stop) back to
    /// FreezeRotation so MovePosition can function again. FreezeRotation is kept because body
    /// rotation is driven directly via _tankBody.Rotate(), not through Rigidbody2D physics.
    /// </summary>
    /// <param name="direction">Movement direction; normalized internally before application.</param>
    public void Move(Vector2 direction)
    {
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        _moveDirection = direction.normalized;
    }

    /// <summary>Sets the tank body's rotation rate. Applied each FixedUpdate via _tankBody.Rotate().</summary>
    /// <param name="angle">Signed rotation multiplier scaled by _rotationSpeed.</param>
    public void Rotate(float angle)
    {
        _desiredRotation = angle;
    }

    /// <summary>
    /// Halts all movement and freezes the Rigidbody2D entirely. FreezeAll prevents collision
    /// impulses from other Dynamic Rigidbody2D tanks using MovePosition from pushing this tank
    /// while it is stopped.
    /// </summary>
    public void Stop()
    {
        _moveDirection = Vector2.zero;
        _desiredRotation = 0;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    /// <summary>
    /// Smoothly rotates the tank body toward a target direction over multiple frames using
    /// SignedAngle clamped to [-1, 1], producing natural deceleration as the tank nears the target heading.
    /// </summary>
    /// <param name="targetDirection">World-space direction vector to face.</param>
    public void RotateToward(Vector2 targetDirection)
    {
        float targetAngle = -Vector2.SignedAngle(targetDirection, Vector2.up);
        float currentAngle = _tankBody.eulerAngles.z;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
        _desiredRotation = Mathf.Abs(angleDiff) > 3f ? Mathf.Clamp(angleDiff / 15f, -1f, 1f) : 0f;
    }
    #endregion
}
