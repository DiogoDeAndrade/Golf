using UC;
using UnityEngine;

[ExecuteInEditMode]
public class EnemyBarrier : Barrier, IConditionalObstacle
{
    [SerializeField, Min(0.0f)] protected int minEnemies = 0;
    [SerializeField, Min(0.0f)] protected int maxEnemies = 5;
    [SerializeField] private Color openColor = Color.green;
    [SerializeField] private Color closeColor = Color.red;

    protected override float animDir => -1.0f;
    protected override Color barrierColor
    {
        get
        {
            if (isOpen) return openColor.ChangeAlpha(baseColor.a);

            return closeColor.ChangeAlpha(baseColor.a);
        }
    }
    private int enemyCount => FindObjectsByType<Agent>(FindObjectsSortMode.None).Length;

    private bool isOpen
    {
        get
        {
            var ec = enemyCount;

            if ((ec >= minEnemies) && (ec <= maxEnemies)) return true;

            return false;
        }
    }

    public bool ShouldIgnoreCollision(BallPhysics ball)
    {
        return isOpen;
    }

}
