using UC;
using UnityEngine;

[ExecuteInEditMode]
public abstract class Barrier : MonoBehaviour
{
    [SerializeField] protected LineRenderer   lineRenderer;
    [SerializeField] protected Transform[]    endPoints;
    [SerializeField] protected BoxCollider    boxCollider;
    [SerializeField] protected float          offsetY;
    [SerializeField] protected float          animationSpeed;

    protected MaterialPropertyBlock   mpb;
    protected Vector4                 textureST = new(1.0f, 1.0f, 0.0f, 0.0f);
    protected Color                   baseColor;

    protected abstract float animDir { get; }
    protected abstract Color barrierColor { get; }

    // Update is called once per frame
    void Update()
    {
        if ((lineRenderer == null) || (endPoints == null) || (endPoints.Length < 2))
            return;

        Vector3 a = endPoints[0].position + Vector3.up * offsetY;
        Vector3 b = endPoints[1].position + Vector3.up * offsetY;

        // Pick a stable ordering for the line.
        // For a top-down golf course, X/Z ordering is usually enough.
        bool swap =
            (a.x > b.x) ||
            (Mathf.Approximately(a.x, b.x) && a.z > b.z);

        if (swap)
        {
            (a, b) = (b, a);
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, a);
        lineRenderer.SetPosition(1, b);

        if (boxCollider)
        {
            var width = lineRenderer.startWidth;

            boxCollider.transform.position = (a + b) / 2;
            boxCollider.center = Vector3.zero;
            boxCollider.size = new Vector3(Vector3.Distance(a, b), width, 0.02f);

            Vector3 dir = b - a;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Vector3 right = dir.normalized;          // collider local X
                Vector3 up = Vector3.up;                 // collider local Y
                Vector3 forward = Vector3.Cross(right, up).normalized; // collider local Z

                // Recompute up to ensure everything is orthogonal
                up = Vector3.Cross(forward, right).normalized;

                boxCollider.transform.rotation = Quaternion.LookRotation(forward, up);
            }
        }

        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        if (mpb == null)
        {
            mpb = new();
            baseColor = lineRenderer.sharedMaterial.GetColor("_BaseColor");
        }
        lineRenderer.GetPropertyBlock(mpb);

        textureST.y = animDir;
        textureST.w -= animationSpeed * Time.deltaTime;

        mpb.SetVector("_BaseMap_ST", textureST);
        mpb.SetColor("_BaseColor", barrierColor);

        lineRenderer.SetPropertyBlock(mpb);
    }
}
