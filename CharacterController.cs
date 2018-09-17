using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class yBotController : MonoBehaviour
{
    #region Private

    // Serializable
    [SerializeField]
    private float _turnSpeed = 5f;
    [SerializeField]
    private float _directionDampTime = 0.1f;
    [SerializeField]
    private float _speedDampTime = 0.04f;
    [SerializeField]
    private float _movementSpeed = 3f;
    [SerializeField]
    private float _gravity = -20f;

    // Components
    private Transform _camTransform;
    private Animator _animator;
    private CharacterController _characterController;

    // Fields
    private Vector3 _inputVector;
    private float _direction;
    private float _speed;

    // Hashes
    private int _directionHash;
    private int _speedHash;

    #endregion

    // Initialize
    private void Start()
    {
        // Components
        _camTransform = Camera.main.transform as Transform;
        _animator = GetComponent<Animator>() as Animator;
        _characterController = GetComponent<CharacterController>() as CharacterController;

        // Hashes
        _directionHash = Animator.StringToHash("Direction");
        _speedHash = Animator.StringToHash("Speed");
    }

    // Main tick
    private void Update()
    {
        _inputVector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
        _speed = _inputVector.sqrMagnitude;

        if (_inputVector != Vector3.zero)
            inputAxisToWorldSpace();

        _animator.SetFloat(_directionHash, _inputVector.x, _directionDampTime, Time.deltaTime);
        _animator.SetFloat(_speedHash, _speed, _speedDampTime, Time.deltaTime);


        if (_characterController.isGrounded)
            _inputVector *= _movementSpeed;
        _inputVector.y += _gravity * Time.deltaTime;
        _characterController.Move(_inputVector * Time.deltaTime);
    }

    // Converts the input acis to world space and rotates the this smoothly
    private void inputAxisToWorldSpace()
    {
        var camDirection = _camTransform.forward;
        camDirection.y = 0f;
        var referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(camDirection));
        var moveDirection = referentialShift * _inputVector;
        var axisSign = Vector3.Cross(moveDirection, transform.forward);
        var angleRootToMove = Vector3.Angle(transform.forward, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);
        _direction = angleRootToMove * _turnSpeed;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, transform.eulerAngles.y + angleRootToMove, 0f), _turnSpeed * Time.deltaTime);
    }
}
