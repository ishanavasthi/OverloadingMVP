using UnityEngine;

/// <summary>
/// Simple chase camera for the truck. It follows smoothly and looks ahead so the
/// player can read upcoming turns and obstacles.
/// </summary>
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = new(0f, 5.5f, -9f);
    [SerializeField] private Vector3 lookOffset = new(0f, 1.2f, 4f);
    [SerializeField] private float followSmoothTime = 0.15f;
    [SerializeField] private float rotationLerp = 10f;

    private Vector3 followVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.TransformPoint(followOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followSmoothTime);

        Vector3 lookPoint = target.TransformPoint(lookOffset);
        Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationLerp * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
