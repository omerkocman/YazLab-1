using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        Instance = this;
    }
    
    public void Show(string key, string action)
    {
        promptText.text = $"<b><color=orange>[{key}]</color></b> <color=white>{action}</color>";
        _canvas.enabled = true;
    }

    public void Hide()
    {
        _canvas.enabled = false;
    }
}
