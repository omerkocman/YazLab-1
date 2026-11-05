using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class NPCDialogueUI : MonoBehaviour
{
    [SerializeField] private NPCDialogueController dialogue;

    public LLMService LLMService => dialogue != null ? dialogue.LLMService : null;

    private PanelRenderer panel;
    private VisualElement overlay;
    private ScrollView chat;
    private Label title;
    private Label connection;
    private Label status;
    private TextField input;
    private Button send;
    private Button retry;
    private Button close;

    private readonly List<MessageBubble> bubbles = new List<MessageBubble>();
    private MessageBubble preview;
    private string errorMessage = "";
    private bool hasReply;

    private void OnEnable()
    {
        panel = GetComponent<PanelRenderer>();
        panel.RegisterUIReloadCallback(OnUIReload);
        if (dialogue == null)
            return;

        dialogue.ConversationOpened += Open;
        dialogue.ConversationClosed += Refresh;
        dialogue.PlayerMessageSent += AddPlayerMessage;
        dialogue.PreviewChanged += ShowPreview;
        dialogue.ResponseReceived += AddGuardMessage;
        dialogue.RequestFailed += ShowError;
        dialogue.WaitingChanged += Refresh;
    }

    private void OnDisable()
    {
        panel.UnregisterUIReloadCallback(OnUIReload);
        UnbindUI();
        if (dialogue == null)
            return;

        dialogue.ConversationOpened -= Open;
        dialogue.ConversationClosed -= Refresh;
        dialogue.PlayerMessageSent -= AddPlayerMessage;
        dialogue.PreviewChanged -= ShowPreview;
        dialogue.ResponseReceived -= AddGuardMessage;
        dialogue.RequestFailed -= ShowError;
        dialogue.WaitingChanged -= Refresh;
        dialogue.EndConversation();
    }

    private void Update()
    {
        if (dialogue != null && dialogue.IsConversationActive &&
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        UnbindUI();
        overlay = root.Q("dialogue-overlay");
        chat = root.Q<ScrollView>("dialogue-chat");
        title = root.Q<Label>("dialogue-title");
        connection = root.Q<Label>("dialogue-connection");
        status = root.Q<Label>("dialogue-status");
        input = root.Q<TextField>("dialogue-input");
        send = root.Q<Button>("dialogue-send");
        retry = root.Q<Button>("dialogue-retry");
        close = root.Q<Button>("dialogue-close");

        if (send != null) send.clicked += Send;
        if (retry != null) retry.clicked += Retry;
        if (close != null) close.clicked += Close;
        input?.RegisterCallback<KeyDownEvent>(OnInputKey, TrickleDown.TrickleDown);

        if (chat != null)
        {
            chat.contentContainer.RegisterCallback<GeometryChangedEvent>(OnChatLayout);
            foreach (MessageBubble bubble in bubbles)
                AttachBubble(bubble);

            if (preview != null)
                AttachBubble(preview);
        }

        Refresh();
    }

    private void UnbindUI()
    {
        if (send != null) send.clicked -= Send;
        if (retry != null) retry.clicked -= Retry;
        if (close != null) close.clicked -= Close;
        input?.UnregisterCallback<KeyDownEvent>(OnInputKey, TrickleDown.TrickleDown);
        chat?.contentContainer.UnregisterCallback<GeometryChangedEvent>(OnChatLayout);
    }

    private void Open()
    {
        chat?.Clear();
        bubbles.Clear();
        preview = null;
        errorMessage = "";
        hasReply = false;

        if (input != null)
            input.value = "";

        Refresh();
        close?.Focus();
    }

    private void AddPlayerMessage(string message, DateTime time)
    {
        var bubble = new MessageBubble("You", true);
        bubble.SetContent(message, time.ToString("HH:mm:ss"));
        bubbles.Add(bubble);
        AttachBubble(bubble);
    }

    private void ShowPreview(string text)
    {
        if (preview == null)
        {
            preview = new MessageBubble(dialogue.GuardName, false);
            AttachBubble(preview);
        }

        preview.SetContent(text, "Receiving...");
        hasReply = true;
        Refresh();
    }

    private void AddGuardMessage(NPCGuardResponse reply, DateTime time, double seconds)
    {
        var bubble = preview;
        if (bubble == null)
        {
            bubble = new MessageBubble(dialogue.GuardName, false);
            AttachBubble(bubble);
        }

        string elapsed = seconds.ToString("0.0", CultureInfo.InvariantCulture);
        bubble.SetContent(reply.response, $"{time:HH:mm:ss}  ·  {elapsed} s");
        bubbles.Add(bubble);
        preview = null;
        hasReply = true;
        Refresh();
    }

    private void AttachBubble(MessageBubble bubble)
    {
        if (chat == null)
            return;

        bubble.RemoveFromHierarchy();
        chat.Add(bubble);
    }

    private void OnChatLayout(GeometryChangedEvent change)
    {
        chat.verticalScroller.value = chat.verticalScroller.highValue;
    }

    private void ShowError(string message)
    {
        preview?.RemoveFromHierarchy();
        preview = null;
        errorMessage = message;
        Refresh();
    }

    private void Refresh()
    {
        if (overlay == null || dialogue == null || input == null || title == null ||
            status == null || connection == null || send == null || retry == null)
            return;

        overlay.style.display = dialogue.IsConversationActive ? DisplayStyle.Flex : DisplayStyle.None;
        if (!dialogue.IsConversationActive)
            return;

        title.text = dialogue.GuardName;
        bool failed = !string.IsNullOrEmpty(errorMessage);
        connection.text = failed ? "Request failed" : hasReply ? "Connected" : "Connecting...";
        connection.EnableInClassList("connection-online", hasReply && !failed);
        connection.EnableInClassList("connection-error", failed);

        if (dialogue.IsWaitingForResponse)
            status.text = preview != null ? "Receiving reply..." : "Waiting for reply...";
        else if (dialogue.IsDoorAccessGranted)
            status.text = "Access granted. The door is opening.";
        else
            status.text = errorMessage;

        retry.style.display = failed && !dialogue.IsWaitingForResponse ? DisplayStyle.Flex : DisplayStyle.None;

        bool wasEnabled = input.enabledSelf;
        input.SetEnabled(dialogue.CanSendMessage);
        send.SetEnabled(dialogue.CanSendMessage);
        if (!wasEnabled && dialogue.CanSendMessage)
            input.Focus();
    }

    private void OnInputKey(KeyDownEvent key)
    {
        if (key.keyCode != KeyCode.Return && key.keyCode != KeyCode.KeypadEnter)
            return;

        key.StopPropagation();
        Send();
    }

    private async void Send()
    {
        if (dialogue == null || !dialogue.CanSendMessage || string.IsNullOrWhiteSpace(input.value))
            return;

        string message = input.value;
        input.value = "";
        await dialogue.SendMessageAsync(message);
    }

    private async void Retry()
    {
        if (dialogue == null)
            return;

        errorMessage = "";
        hasReply = false;
        await dialogue.RetryAsync();
    }

    private void Close()
    {
        dialogue?.EndConversation();
    }

    private sealed class MessageBubble : VisualElement
    {
        private readonly Label body = new Label { enableRichText = false };
        private readonly Label details = new Label { enableRichText = false };

        public MessageBubble(string sender, bool fromPlayer)
        {
            AddToClassList("chat-bubble");
            if (fromPlayer)
                AddToClassList("chat-bubble-player");

            var senderLabel = new Label(sender) { enableRichText = false };
            senderLabel.AddToClassList("chat-sender");
            body.AddToClassList("chat-body");
            details.AddToClassList("chat-details");

            Add(senderLabel);
            Add(body);
            Add(details);
        }

        public void SetContent(string message, string metadata)
        {
            body.text = message;
            details.text = metadata;
        }
    }
}
