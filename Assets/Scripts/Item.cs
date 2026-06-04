using UC;
using UnityEngine;

[SelectionBase]
public abstract class Item : MonoBehaviour, ITooltip
{
    [SerializeField] protected GameObject   pickupFX;
    [SerializeField] protected SoundDef     pickupSound;
    [SerializeField] protected float        deletionTime = 0.1f;
    [SerializeField, TextArea] protected string tooltipDescription;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Ball>();
        if (player)
        {
            Grab(player);
            pickupSound?.Play();
            Instantiate(pickupFX, transform.position, transform.rotation);
            Destroy(gameObject, deletionTime);
        }
    }

    public int GetOrder() => 0;
    
    public RectTransform GetTooltip(RectTransform parentTransform)
    {
        var textTooltip = Instantiate(GlobalsBase.textTooltip, parentTransform);
        textTooltip.SetText(GetTooltipDescription());

        return textTooltip.GetComponent<RectTransform>();
    }

    public virtual string GetTooltipDescription() => tooltipDescription;

    protected abstract void Grab(Ball player);
}
