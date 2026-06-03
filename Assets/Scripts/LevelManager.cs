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
    private Hypertag        healthResourceDisplay;
    [SerializeField]
    private Hypertag        attackResourceDisplay;
    [SerializeField]
    private Hypertag        defenseResourceDisplay;
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

#if UNITY_EDITOR
        // Load level if needed
        if (GameManager.instance.state == GameManager.State.Playing)
        {
            GameManager.instance.ResetLevel();
        }
        else
        {
            GameManager.instance.SetDebugLevel(GameManager.instance.startLevel);
        }
#else
        GameManager.instance.ResetLevel();
#endif
        ResetComplete();
    }

    void ResetComplete()
    { 
        var startMarker = Marker.FindMarker(Marker.Type.Start);
        if (startMarker)
        {
            gameBall = Instantiate(ballPrefab, startMarker.transform.position, Quaternion.identity);

            // Reset view
            cameraCtrl.ResetToPosition(gameBall.transform.position);

            var healthDisplay = healthResourceDisplay.FindFirst<ResourceBar>();
            healthDisplay.gameObject.SetActive(GameManager.instance.currentMap.healthDisplay);
            healthDisplay.SetTarget(gameBall.FindResourceHandler(Globals.healthResource));

            var attackDisplay = attackResourceDisplay.FindFirst<ResourceBar>();
            attackDisplay.gameObject.SetActive(true);
            attackDisplay.SetTarget(gameBall.FindResourceHandler(Globals.attackResource));

            var defenseDisplay = defenseResourceDisplay.FindFirst<ResourceBar>();
            defenseDisplay.gameObject.SetActive(true);
            defenseDisplay.SetTarget(gameBall.FindResourceHandler(Globals.shieldResource));
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
        if (gameOverCanvas.alpha > 0) return;

        congratsCanvas.FadeIn(0.5f);
        congratsCanvas.interactable = true;
        congratsCanvas.blocksRaycasts = true;
        heldBall?.Release();
        heldBall = null;
        gameBall.Stop();
    }

    public void GameOver()
    {
        if (congratsCanvas.alpha > 0) return;

        gameOverCanvas.FadeIn(0.5f);
        gameOverCanvas.interactable = true;
        gameOverCanvas.blocksRaycasts = true;
        heldBall?.Release();
        heldBall = null;
        gameBall.Stop();
    }

    public void RetryLevel()
    {
        GameManager.instance.RetryLevel();
    }

    public void NextLevel()
    {
        GameManager.instance.NextLevel();
    }
    public void MainMenu()
    {
        GameManager.instance.MainMenu();
    }
}
