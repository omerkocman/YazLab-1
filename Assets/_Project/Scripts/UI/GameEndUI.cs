using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class GameEndUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;

    private PanelRenderer panelRenderer;

    private VisualElement deathOverlay;
    private VisualElement deathPanel;

    private Label deathIndicator;
    private Label deathSubtitle;
    private Label deathTitle;
    private Label deathDescription;
    private Label deathFooter;

    private Button restartButton;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if(gameFlowManager != null)
        {
            gameFlowManager.GameEnded += ShowGameEnd;
        }
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if(gameFlowManager != null)
        {
            gameFlowManager.GameEnded -= ShowGameEnd;
        }

        if(restartButton != null)
        {
            restartButton.clicked -= RestartGame;
        }

        deathOverlay = null;
        deathPanel = null;
        restartButton = null;

    }
    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        if(restartButton != null)
        {
            restartButton.clicked -= RestartGame;
        }

        deathOverlay = root.Q<VisualElement>("death-overlay");
        deathPanel = root.Q<VisualElement>("death-panel");

        deathIndicator = root.Q<Label>("death-indicator");
        deathSubtitle = root.Q<Label>("death-subtitle");
        deathTitle = root.Q<Label>("death-title");
        deathDescription = root.Q<Label>("death-description");
        deathFooter = root.Q<Label>("death-footer");

        restartButton = root.Q<Button>("death-restart-button");

        if(deathOverlay == null || deathPanel == null || deathIndicator == null || deathSubtitle == null || deathTitle == null || deathDescription == null || deathFooter == null || restartButton == null)
        {
            Debug.LogError("[Game End UI] One or more UI elements are missing in the UI document.");
            return;
        }

        restartButton.clicked += RestartGame;

        if (gameFlowManager != null && gameFlowManager.IsGameEnded)
        {
            ShowGameEnd(gameFlowManager.EndReason);
        }

        else
        {
            deathOverlay.style.display = DisplayStyle.None;
        }
    }

    private void ShowGameEnd(GameEndReason reason)
    {
        if(deathOverlay == null || deathPanel == null)
        {
            return;
        }

        deathPanel.EnableInClassList("game-end-success", reason == GameEndReason.MissionCompleted);

        deathPanel.EnableInClassList("game-end-failure", reason == GameEndReason.MissionFailed);

        switch(reason)
        {
            case GameEndReason.PlayerDied:
                deathIndicator.text = "✕";
                deathSubtitle.text = "MISSION FAILED";
                deathTitle.text = "YOU DIED";
                deathDescription.text = "You have succumbed to your injuries. Better luck next time.";
                deathFooter.text = "Press 'Restart' to try again.";
                restartButton.text = "RESTART MISSION";
                
                break;

            case GameEndReason.MissionCompleted:
                deathIndicator.text = "✓";
                deathSubtitle.text = "MISSION SUCCESS";
                deathTitle.text = "CONGRATULATIONS!";
                deathDescription.text = "You have successfully completed the mission. Well done!";
                deathFooter.text = "Press 'Restart' to play again.";
                restartButton.text = "RESTART MISSION";

                break;

            case GameEndReason.MissionFailed:
                deathIndicator.text = "!";
                deathSubtitle.text = "MISSION FAILED";
                deathTitle.text = "MISSION FAILED";
                deathDescription.text = "You have failed the mission. Better luck next time.";
                deathFooter.text = "Press 'Restart' to try again.";
                restartButton.text = "RESTART MISSION";
                break;
        }

        deathOverlay.style.display = DisplayStyle.Flex;
    }

    private void RestartGame()
    {
        if(gameFlowManager != null)
        {
            gameFlowManager.RestartGame();
        }
    }
}
