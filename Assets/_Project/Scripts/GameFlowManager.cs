using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameEndReason
{
    PlayerDied,
    MissionCompleted,
    MissionFailed
}

public class GameFlowManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PoisonGasVent gasVent;
    [SerializeField] private Switch[] gasSwitchCandidates;
    [SerializeField] private DoorInteractable guardedDoor;
    [SerializeField] private Switch finalSwitch;
    [SerializeField] private MonoBehaviour[] controlsToDisable;
    [SerializeField] private bool showPlayMenu = true;

    public bool HasStarted { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameEnded { get; private set; }
    public GameEndReason EndReason { get; private set; }
    public string CurrentObjective => gasVent != null && gasVent.IsActive
        ? "Activate the maintenance robot and wait for it to shut off the gas."
        : guardedDoor != null && guardedDoor.IsOpen
            ? "Shut off the main power switch."
            : "Talk to the guard and request access.";

    public event Action<GameEndReason> GameEnded;
    private bool dialogueOpen;
    private bool finalSwitchWasOpen;
    private Switch activeGasSwitch;

    private void Start()
    {
        if (!showPlayMenu)
            StartMission();
        else
            SetPlayerInput(false, true);
    }

    private void OnEnable()
    {
        finalSwitchWasOpen = finalSwitch != null && finalSwitch.IsOpen;
        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Died -= OnPlayerDied;
    }

    public void StartMission()
    {
        if (HasStarted || IsGameEnded)
            return;

        SelectGasSwitch();
        HasStarted = true;
        SetPlayerInput(true, false);
    }

    public bool CanRobotTargetSwitch(Switch candidate)
    {
        return candidate != finalSwitch && (activeGasSwitch == null || candidate == activeGasSwitch);
    }

    private void SelectGasSwitch()
    {
        if (gasSwitchCandidates == null || gasSwitchCandidates.Length == 0)
            return;

        if (gasVent == null)
        {
            Debug.LogWarning("[GAME FLOW] Assign a Gas Vent to use Gas Switch Candidates.", this);
            return;
        }

        var candidates = new List<Switch>();
        foreach (Switch candidate in gasSwitchCandidates)
        {
            if (candidate != null && candidate != finalSwitch && !candidates.Contains(candidate))
                candidates.Add(candidate);
        }

        if (candidates.Count == 0)
            return;

        activeGasSwitch = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        foreach (Switch candidate in candidates)
        {
            if (candidate != activeGasSwitch)
                candidate.gameObject.SetActive(false);
        }

        activeGasSwitch.gameObject.SetActive(true);
        activeGasSwitch.ConfigureGasControl(gasVent);
    }

    public bool TryBeginDialogue()
    {
        if (!HasStarted || IsGameEnded || IsPaused || dialogueOpen || (gasVent != null && gasVent.IsActive))
            return false;

        dialogueOpen = true;
        SetPlayerInput(false, false);
        return true;
    }

    public void EndDialogue()
    {
        dialogueOpen = false;
        if (HasStarted && !IsGameEnded && !IsPaused)
            SetPlayerInput(true, false);
    }

    public void TogglePause()
    {
        if (!HasStarted || IsGameEnded || dialogueOpen)
            return;

        IsPaused = !IsPaused;
        SetPlayerInput(!IsPaused, IsPaused);
    }

    public void CompleteMission()
    {
        if (HasStarted && finalSwitch != null && !finalSwitch.IsOpen && !finalSwitch.IsMoving)
        {
            EndGame(GameEndReason.MissionCompleted);
        }
    }

    private void Update()
    {
        if (finalSwitch == null || IsGameEnded)
            return;

        // Switch updates IsOpen once its animation finishes.
        bool isOpen = finalSwitch.IsOpen;
        bool justTurnedOff = finalSwitchWasOpen && !isOpen;
        finalSwitchWasOpen = isOpen;
        if (justTurnedOff)
            CompleteMission();
    }

    private void OnPlayerDied()
    {
        EndGame(GameEndReason.PlayerDied);
    }

    public void EndGame(GameEndReason reason)
    {
        if (IsGameEnded)
            return;

        IsGameEnded = true;
        IsPaused = false;
        EndReason = reason;
        SetPlayerInput(false, true);
        GameEnded?.Invoke(reason);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetPlayerInput(bool enabled, bool pause)
    {
        if (controlsToDisable != null)
        {
            foreach (MonoBehaviour control in controlsToDisable)
            {
                if (control != null)
                    control.enabled = enabled;
            }
        }

        Time.timeScale = pause ? 0f : 1f;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }
}
