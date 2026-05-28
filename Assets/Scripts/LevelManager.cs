using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UC.RPG;

public class LevelManager : MonoBehaviour
{
    public delegate void StrokeTaken(int strokeCount);
    public event StrokeTaken onStrokeTaken;

    [SerializeField] 
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl mouseClickControl;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl mousePosition;
    [SerializeField]
    private Camera          mainCamera;
    [SerializeField]
    private LayerMask       ballLayers;
    [SerializeField]
    private Ball            ballPrefab;
    [Header("RPG")]
    [SerializeField]
    private bool            healthEnabled = false;
    [SerializeField]
    private Hypertag        healthResourceDisplay;
    [Header("Titles")]
    [SerializeField]
    private CanvasGroup congratsCanvas;
    [SerializeField]
    private CanvasGroup gameOverCanvas;

    private List<Marker>                goalMarkers;
    private TacticalCameraController    cameraCtrl;
    private int                         strokeCount = 0;

    public Ball heldBall { get; private set; }
    public Ball gameBall { get; private set; }

    private static LevelManager _instance;
    public static LevelManager instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindFirstObjectByType<LevelManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if ((_instance == null) || (_instance == this))
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        mouseClickControl.playerInput = playerInput;
        mousePosition.playerInput = playerInput;

        cameraCtrl = mainCamera.GetComponent<TacticalCameraController>();

        var startMarker = Marker.FindMarker(Marker.Type.Start);
        if (startMarker)
        {
            gameBall = Instantiate(ballPrefab, startMarker.transform.position, Quaternion.identity);

            var healthDisplay = healthResourceDisplay.FindFirst<ResourceBar>();
            healthDisplay.gameObject.SetActive(healthEnabled);
            healthDisplay.SetTarget(gameBall.FindResourceHandler(Globals.healthResource));
        }
        goalMarkers =  Marker.FindMarkers(Marker.Type.Goal);

        congratsCanvas.alpha = 0.0f;
        gameOverCanvas.alpha = 0.0f;

        TooltipManager.isTooltipEnabled += TooltipManager_isTooltipEnabled;

    }

    private void OnDestroy()
    {
        TooltipManager.isTooltipEnabled -= TooltipManager_isTooltipEnabled;
    }

    private bool TooltipManager_isTooltipEnabled()
    {
        return heldBall == null;
    }

    void Update()
    {
        if (congratsCanvas.alpha > 0) return;
        if (gameOverCanvas.alpha > 0) return;

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
                    if (ball.CanSelect())
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
                if (heldBall.Release())
                {
                    strokeCount++;
                    onStrokeTaken?.Invoke(strokeCount);
                }
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

        CheckGoals();
    }

    void CheckGoals()
    {
        if (gameBall == null) return;

        foreach (var g in goalMarkers)
        {
            if (Vector3.Distance(gameBall.transform.position, g.transform.position) < g.radius)
            {
                OnBallInHole();
                break;
            }
        }
    }

    public void OnBallInHole()
    {
        congratsCanvas.FadeIn(0.5f);
        heldBall?.Release();
        heldBall = null;
    }

    public void GameOver()
    {
        gameOverCanvas.FadeIn(0.5f);
        heldBall?.Release();
        heldBall = null;
    }
}
