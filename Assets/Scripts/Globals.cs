using NaughtyAttributes;
using UC;
using UC.RPG;
using UnityEngine;

[CreateAssetMenu(fileName = "Globals", menuName = "GolfQuest/Globals")]
public class Globals : GlobalsBase
{
    [HorizontalLine(color: EColor.Green)]
    [SerializeField]
    private ResourceType _attackResource;
    public static ResourceType attackResource => instance?._attackResource ?? null;

    public static Globals instance => GetInstanceBase<Globals>();
}
