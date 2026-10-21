using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement interactionCard;
    private Label keyLabel;
    private Label titleLabel;
    private Label descLabel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        interactionCard = root.Q<VisualElement>("interaction-card");
        keyLabel = root.Q<Label>("interaction-key");
        titleLabel = root.Q<Label>("interaction-title");
        descLabel = root.Q<Label>("interaction-desc");

        HidePrompt();
    }

    public void ShowPrompt(string actionName, string description, string keyText = "E")
    {
        if (interactionCard == null) return;

        keyLabel.text = keyText;
        titleLabel.text = actionName;
        descLabel.text = description;

        // Açıklama boşsa alanı gizleyerek kartın küçülmesini sağlar
        descLabel.style.display = string.IsNullOrEmpty(description) ? DisplayStyle.None : DisplayStyle.Flex;

        interactionCard.style.display = DisplayStyle.Flex;
    }

    public void HidePrompt()
    {
        if (interactionCard != null)
        {
            interactionCard.style.display = DisplayStyle.None;
        }
    }
}
