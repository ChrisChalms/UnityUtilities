using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    #region Private

    // Serializable
    [SerializeField] public LayerMask _collisionMask;

    // Components
    private Camera _camera;

    // Fields
    private bool _colliding;
    private Vector3[] _adjustedCameraClipPoints;
    public Vector3[] _desiredCameraClipPoints;

    #endregion

    #region Properties

    public bool Colliding { get { return _colliding; } }

    #endregion

    // Initialize
    public void Initialize(Camera cam)
    {
        _camera = cam;
        _adjustedCameraClipPoints = new Vector3[5];
        _desiredCameraClipPoints = new Vector3[5];
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
        intoArray[4] = camPosition - _camera.transform.forward;
    }

    // Checks to see if there's a collision at any of the calculated clip points
    private bool collisionDetectedAtClipPoints(Vector3[] clipPoints, Vector3 fromPosition)
    {
        for(int i = 0; i < clipPoints.Length; i++)
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

        for (var i = 0; i < _desiredCameraClipPoints.Length; i++)
        {
            Ray ray = new Ray(from, _desiredCameraClipPoints[i] - from);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
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
        if (collisionDetectedAtClipPoints(_desiredCameraClipPoints, targetPosition))
            _colliding = true;
        else
            _colliding = false;
    }
}
