using UC.RPG;
using UnityEngine;

 [SelectionBase] public class Spikes : MonoBehaviour, IBallContactResponder
{
    [SerializeField] private int damage = 1;

    public void OnBallContact(BallPhysics ball, RaycastHit contact)
    {
        var healtSystem = ball.FindResourceHandler(Globals.healthResource);
        if (healtSystem)
        {
            healtSystem.Change(new ChangeData(-damage)
            {
                changeSrcPosition = contact.point,
                changeSrcDirection = contact.normal,
                source = gameObject
            });
        }
    }

    public bool ShouldHandleAsCollision(BallPhysics ball, RaycastHit contact)
    {
        return true;
    }
}
