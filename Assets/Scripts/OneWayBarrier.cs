using UC;
using UnityEngine;

[ExecuteInEditMode]
public class OneWayBarrier : Barrier, IConditionalObstacle
{
    [SerializeField, Min(0.0f)] protected int minEnemies = 0;
    [SerializeField, Min(0.0f)] protected int maxEnemies = 5;
    [SerializeField] private Color closeColor = Color.red;

    protected Vector3 onewayDir
    {
        get
        {
            Vector3 d = (endPoints[1].position - endPoints[0].position).x0z().normalized;

            return Vector3.Cross(d, Vector3.up);
        }
    }

    protected override float animDir => -1.0f;
    protected override Color barrierColor
    {
        get
        {
            return closeColor;
        }
    }

    public bool ShouldIgnoreCollision(BallPhysics ball)
    {
        if (Vector3.Dot(ball.linearVelocity.normalized, onewayDir) > 0.0f) return true;

        return false;
    }
    public SoundDef GetObstacleHitSound()
    {
        return blockedSound;
    }

    public void OnDrawGizmosSelected()
    {
        if (endPoints.Length < 2) return;

        DebugHelpers.DrawArrow(centerPos + Vector3.up * offsetY, onewayDir, 0.25f, 0.05f, 45.0f);
    }

}
