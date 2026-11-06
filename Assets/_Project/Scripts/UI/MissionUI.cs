using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class MissionUI : MonoBehaviour
{
    [SerializeField] private GameFlowManager gameFlow;

    private PanelRenderer panel;
    private NPCDialogueUI dialogueUI;
    private VisualElement playOverlay;
    private VisualElement playMenu;
    private VisualElement settingsMenu;
    private VisualElement missionBanner;
    private Label objective;
    private Label menuTitle;
    private Label settingsStatus;
    private Button play;
    private Button settings;
    private Button back;
    private Button save;
    private Button quit;
    private Button endQuit;
    private TextField server;
    private TextField model;
    private IntegerField timeout;

    private void OnEnable()
    {
        panel = GetComponent<PanelRenderer>();
        dialogueUI = GetComponent<NPCDialogueUI>();
        panel.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panel.UnregisterUIReloadCallback(OnUIReload);
        UnbindButtons();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        UnbindButtons();

        playOverlay = root.Q("play-overlay");
        playMenu = root.Q("play-menu");
        settingsMenu = root.Q("settings-menu");
        missionBanner = root.Q("mission-banner");
        objective = root.Q<Label>("mission-objective");
        menuTitle = root.Q<Label>("menu-title");
        settingsStatus = root.Q<Label>("settings-status");

        play = root.Q<Button>("play-button");
        settings = root.Q<Button>("settings-button");
        back = root.Q<Button>("settings-back");
        save = root.Q<Button>("settings-save");
        quit = root.Q<Button>("quit-button");
        endQuit = root.Q<Button>("death-quit-button");
        server = root.Q<TextField>("settings-server");
        model = root.Q<TextField>("settings-model");
        timeout = root.Q<IntegerField>("settings-timeout");

        if (play != null)
            play.clicked += StartMission;
        if (quit != null) quit.clicked += Quit;
        if (endQuit != null) endQuit.clicked += Quit;

        if (settings != null && back != null && save != null && playMenu != null &&
            settingsMenu != null && server != null && model != null && timeout != null && settingsStatus != null)
        {
            settings.clicked += OpenSettings;
            back.clicked += ShowPlayMenu;
            save.clicked += SaveSettings;
        }
    }

    private void UnbindButtons()
    {
        if (play != null) play.clicked -= StartMission;
        if (settings != null) settings.clicked -= OpenSettings;
        if (back != null) back.clicked -= ShowPlayMenu;
        if (save != null) save.clicked -= SaveSettings;
        if (quit != null) quit.clicked -= Quit;
        if (endQuit != null) endQuit.clicked -= Quit;
    }

    private void Update()
    {
        if (gameFlow == null || playOverlay == null || missionBanner == null || objective == null)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleEscape();

        playOverlay.style.display = !gameFlow.IsGameEnded && (!gameFlow.HasStarted || gameFlow.IsPaused)
            ? DisplayStyle.Flex : DisplayStyle.None;
        missionBanner.style.display = gameFlow.HasStarted && !gameFlow.IsGameEnded && !gameFlow.IsPaused
            ? DisplayStyle.Flex : DisplayStyle.None;
        if (play != null) play.text = gameFlow.IsPaused ? "Resume" : "Play";
        if (menuTitle != null) menuTitle.text = gameFlow.IsPaused ? "MISSION PAUSED" : "BLACKLINE PROTOCOL";

        string currentObjective = gameFlow.CurrentObjective;
        if (objective.text != currentObjective)
            objective.text = currentObjective;
    }

    private void StartMission()
    {
        if (gameFlow == null)
            return;

        if (gameFlow.IsPaused)
            gameFlow.TogglePause();
        else
            gameFlow.StartMission();
    }

    private void HandleEscape()
    {
        if (dialogueUI != null && dialogueUI.TryCloseConversation())
            return;

        if (settingsMenu != null && settingsMenu.resolvedStyle.display == DisplayStyle.Flex)
        {
            ShowPlayMenu();
            return;
        }

        gameFlow.TogglePause();
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenSettings()
    {
        playMenu.style.display = DisplayStyle.None;
        settingsMenu.style.display = DisplayStyle.Flex;

        LLMService service = dialogueUI != null ? dialogueUI.LLMService : null;
        save.SetEnabled(service != null);
        settingsStatus.text = service == null ? "The guard's LLM service is not assigned." : "";

        if (service == null)
            return;

        server.SetValueWithoutNotify(service.ServerUrl);
        model.SetValueWithoutNotify(service.ModelName);
        timeout.SetValueWithoutNotify(service.TimeoutSeconds);
    }

    private void SaveSettings()
    {
        LLMService service = dialogueUI != null ? dialogueUI.LLMService : null;
        if (service == null)
            return;

        if (service.TrySaveSettings(server.value, model.value, timeout.value, out string error))
            ShowPlayMenu();
        else
            settingsStatus.text = error;
    }

    private void ShowPlayMenu()
    {
        settingsMenu.style.display = DisplayStyle.None;
        playMenu.style.display = DisplayStyle.Flex;
    }
}
