using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public enum RotationAxes
    {
        YOnly,
        XY,
        XYZ
    }

    [Tooltip("Which axes are allowed to rotate toward the camera.")]
    public RotationAxes rotationAxes = RotationAxes.YOnly;

    [Tooltip("Speed of the smooth rotation toward the camera.")]
    [Range(0.1f, 20f)]
    public float smoothSpeed = 5f;

    public bool showAss;

    private Transform cam;

    private void Start()
    {
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("[LookAtCameraAxes] No Main Camera found.");
        }
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        Vector3 direction = cam.position - transform.position;

        switch (rotationAxes)
        {
            case RotationAxes.YOnly:
                direction.y = 0f;
                break;
            case RotationAxes.XY:
                direction.z = 0f;
                break;
            case RotationAxes.XYZ:
                break;
        }

        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 dir = direction.normalized;
            if (showAss)
                dir *= -1;

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }
}
