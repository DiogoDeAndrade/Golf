using NaughtyAttributes;
using UC;
using UnityEditor;
using UnityEngine;

public class Map : MonoBehaviour
{
    [field: SerializeField] public int par { get; private set; } = 3;
    [field: SerializeField] public bool healthDisplay { get; private set; } = false;
    [field: SerializeField, ReadOnly] public string levelGUID { get; private set; }

    [SerializeField] private Hypertag playerTag;

    TacticalCameraController mainCamera;

    private void Start()
    {
        var boxCollider = GetComponent<BoxCollider>();
        mainCamera = FindFirstObjectByType<TacticalCameraController>();
        if ((mainCamera) && (boxCollider))
        {
            mainCamera.targetBoundsCollider = boxCollider;
        }
    }

    [Button("Generate GUID")]
    protected void GenerateGUID()
    {
        levelGUID = GUID.Generate().ToString();
    }
}
