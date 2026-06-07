using UC;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Camera")]
    [Tooltip("1 = fit the whole level. Below 1 zooms in and lets the level go beyond the screen.")]
    [SerializeField, Range(0.5f, 1.5f)]
    private float menuCameraFraming = 0.85f;
    [SerializeField]
    private float minPerspectiveDistance = 2.0f;
    [Header("Menu Camera Sway")]
    [SerializeField]
    private bool animateMenuCamera = true;

    [SerializeField]
    private float cameraSwayPositionAmplitude = 0.35f;

    [SerializeField]
    private float cameraSwayRotationAmplitude = 1.5f;

    [SerializeField]
    private float cameraSwaySpeed = 0.25f;

    [SerializeField]
    private CanvasGroup     mainMenuCanvas;
    [SerializeField]
    private CanvasGroup     optionsCanvas;
    [SerializeField]
    private BigTextScroll   creditsScroll;

    [SerializeField]
    private GameObject continueButtonContainer;

    private Transform menuCameraTransform;
    private Vector3 menuCameraBasePosition;
    private Quaternion menuCameraBaseRotation;

    void Start()
    {
        int levelData = PlayerPrefs.GetInt("MaxLevel", 0);
        continueButtonContainer?.SetActive(levelData > 0);

        GameManager.instance.LoadLevel(levelData);

        var map = GameManager.instance.currentMap;
        FocusCameraOnGoal(map);
    }

    void FocusCameraOnGoal(Map map)
    {
        var tacticalCamera = FindFirstObjectByType<TacticalCameraController>();
        if (!tacticalCamera) return;

        var cam = tacticalCamera.GetComponent<Camera>();
        if (!cam) return;

        var mapBounds = map ? map.GetComponent<BoxCollider>() : null;

        if (mapBounds)
        {
            tacticalCamera.targetBoundsCollider = mapBounds;
        }


        Vector3 focusPosition = Vector3.zero;

        /*var goalMarker = Marker.FindMarker(Marker.Type.Goal);
        if (goalMarker)
        {
            focusPosition = goalMarker.transform.position;
        }
        else */ if (mapBounds)
        {
            focusPosition = mapBounds.bounds.center;
        }
        else
        {
            return;
        }

        Vector3[] boundsCorners = mapBounds ? GetBoxColliderCorners(mapBounds) : null;

        if (cam.orthographic)
        {
            FocusOrthographicCamera(cam, focusPosition, boundsCorners);
        }
        else
        {
            FocusPerspectiveCamera(cam, focusPosition, boundsCorners);
        }

        menuCameraTransform = cam.transform;
        menuCameraBasePosition = menuCameraTransform.position;
        menuCameraBaseRotation = menuCameraTransform.rotation;

        if (animateMenuCamera)
        {
            tacticalCamera.enabled = false;
        }
    }

    void FocusOrthographicCamera(Camera cam, Vector3 focusPosition, Vector3[] boundsCorners)
    {
        Transform camTransform = cam.transform;

        float currentDistance = Vector3.Distance(camTransform.position, focusPosition);
        camTransform.position = focusPosition - camTransform.forward * currentDistance;

        if (boundsCorners == null || boundsCorners.Length == 0)
            return;

        float aspect = cam.aspect;
        float requiredOrthoSize = 0.0f;

        foreach (Vector3 corner in boundsCorners)
        {
            Vector3 offset = corner - focusPosition;

            float vertical = Mathf.Abs(Vector3.Dot(offset, camTransform.up));
            float horizontal = Mathf.Abs(Vector3.Dot(offset, camTransform.right)) / aspect;

            requiredOrthoSize = Mathf.Max(requiredOrthoSize, vertical, horizontal);
        }

        cam.orthographicSize = requiredOrthoSize * menuCameraFraming;
    }

    void FocusPerspectiveCamera(Camera cam, Vector3 focusPosition, Vector3[] boundsCorners)
    {
        Transform camTransform = cam.transform;

        if (boundsCorners == null || boundsCorners.Length == 0)
        {
            float currentDistance = Vector3.Distance(camTransform.position, focusPosition);
            camTransform.position = focusPosition - camTransform.forward * currentDistance;
            return;
        }

        float low = minPerspectiveDistance;
        float high = 10.0f;

        // Expand until the whole level fits.
        for (int i = 0; i < 20; i++)
        {
            camTransform.position = focusPosition - camTransform.forward * high;

            if (CameraFitsPoints(cam, boundsCorners))
                break;

            high *= 2.0f;
        }

        // Binary search the closest distance that still fits the whole level.
        for (int i = 0; i < 32; i++)
        {
            float mid = (low + high) * 0.5f;
            camTransform.position = focusPosition - camTransform.forward * mid;

            if (CameraFitsPoints(cam, boundsCorners))
                high = mid;
            else
                low = mid;
        }

        float finalDistance = Mathf.Max(minPerspectiveDistance, high * menuCameraFraming);
        camTransform.position = focusPosition - camTransform.forward * finalDistance;
    }

    bool CameraFitsPoints(Camera cam, Vector3[] points)
    {
        foreach (Vector3 point in points)
        {
            Vector3 viewportPoint = cam.WorldToViewportPoint(point);

            if (viewportPoint.z <= 0.0f)
                return false;

            if (viewportPoint.x < 0.0f || viewportPoint.x > 1.0f ||
                viewportPoint.y < 0.0f || viewportPoint.y > 1.0f)
            {
                return false;
            }
        }

        return true;
    }

    Vector3[] GetBoxColliderCorners(BoxCollider box)
    {
        Vector3 center = box.center;
        Vector3 halfSize = box.size * 0.5f;

        Vector3[] corners = new Vector3[8];

        int index = 0;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = center + Vector3.Scale(
                        halfSize,
                        new Vector3(x, y, z)
                    );

                    corners[index++] = box.transform.TransformPoint(localCorner);
                }
            }
        }

        return corners;
    }

    private void LateUpdate()
    {
        ClearUISelection();
        UpdateCameraSway();
    }

    void ClearUISelection()
    {
        if (!EventSystem.current) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void UpdateCameraSway()
    {
        if (!animateMenuCamera) return;
        if (!menuCameraTransform) return;

        float t = Time.unscaledTime * cameraSwaySpeed;

        float x = Mathf.Sin(t * 1.00f) * cameraSwayPositionAmplitude;
        float y = Mathf.Sin(t * 0.73f + 1.7f) * cameraSwayPositionAmplitude * 0.5f;
        float z = Mathf.Sin(t * 0.41f + 3.2f) * cameraSwayPositionAmplitude * 0.25f;

        Vector3 localOffset =
            menuCameraBaseRotation * new Vector3(x, y, z);

        float yaw = Mathf.Sin(t * 0.67f + 0.5f) * cameraSwayRotationAmplitude;
        float pitch = Mathf.Sin(t * 0.53f + 2.0f) * cameraSwayRotationAmplitude * 0.6f;
        float roll = Mathf.Sin(t * 0.37f + 4.0f) * cameraSwayRotationAmplitude * 0.35f;

        Quaternion rotationOffset = Quaternion.Euler(pitch, yaw, roll);

        menuCameraTransform.position = menuCameraBasePosition + localOffset;
        menuCameraTransform.rotation = menuCameraBaseRotation * rotationOffset;
    }

    public void StartGame()
    {
        GameManager.instance.StartGame();
    }

    public void ContinueGame()
    {
        GameManager.instance.ContinueGame();
    }

    public void ShowCredits()
    {
        mainMenuCanvas.FadeOut(0.25f);
        var cg = creditsScroll.GetComponent<CanvasGroup>();
        cg.FadeIn(0.25f);
        creditsScroll.Reset();
        creditsScroll.onEndScroll += CreditsScroll_onEndScroll; ;
    }

    private void CreditsScroll_onEndScroll()
    {
        var cg = creditsScroll.GetComponent<CanvasGroup>();
        cg.FadeOut(0.25f);
        mainMenuCanvas.FadeIn(0.25f);
    }

    public void ShowOptions()
    {
        mainMenuCanvas.FadeOut(0.25f);
        optionsCanvas.FadeIn(0.25f);
    }

    public void CloseOptions()
    {
        mainMenuCanvas.FadeIn(0.25f);
        optionsCanvas.FadeOut(0.25f);
    }

    public void QuitApplication()
    {
        SoundManager.PlayMusic(null, crossfadeTime : 0.5f);
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }
}