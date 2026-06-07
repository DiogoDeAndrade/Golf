using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SecondaryFillToHandle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image secondaryFillImage;
    [SerializeField] private RectTransform handle;

    [Header("Options")]
    [SerializeField] private bool updateEveryFrame = true;
    [SerializeField] private bool invert;

    private RectTransform fillRect;

    private readonly Vector3[] fillCorners = new Vector3[4];

    private void OnValidate()
    {
        UpdateFillAmount();
    }

    private void LateUpdate()
    {
        if (updateEveryFrame)
            UpdateFillAmount();
    }

    public void UpdateFillAmount()
    {
        if (secondaryFillImage == null || handle == null)
            return;

        fillRect = secondaryFillImage.rectTransform;

        if (fillRect == null)
            return;

        // RectTransform.GetWorldCorners order:
        // 0 = bottom-left
        // 1 = top-left
        // 2 = top-right
        // 3 = bottom-right
        fillRect.GetWorldCorners(fillCorners);

        Vector3 barLeft = (fillCorners[0] + fillCorners[1]) * 0.5f;
        Vector3 barRight = (fillCorners[2] + fillCorners[3]) * 0.5f;

        Vector3 barVector = barRight - barLeft;
        float barWidth = barVector.magnitude;

        if (barWidth <= 0.0001f)
            return;

        Vector3 barDirection = barVector / barWidth;

        // Center of the handle in world space
        Vector3 handleCenter = handle.TransformPoint(handle.rect.center);

        // Distance from beginning of bar to handle center, projected onto the bar axis
        float distanceFromStart = Vector3.Dot(handleCenter - barLeft, barDirection);

        float amount = distanceFromStart / barWidth;
        amount = Mathf.Clamp01(amount);

        if (invert)
            amount = 1.0f - amount;

        secondaryFillImage.fillAmount = amount;
    }
}
