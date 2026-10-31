using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractor interactor;

    [Header("Settings")]
    [SerializeField] private float feedbackDuration = 2f;


    [SerializeField] private PanelRenderer panelRenderer;
    private VisualElement interactionContainer;
    private Label interactionPrompt;
    private Label interactionFeedback;

    private Coroutine feedbackRoutine;



    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();

    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if (interactor != null)
        {
            interactor.PromptChanged += ShowPrompt;
            interactor.FeedbackRequested += ShowFeedback;
        }

    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if(interactor != null)
        {
            interactor.PromptChanged -= ShowPrompt;
            interactor.FeedbackRequested -= ShowFeedback;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        ShowPrompt("");
        HideFeedback();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if(feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        interactionContainer = root.Q<VisualElement>("interaction-container");

        interactionPrompt = root.Q<Label>("interaction-prompt");

        interactionFeedback = root.Q<Label>("interaction-feedback");

        if(interactionContainer == null || interactionPrompt == null || interactionFeedback == null)
        {
            Debug.LogError("[INTERACTION UI] UXML elements not found!", this);
            return;
        }

        ShowPrompt(interactor != null ? interactor.CurrentPrompt : "");

        HideFeedback();
    }

    public void ShowPrompt(string message)
    {
        if(interactionContainer == null || interactionPrompt == null)
        {
            return;
        }

        interactionPrompt.text = message;

        interactionContainer.style.display = string.IsNullOrEmpty(message) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void ShowFeedback(string message)
    {
        if(interactionFeedback == null)
        {
            return;
        }

        if(feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        interactionFeedback.text = message;

        interactionFeedback.style.display = DisplayStyle.Flex;

        feedbackRoutine = StartCoroutine(HideFeedbackAfterDelay());
    }

    public void HideFeedback()
    {
        if(interactionFeedback == null)
        {
            return;
        }

        interactionFeedback.style.display = DisplayStyle.None;
    }

    private IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);

        HideFeedback();
        feedbackRoutine = null;
    }
}
