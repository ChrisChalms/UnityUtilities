using UnityEngine;

public class yBotCameraController : MonoBehaviour
{
    #region Private

    // Serializable
    [SerializeField] private Transform _target;
    [SerializeField] private PositionSettings _position = new PositionSettings();
    [SerializeField] private SnapOrbitSettings _snapOrbit = new SnapOrbitSettings();
    [SerializeField] private InputSettings _input = new InputSettings();
    [SerializeField] private DebugSettings _debug = new DebugSettings();
    [SerializeField] private CameraCollisionHandler _collisionHandler = new CameraCollisionHandler();
    [SerializeField] private CameraModes _camMode;

    // Fields
    private Vector3 _targetPos;
    private Vector3 _destination;
    private Vector3 _adjustedDestination;
    private Vector3 _cameraVelocity;
    private float _vOrbit, _hOrbit, _zoomInput, _hOrbitSnapInput;

    #endregion

    #region Public

    public enum CameraModes
    {
        SNAP_ORBIT
    }

    #endregion

    // Initialize
    private void Start()
    {
        // Components
        _target = GameObject.Find("yBot/Follow").transform as Transform;

        // Initialize
        moveToTarget();
        _collisionHandler.Initialize(Camera.main);
        _collisionHandler.UpdateCameraClipPoints(transform.position, transform.rotation, ref _collisionHandler.AdjustedCameraClipPoints);
        _collisionHandler.UpdateCameraClipPoints(_destination, transform.rotation, ref _collisionHandler.DesiredCameraClipPoints);
    }

    // Main tick
    private void Update()
    {
        getInput();
        zoomInOnTarget();
    }

    // Move/rotate after main tick
    private void FixedUpdate()
    {
        moveToTarget();
        if (_camMode == CameraModes.SNAP_ORBIT)
            snapOrbit();
        //orbitTarget();
        lookAtTarget();

        _collisionHandler.UpdateCameraClipPoints(transform.position, transform.rotation, ref _collisionHandler.AdjustedCameraClipPoints);
        _collisionHandler.UpdateCameraClipPoints(_destination, transform.rotation, ref _collisionHandler.DesiredCameraClipPoints);
        
        for(var i = 0; i < 5; i++)
        {
            if (_debug.DrawAdjustedLines)
                Debug.DrawLine(_targetPos, _collisionHandler.AdjustedCameraClipPoints[i], Color.green);
            if (_debug.DrawDesiredLines)
                Debug.DrawLine(_targetPos, _collisionHandler.DesiredCameraClipPoints[i], Color.white);
        }
        _collisionHandler.CheckColliding(_targetPos);
        _position.AdjustmentDistance = _collisionHandler.GetAdjustedDistanceWithRay(_targetPos);
    }
    
    
    // Moves to the target - Including any offets
    private void moveToTarget()
    {
        _targetPos = _target.position + _position.TargetPosOffset;
        _destination = Quaternion.Euler(0f, _snapOrbit.YAngle, 0f) * Vector3.forward * _position.DistanceFromTarget;
        _destination += _targetPos;

        if (_collisionHandler.Colliding)
        {
            _adjustedDestination = Quaternion.Euler(0f, _snapOrbit.YAngle, 0f) * -Vector3.forward * _position.AdjustmentDistance;
            _adjustedDestination += _targetPos;

            if (_position.SmoothFollow)
                transform.position = Vector3.SmoothDamp(transform.position, _adjustedDestination, ref _cameraVelocity, _position.CollisionSmooth);
            else
                transform.position = _adjustedDestination;
        }
        else
        {
            if (_position.SmoothFollow)
                transform.position = Vector3.SmoothDamp(transform.position, _destination, ref _cameraVelocity, _position.FollowSmooth);
            else
                transform.position = _destination;
        }
    }

    // Smooth rotate to look a target
    private void lookAtTarget()
    {
        var targetRotation = Quaternion.LookRotation(_targetPos - transform.position - _position.TargetPosOffset);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, _position.LookSmooth * Time.deltaTime);
    }
    
    // Get the inputs
    private void getInput()
    {
        _vOrbit = Input.GetAxisRaw(_input.ORBIT_VERTICAL);
        _hOrbit = Input.GetAxisRaw(_input.ORBIT_HORIZONTAL);
        _hOrbitSnapInput = Input.GetAxisRaw(_input.ORBIT_HORIZONTAL_SNAP);
        _zoomInput = Input.GetAxisRaw(_input.ZOOM);
    }

    private void snapOrbit()
    {
        if (_hOrbitSnapInput == 0f) // Not pressed
        {
            _snapOrbit.ProcessedInput = false;
            return;
        }

        if (!_snapOrbit.ProcessedInput)
        {
            _snapOrbit.YAngle += _snapOrbit.SnapIncrement * ((_hOrbitSnapInput < 0) ? -1f : 1f);
            _snapOrbit.ProcessedInput = true;
        }

        if (_snapOrbit.YAngle == 360f || _snapOrbit.YAngle == -360f)
            _snapOrbit.YAngle = 0f;
    }

    // Zoom based on inputs
    private void zoomInOnTarget()
    {
        _position.DistanceFromTarget += _zoomInput * _position.ZoomSmooth * Time.deltaTime;
        if (_position.DistanceFromTarget > _position.MaxZoom)
            _position.DistanceFromTarget = _position.MaxZoom;
        if (_position.DistanceFromTarget < _position.MinZoom)
            _position.DistanceFromTarget = _position.MinZoom;
    }
}


