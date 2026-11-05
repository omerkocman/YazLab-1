using System;
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
    [SerializeField] private DoorInteractable guardedDoor;
    [SerializeField] private MonoBehaviour[] controlsToDisable;
    [SerializeField] private bool showPlayMenu = true;

    public bool HasStarted { get; private set; }
    public bool IsGameEnded { get; private set; }
    public GameEndReason EndReason { get; private set; }
    public string CurrentObjective => gasVent != null && gasVent.IsActive
        ? "Activate the maintenance robot and wait for it to shut off the gas."
        : guardedDoor != null && guardedDoor.IsOpen
            ? "Shut off the main power switch."
            : "Talk to the guard and request access.";

    public event Action<GameEndReason> GameEnded;
    private bool dialogueOpen;

    private void Start()
    {
        if (!showPlayMenu)
            StartMission();
        else
            SetPlayerInput(false, true);
    }

    private void OnEnable()
    {
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

        HasStarted = true;
        SetPlayerInput(true, false);
    }

    public bool TryBeginDialogue()
    {
        if (!HasStarted || IsGameEnded || dialogueOpen || (gasVent != null && gasVent.IsActive))
            return false;

        dialogueOpen = true;
        SetPlayerInput(false, false);
        return true;
    }

    public void EndDialogue()
    {
        dialogueOpen = false;
        if (HasStarted && !IsGameEnded)
            SetPlayerInput(true, false);
    }

    public void CompleteMission()
    {
        if (HasStarted && !dialogueOpen && gasVent != null && !gasVent.IsActive &&
            guardedDoor != null && guardedDoor.IsOpen)
        {
            EndGame(GameEndReason.MissionCompleted);
        }
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
