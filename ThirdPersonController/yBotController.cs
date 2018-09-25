using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class yBotController : MonoBehaviour
{
    #region Private

    // Serialized
    [SerializeField] private float _turnSpeed = 360;
    [SerializeField] private float _turnDampTime = 0.25f;
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _gravity = -350f;

    // Components
    private CharacterController _characterController;
    private Animator _animator;
    private Transform _cameraTrans;
    
    // Fields
    private float _turnAmount;
    private float _forwardAmount;

    // Hashes
    private int _turnHash;
    private int _speedHash;

    #endregion

    private void Start()
    {
        // Components
        _characterController = GetComponent<CharacterController>() as CharacterController;
        _animator = GetComponent<Animator>() as Animator;
        _cameraTrans = Camera.main.transform;

        // Hashes
        _turnHash = Animator.StringToHash("Turn");
        _speedHash = Animator.StringToHash("Speed");
    }

    // Physics tick
    private void FixedUpdate()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        
        // Camera relative
        var camForward = Vector3.Scale(_cameraTrans.forward, new Vector3(1, 0, 1)).normalized;
        var moveDir = (v * camForward) + (h * _cameraTrans.right);

        // Move player
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        if (_characterController.isGrounded)
            moveDir *= _movementSpeed;
        moveDir.y += _gravity * Time.deltaTime;
        _characterController.Move(moveDir * Time.deltaTime);

        moveDir = transform.InverseTransformDirection(moveDir);
        moveDir = Vector3.ProjectOnPlane(moveDir, transform.up);
        _turnAmount = Mathf.Atan2(moveDir.x, moveDir.z);
        _forwardAmount = moveDir.z;

        // Rotate Character=
        transform.Rotate(0, _turnAmount * _turnSpeed * Time.deltaTime, 0);
        
        // Update animator
        _animator.SetFloat(_turnHash, _turnAmount, _turnDampTime, Time.deltaTime);
        _animator.SetFloat(_speedHash, Mathf.Abs(_forwardAmount));
    }
}
