using UnityEngine;

namespace UC
{
    public class DefaultTooltip : MonoBehaviour, ITooltip
    {
        [SerializeField, TextArea] private string tooltipDescription;
        [SerializeField] private int priority;

        public RectTransform GetTooltip(RectTransform parentTransform)
        {
            var textTooltip = Instantiate(GlobalsBase.textTooltip, parentTransform);
            textTooltip.SetText(tooltipDescription);

            return textTooltip.GetComponent<RectTransform>();
        }

        public int GetOrder()
        {
            return priority;
        }
    }
}
