using UnityEngine;

public interface IConditionalObstacle 
{
    public bool ShouldIgnoreCollision(BallPhysics ball);
}
