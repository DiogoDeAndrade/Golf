using UC;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndGame : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))]
    private UC.InputControl continueControl;
    [SerializeField] private CanvasGroup theEndCanvasGroup;

    private void Start()
    {
        theEndCanvasGroup.alpha = 0;
        theEndCanvasGroup.interactable = false;
        theEndCanvasGroup.blocksRaycasts = false;

        continueControl.playerInput = playerInput;
    }
    private void Update()
    {
        if (continueControl.IsPressed())
        {
            theEndCanvasGroup.FadeIn(0.5f);
            theEndCanvasGroup.interactable = true;
            theEndCanvasGroup.blocksRaycasts = true;
        }
    }

    public void BackToMainMenu()
    {
        GameManager.instance.MainMenu();
    }
}
