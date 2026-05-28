using UC;
using UnityEngine;
using UnityEngine.Rendering;

public class Map : MonoBehaviour
{
    [field:SerializeField] public int par { get; private set; } = 3;

    [SerializeField] private Hypertag playerTag;

    private void Start()
    {
        var boxCollider = GetComponent<BoxCollider>();
        var mainCamera = FindFirstObjectByType<TacticalCameraController>();
        if ((mainCamera) && (boxCollider))
        {
            mainCamera.targetBoundsCollider = boxCollider;

            var ball = playerTag.FindFirst<Ball>();
            if (ball)
            {
                mainCamera.ResetToPosition(ball.transform.position);
            }
            else
            {
                mainCamera.ResetToDefault();
            }
        }
    }
}
