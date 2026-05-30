using UC;
using UC.RPG;
using UnityEngine;

public class Sword : Item
{
    [SerializeField] private int count = 1;

    protected override void Grab(Ball player)
    {
        var res = player.FindResourceHandler(Globals.attackResource);
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
}
