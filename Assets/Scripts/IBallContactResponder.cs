using UnityEngine;

public interface IBallContactResponder
{
    // This should return true if the contact should be handled by the ball physics system (for example, the spikes that make the player take damage and bounce back),
    // or false if the contact should be ignored (an item pickup, that should trigger an effect but not affect the ball's movement).
    public bool ShouldHandleAsCollision(BallPhysics ball, RaycastHit contact);

    public void OnBallContact(BallPhysics ball, RaycastHit contact);
}
