using UC;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[ExecuteInEditMode]
public class SpeedBarrier : MonoBehaviour, IConditionalObstacle
{
    [SerializeField] private LineRenderer   lineRenderer;
    [SerializeField] private Transform[]    endPoints;
    [SerializeField] private BoxCollider    boxCollider;
    [SerializeField] private float          offsetY;
    [SerializeField] private float          animationSpeed;

    [SerializeField, Min(0.0f)] private float          minSpeed = 0.0f;
    [SerializeField, Min(0.0f)] private float          maxSpeed = 10.0f;

    public bool ShouldIgnoreCollision(BallPhysics ball)
    {
        float speed = ball.linearVelocity.x0z().magnitude;

        return (speed >= minSpeed) && (speed <= maxSpeed);
    }

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
        var material = lineRenderer.material;

        if (minSpeed > 0.0f)
        {
            material.mainTextureScale = new Vector2(1.0f, -1.0f);
        }
        else
        {
            material.mainTextureScale = new Vector2(1.0f, 1.0f);
        }

        material.mainTextureOffset -= Vector2.up * animationSpeed * Time.deltaTime;

        Color color = material.color;
        float speed;

        if (LevelManager.instance.heldBall)
        {
            // We have a held ball, get the strength it's being held with
            speed = LevelManager.instance.heldBall.potentialVelocity;
        }
        else
        {
            if (LevelManager.instance.gameBall)
            {
                // No held ball, but we have a game ball, get its current speed
                speed = LevelManager.instance.gameBall.velocity;
            }
            else
            {
                // No held ball and no game ball, just use 0
                speed = 0.0f;
            }
        }

        if (speed < minSpeed)
        {
            color = Color.red.ChangeAlpha(color.a);
        }
        else if (speed > maxSpeed)
        {
            color = Color.red.ChangeAlpha(color.a);
        }
        else
        {
            color = Color.green.ChangeAlpha(color.a);
        }

        material.color = color;
    }
}
