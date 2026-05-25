using TMPro;
using UnityEngine;

public class UIParDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {       
        UpdateCounter(LevelManager.instance.par);
    }

    void UpdateCounter(int strokeCount)
    {
        text.text = $"x{strokeCount}";
    }
}
