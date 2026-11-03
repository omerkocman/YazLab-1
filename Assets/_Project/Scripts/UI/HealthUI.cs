using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private PanelRenderer panelRenderer;

    private VisualElement healthFill;
    private Label healthValue;
    private Label healthStatus;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if(playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealth;
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= UpdateHealth;
        }

        healthFill = null;
        healthValue = null;
        healthStatus = null;
    }

    private void UpdateHealth(float current, float max)
    {
        if(healthFill == null || healthValue == null || healthStatus == null)
        {
            return;
        }

        float percentage = max > 0f ? Mathf.Clamp01(current / max) * 100f : 0f;

        healthFill.style.width = Length.Percent(percentage);

        healthValue.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)} HP";

        Color fillColor;
        string statusText;

        if (percentage <= 0f)
        {
            fillColor = new Color32(240, 107, 112, 255);
            statusText = "STATUS: DEAD";
        }

        else if (percentage <= 25f)
        {
            fillColor = new Color32(240, 107, 112, 255);
            statusText = "STATUS: CRITICAL";
        }
        else if (percentage <= 65f)
        {
            fillColor = new Color32(241, 184, 75, 255);
            statusText = "STATUS: INJURED";
        }
        else
        {
            fillColor = new Color32(79, 221, 156, 255);
            statusText = "STATUS: STABLE";
        }

        healthFill.style.backgroundColor = fillColor;
        healthStatus.style.color = fillColor;
        healthStatus.text = statusText;
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        healthFill = root.Q<VisualElement>("health-fill");
        healthValue = root.Q<Label>("health-value");
        healthStatus = root.Q<Label>("health-status");

        if(healthFill == null || healthValue == null || healthStatus == null)
        {
            Debug.LogWarning("[HEALTH UI] Missing UI elements in the HealthUI panel.");
            return;
        }

        if(playerHealth != null)
        {
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

    }
}
