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
    [SerializeField]
    private ResourceType _shieldResource;
    [SerializeField]
    private Hypertag     _mainAreaTag;
    public static ResourceType attackResource => instance?._attackResource ?? null;
    public static ResourceType shieldResource => instance?._shieldResource ?? null;
    public static Hypertag mainAreaTag => instance?._mainAreaTag ?? null;

    public static Globals instance => GetInstanceBase<Globals>();
}