#region Classes

[System.Serializable]
public class PositionSettings
{
    public Vector3 TargetPosOffset = new Vector3(0f, 0.5f, 0f);
    public float LookSmooth = 100f;
    public float DistanceFromTarget = -2.5f;
    public float ZoomSmooth = 40f;
    public float MaxZoom = -1f;
    public float MinZoom = -12f;
    public bool SmoothFollow = true;
    public float FollowSmooth = 0.05f;
    public float CollisionSmooth = 0.05f;

    [HideInInspector]
    public float NewDistance = -2.5f;
    [HideInInspector]
    public float AdjustmentDistance = -2.5f;
}

[System.Serializable]
public class SnapOrbitSettings
{
    public float YAngle = 0f;
    public bool ProcessedInput = false;
    public float SnapIncrement = 90f;
}

[System.Serializable]
public class InputSettings
{
    public string ORBIT_HORIZONTAL_SNAP = "OrbitHorizontalSnap";
    public string ORBIT_HORIZONTAL = "OrbitHorizontal";
    public string ORBIT_VERTICAL = "OrbitVertcal";
    public string ZOOM = "Mouse ScrollWheel";
}

[System.Serializable]
public class DebugSettings
{
    public bool DrawDesiredLines = true;
    public bool DrawAdjustedLines = true;
}

[System.Serializable]
public class CameraCollisionHandler
{
    #region Public

    public Vector3[] AdjustedCameraClipPoints;
    public Vector3[] DesiredCameraClipPoints;

    #endregion

    #region Private

    // Serializable
    [SerializeField] public LayerMask _collisionMask;

    // Components
    private Camera _camera;

    // Fields
    private bool _colliding;

    #endregion

    #region Properties

    public bool Colliding { get { return _colliding; } }

    #endregion

    // Initialize
    public void Initialize(Camera cam)
    {
        _camera = cam;
        AdjustedCameraClipPoints = new Vector3[5];
        DesiredCameraClipPoints = new Vector3[5];
    }

    // Update clip point array
    public void UpdateCameraClipPoints(Vector3 camPosition, Quaternion atRotation, ref Vector3[] intoArray)
    {
        if (!_camera)
            return;

        intoArray = new Vector3[5];
        var z = _camera.nearClipPlane;
        var x = Mathf.Tan(_camera.fieldOfView / 3.41f) * z; // Increased collision area
        var y = x / _camera.aspect;

        // Assign clip points
        intoArray[0] = (atRotation * new Vector3(-x, y, z)) + camPosition; // Top left
        intoArray[1] = (atRotation * new Vector3(x, y, z)) + camPosition; // Top right
        intoArray[2] = (atRotation * new Vector3(-x, -y, z)) + camPosition; // Bottom left
        intoArray[3] = (atRotation * new Vector3(x, -y, z)) + camPosition; // Bottom right
        intoArray[4] = camPosition - (_camera.transform.forward * 0.5f); // Size of back ray
    }

    // Checks to see if there's a collision at any of the calculated clip points
    private bool collisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
    {
        for (int i = 0; i < clipPoints.Length; i++)
        {
            Ray ray = new Ray(fromPosition, clipPoints[i] - fromPosition);
            float distance = Vector3.Distance(clipPoints[i], fromPosition);
            if (Physics.Raycast(ray, distance, _collisionMask))
                return true;
        }
        return false;
    }

    // Return distance camera needs to be to not collide
    public float GetAdjustedDistanceWithRay(Vector3 from)
    {
        var distance = -1f;

        for (var i = 0; i < DesiredCameraClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, DesiredCameraClipPoints[i] - from);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (distance == -1)
                    distance = hit.distance;
                else
                {
                    if (hit.distance < distance)
                        distance = hit.distance;
                }
            }
        }

        if (distance == -1f)
            return 0;
        else return distance;
    }

    public void CheckColliding(Vector3 targetPosition)
    {
        if (collisionDetectedAtClipPoints(DesiredCameraClipPoints, targetPosition))
            _colliding = true;
        else
            _colliding = false;
    }
}

#endregion
