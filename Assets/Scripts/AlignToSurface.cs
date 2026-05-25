using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class AlignToSurface : MonoBehaviour
{
    [SerializeField] private bool       alwaysAlign = true;
    [SerializeField] private float      raycastDistance = 1f;
    [SerializeField] private LayerMask  surfaceLayerMask;
    [SerializeField] private Vector3    offset;

    private void Start()
    {
        Align();

#if UNITY_EDITOR
        if (Application.isPlaying)
            enabled = alwaysAlign;
#else
        enabled = alwaysAlign;
#endif
    }

    void Update()
    {
        Align();
    }

    void Align()
    {
        Vector3 origin = transform.position + Vector3.up * raycastDistance * 0.5f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, surfaceLayerMask, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        Vector3 normal = hit.normal;

        // Preserve current forward direction as much as possible,
        // but project it onto the hit surface.
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, normal);

        // Fallback in case forward is almost parallel to the normal.
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(transform.right, normal);

            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
        }

        forward.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(forward, normal);

        transform.SetPositionAndRotation(hit.point + targetRotation * offset, targetRotation);
    }
}
