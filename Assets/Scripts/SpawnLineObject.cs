using UnityEngine;
using UC;

public class SpawnLineObject : MonoBehaviour
{
    public Collider Collider { get; private set; }

    private Vector3 velocity;
    private bool moving;

    public void Initialize(Collider collider)
    {
        Collider = collider;
        velocity = Vector3.zero;
        moving = false;
    }

    public void StartMoving(Vector3 speed)
    {
        velocity = speed;
        moving = true;
    }

    private void Update()
    {
        Move(Time.deltaTime);
    }

    public void Move(float dt)
    {
        if (!moving)
            return;

        transform.position += velocity * dt;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.HasHypertag(Globals.mainAreaTag))
        {
            Destroy(gameObject);
        }
    }
}
