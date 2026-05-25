using TMPro;
using UnityEngine;
using System.Collections;
using UC;

public class UIParDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    text;
    
    IEnumerator Start()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
        while (true)
        {
            Map map = FindFirstObjectByType<Map>();
            if (map)
            {
                UpdateCounter(map.par);
                canvasGroup.FadeIn(0.25f);
                break;
            }
            else
            {
                yield return null;
            }
        }
    }

    void UpdateCounter(int strokeCount)
    {
        text.text = $"x{strokeCount}";
    }
}
