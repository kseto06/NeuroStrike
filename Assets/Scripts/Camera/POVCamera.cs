using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset = new Vector3(2f, 1.5f, -2f);
    public Vector3 rotationOffset = new Vector3(0f, -45f, 0f); 

    public float smoothSpeed = 0.1f;

    void LateUpdate()
    {
        if (!target) return;

        // Move camera to local offset
        Vector3 desiredPosition = target.TransformPoint(positionOffset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Local rotational offset
        Quaternion desiredRotation = target.rotation * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);
    }
}
