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
            if (isOpen) return openColor;

            return closeColor;
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


    public SoundDef GetObstacleHitSound()
    {
        return blockedSound;
    }

    protected override string GetTooltipDescription()
    {
        string str = $"<color=#{barrierColor.ToHex()}>Enemy Barrier</color>\n";

        if (maxEnemies == 0.0f)
            str += $"Only lets the ball pass if there are no living animals";
        else if (minEnemies == 0.0f)
        {
            str += $"Only lets the ball pass if there are less than {maxEnemies + 1} animals living";
        }
        else
        {
            if (maxEnemies > 1000.0f)
                str += $"Only lets the ball pass if there are more than {minEnemies - 1} animals living";
            else
                str += $"Only let's the ball pass if there are between {minEnemies} and {maxEnemies} animals living";
        }

        return str;
    }
}
