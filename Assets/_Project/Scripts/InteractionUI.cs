using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;
    }
    
    public void Show(string key, string action)
    {
        promptText.text = $"<b><color=orange>[{key}]</color></b> <color=white>{action}</color>";
        promptText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        promptText.gameObject.SetActive(false);
    }
}
