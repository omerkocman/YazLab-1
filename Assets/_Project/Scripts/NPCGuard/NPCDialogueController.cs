using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class NPCDialogueController : MonoBehaviour
{
    [SerializeField] private NPCGuardInteractable npcGuard;
    [SerializeField] private LLMService llmService;
    [SerializeField] private TextAsset prompt;
    [SerializeField] private GameFlowManager gameFlow;
    [SerializeField] private PoisonGasVent gasVent;
    [SerializeField] private DoorInteractable guardedDoor;

    public LLMService LLMService => llmService;
    public string GuardName => npcGuard != null ? npcGuard.GuardName : "NPCGuard";
    public bool IsConversationActive { get; private set; }
    public bool IsWaitingForResponse { get; private set; }
    public bool IsDoorAccessGranted { get; private set; }
    public bool CanSendMessage => IsConversationActive && !IsWaitingForResponse &&
        !IsDoorAccessGranted && hasGreeting && !canRetry;

    public event Action ConversationOpened;
    public event Action ConversationClosed;
    public event Action<string, DateTime> PlayerMessageSent;
    public event Action<string> PreviewChanged;
    public event Action<NPCGuardResponse, DateTime, double> ResponseReceived;
    public event Action<string> RequestFailed;
    public event Action WaitingChanged;

    private readonly List<LLMMessage> messages = new List<LLMMessage>();
    private CancellationTokenSource conversation;
    private bool hasGreeting;
    private bool canRetry;
    private string pendingMessage;

    private void OnEnable()
    {
        if (npcGuard != null)
            npcGuard.ConversationRequested += StartConversation;
        if (gameFlow != null)
            gameFlow.GameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        if (npcGuard != null)
            npcGuard.ConversationRequested -= StartConversation;
        if (gameFlow != null)
            gameFlow.GameEnded -= OnGameEnded;
        EndConversation();
    }

    private void OnGameEnded(GameEndReason reason)
    {
        EndConversation();
    }

    private async void StartConversation(GameObject interactor)
    {
        if (IsConversationActive || IsDoorAccessGranted || gameFlow == null || !gameFlow.TryBeginDialogue())
            return;

        conversation = new CancellationTokenSource();
        IsConversationActive = true;
        hasGreeting = false;
        canRetry = false;
        pendingMessage = null;
        messages.Clear();

        string gasStatus = gasVent == null ? "bilinmiyor" : gasVent.IsActive ? "açık" : "kapalı";
        string instructions = prompt != null ? prompt.text : "";
        messages.Add(new LLMMessage("system",
            instructions + "\nMuhafızın görünen adı: " + GuardName + "\nGaz vanası: " + gasStatus));
        ConversationOpened?.Invoke();

        await RequestReplyAsync();
    }

    public async Awaitable SendMessageAsync(string text)
    {
        if (!CanSendMessage || string.IsNullOrWhiteSpace(text))
            return;

        pendingMessage = text.Trim();
        PlayerMessageSent?.Invoke(pendingMessage, DateTime.Now);
        await RequestReplyAsync();
    }

    public async Awaitable RetryAsync()
    {
        if (IsConversationActive && canRetry && !IsWaitingForResponse)
            await RequestReplyAsync();
    }

    private async Awaitable RequestReplyAsync()
    {
        var token = conversation.Token;
        bool isGreeting = !hasGreeting;
        var requestMessages = new List<LLMMessage>(messages)
        {
            new LLMMessage("user", isGreeting ? "[Sistem: Oyuncu geldi. Karşılama repliğini söyle.]" : pendingMessage)
        };

        var timer = Stopwatch.StartNew();
        DateTime replyTime = default;
        IsWaitingForResponse = true;
        canRetry = false;
        WaitingChanged?.Invoke();

        try
        {
            if (llmService == null || prompt == null || string.IsNullOrWhiteSpace(prompt.text))
                throw new InvalidOperationException("LLM Service or NPCGuard Prompt is not assigned.");

            NPCGuardResponse reply = await llmService.ChatAsync(requestMessages, preview =>
            {
                if (replyTime == default)
                    replyTime = DateTime.Now;
                PreviewChanged?.Invoke(preview);
            }, token);

            token.ThrowIfCancellationRequested();
            timer.Stop();
            bool accessGranted = !isGreeting && reply.openDoor;
            if (accessGranted && (gasVent == null || gasVent.IsActive || guardedDoor == null ||
                !guardedDoor.UnlockAndOpen()))
            {
                throw new InvalidOperationException("Access could not be applied. Check the gas vent and guarded door assignments.");
            }

            hasGreeting = true;
            pendingMessage = null;
            messages.Add(requestMessages[requestMessages.Count - 1]);
            reply.openDoor = accessGranted;
            messages.Add(new LLMMessage("assistant", JsonUtility.ToJson(reply)));
            IsDoorAccessGranted = accessGranted;

            ResponseReceived?.Invoke(reply, replyTime == default ? DateTime.Now : replyTime, timer.Elapsed.TotalSeconds);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Closing this panel also cancels the request.
        }
        catch (Exception error)
        {
            if (!token.IsCancellationRequested)
            {
                canRetry = true;
                Debug.LogWarning($"[NPCGuard] {error.Message}", this);
                RequestFailed?.Invoke(error is FormatException
                    ? "The guard sent an invalid response. Try again."
                    : "The request failed. Check the local model settings and try again.");
            }
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsWaitingForResponse = false;
                WaitingChanged?.Invoke();
            }
        }
    }

    public void EndConversation()
    {
        if (!IsConversationActive)
            return;

        IsConversationActive = false;
        IsWaitingForResponse = false;
        conversation.Cancel();
        conversation.Dispose();
        conversation = null;
        gameFlow.EndDialogue();
        ConversationClosed?.Invoke();
    }
}
