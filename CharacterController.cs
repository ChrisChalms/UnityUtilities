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
    private float _directionSpeed = 3f;
    [SerializeField]
    private float _directionDampTime = 0.25f;
    [SerializeField]
    private float _speedDampTime = 0.05f;

    // Components
    private Transform _camTransform;
    private Animator _animator;

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

        // Hashes
        _directionHash = Animator.StringToHash("Direction");
        _speedHash = Animator.StringToHash("Speed");
    }

    // Main tick
    private void Update()
    {
        _inputVector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        _inputVector = Quaternion.Euler(0f, _camTransform.eulerAngles.y, 0f) * _inputVector;
        _direction = _inputVector.x;
        _speed = _inputVector.sqrMagnitude;
        
        if (_inputVector != Vector3.zero)
        {
            var angleToMove = Vector3.Angle(transform.forward, _inputVector) * (Vector3.Cross(_inputVector, transform.forward).y >= 0 ? -1f : 1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, transform.eulerAngles.y + angleToMove, 0f), _turnSpeed * Time.deltaTime);
        }

        _animator.SetFloat(_directionHash, _direction, _directionDampTime, Time.deltaTime);
        _animator.SetFloat(_speedHash, _speed, _speedDampTime, Time.deltaTime);
    }
}
