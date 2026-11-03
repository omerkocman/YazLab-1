using System;
using System.Runtime.CompilerServices;
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
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Player Controls")]
    [SerializeField] private MonoBehaviour[] controlsToDisable;

    public bool IsGameEnded { get; private set; }
    public GameEndReason EndReason { get; private set; }

    public event Action<GameEndReason> GameEnded;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if(playerHealth != null)
        {
            playerHealth.Died += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        if(playerHealth != null)
        {
            playerHealth.Died -= OnPlayerDied;
        }
    }

    private void OnPlayerDied()
    {
        EndGame(GameEndReason.PlayerDied);
    }

    public void EndGame(GameEndReason reason)
    {
        if(IsGameEnded)
        {
            return;
        }

        IsGameEnded = true;
        EndReason = reason;

        foreach(MonoBehaviour control in controlsToDisable)
        {
            if(control != null)
            {
                control.enabled = false;
            }

        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        GameEnded?.Invoke(reason);

        Debug.Log($"[GAME FLOW] Game ended due to: {reason}");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    [ContextMenu("Test/Mission Completed")]
    private void TestMissionCompleted()
    {
        EndGame(GameEndReason.MissionCompleted);
    }

    [ContextMenu("Test/Mission Failed")]
    private void TestMissionFailed()
    {
        EndGame(GameEndReason.MissionFailed);
    }

    [ContextMenu("Test/Restart")]
    private void TestRestart()
    {
        RestartGame();
    }
}
