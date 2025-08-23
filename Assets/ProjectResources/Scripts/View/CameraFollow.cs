using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target; // player transform

    [Header("Follow Settings")]
    [SerializeField] float smoothSpeed = 0.15f;   // lower = smoother, higher = snappier
    [SerializeField] Vector3 offset = new Vector3(0, 1, -10); // adjust to your game

    [Header("Bounds (Optional)")]
    [SerializeField] bool useBounds = false;
    [SerializeField] Vector2 minBounds;
    [SerializeField] Vector2 maxBounds;

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);

        if (useBounds)
        {
            smoothedPos.x = Mathf.Clamp(smoothedPos.x, minBounds.x, maxBounds.x);
            smoothedPos.y = Mathf.Clamp(smoothedPos.y, minBounds.y, maxBounds.y);
        }

        transform.position = smoothedPos;
    }
}
