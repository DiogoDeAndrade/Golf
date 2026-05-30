using UnityEngine;

[SelectionBase]
public abstract class Item : MonoBehaviour
{
    [SerializeField] private GameObject pickupFX;
    [SerializeField] private float      deletionTime = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Ball>();
        if (player)
        {
            Grab(player);
            Instantiate(pickupFX, transform.position, transform.rotation);
            Destroy(gameObject, deletionTime);
        }
    }

    protected abstract void Grab(Ball player);
}
