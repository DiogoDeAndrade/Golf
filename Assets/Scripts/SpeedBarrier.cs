using UC;
using UnityEngine;

[ExecuteInEditMode]
public class SpeedBarrier : Barrier, IConditionalObstacle
{
    [SerializeField, Min(0.0f)] protected float minSpeed = 0.0f;
    [SerializeField, Min(0.0f)] protected float maxSpeed = 10.0f;

    protected override float animDir => (minSpeed > 0.0f) ? (-1.0f) : (1.0f);

    protected float speed
    {
        get
        {
            if (LevelManager.instance.heldBall)
            {
                // We have a held ball, get the strength it's being held with
                return LevelManager.instance.heldBall.potentialVelocity;
            }
            else
            {
                if (LevelManager.instance.gameBall)
                {
                    // No held ball, but we have a game ball, get its current speed
                    return LevelManager.instance.gameBall.velocity;
                }
                else
                {
                    // No held ball and no game ball, just use 0
                    return 0.0f;
                }
            }
        }
    }
    protected override Color barrierColor
    {
        get
        {
            if (speed < minSpeed)
            {
                return Color.red.ChangeAlpha(baseColor.a);
            }
            else if (speed > maxSpeed)
            {
                return Color.red.ChangeAlpha(baseColor.a);
            }
            else
            {
                return Color.green.ChangeAlpha(baseColor.a);
            }
        }
    }

    public bool ShouldIgnoreCollision(BallPhysics ball)
    {
        float speed = ball.linearVelocity.x0z().magnitude;

        return (speed >= minSpeed) && (speed <= maxSpeed);
    }

}
