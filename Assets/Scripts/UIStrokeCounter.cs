using TMPro;
using UnityEngine;

public class UIStrokeCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        LevelManager.instance.onStrokeTaken += UpdateCounter;

        UpdateCounter(0);
    }

    void UpdateCounter(int strokeCount)
    {
        text.text = $"x{strokeCount}";
    }
}
