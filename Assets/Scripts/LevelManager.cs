using UC;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [SerializeField] 
    private PlayerInput playerInput;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl mouseClickControl;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl mousePosition;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private LayerMask ballLayers;

    private Ball                        heldBall;
    private TacticalCameraController    cameraCtrl;

    void Start()
    {
        mouseClickControl.playerInput = playerInput;
        mousePosition.playerInput = playerInput;

        cameraCtrl = mainCamera.GetComponent<TacticalCameraController>();
    }

    void Update()
    {
        if (mouseClickControl.IsDown())
        {
            var ray = mainCamera.ScreenPointToRay(mousePosition.GetAxis2());
            var hits = Physics.RaycastAll(ray, ballLayers);
            foreach (var hit in hits)
            {
                var ball = hit.collider.GetComponent<Ball>();
                if (ball == null) ball = hit.collider.GetComponentInParent<Ball>();
                if (ball)
                {
                    if (!ball.isMoving)
                    {
                        heldBall = ball;
                        cameraCtrl.panBorderEnable = false;
                    }
                }
            }
        }
        else if (heldBall)
        {
            if (mouseClickControl.IsUp())
            {
                heldBall.Release();
                heldBall = null;
                cameraCtrl.panBorderEnable = true;
            }
            else
            {
                // Compute the position of the cursor in the plane where the ball is
                var ray = mainCamera.ScreenPointToRay(mousePosition.GetAxis2());
                var hitPos = ray.origin + ray.direction * (heldBall.transform.position.y - ray.origin.y) / ray.direction.y;

                heldBall.Hold(hitPos);
            }
        }
    }
}
