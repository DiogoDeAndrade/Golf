using TMPro;
using UnityEngine;

public class PopupText : MonoBehaviour
{
    TextMeshProUGUI text;
    CanvasGroup     canvasGroup;
    float           timer;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        
    }

    void Init(string s, float duration, float fadeTime, Color color, float popSize)
    {
        timer = duration;
    }
}
