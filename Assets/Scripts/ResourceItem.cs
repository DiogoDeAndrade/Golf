using UC;
using UC.RPG;
using UnityEngine;

public class ResourceItem : Item
{
    [SerializeField] private int            count = 1;
    [SerializeField] private ResourceType   resourceType;

    protected override void Grab(Ball player)
    {
        var res = player.FindResourceHandler(resourceType);
        if (res != null)
        {
            res.Change(new ChangeData(count)
            {
                changeSrcPosition = transform.position,
                changeSrcDirection = (player.transform.position - transform.position).x0z().normalized,
                source = gameObject
            });
        }
    }

    public override string GetTooltipDescription()
    {
        return tooltipDescription.Replace("{count}", $"{count}");
    }
    
}
