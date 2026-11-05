using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal sealed class ChatStreamHandler : DownloadHandlerScript
{
    private readonly MemoryStream line = new MemoryStream();
    private readonly StringBuilder reply = new StringBuilder();

    public string Reply => reply.ToString();
    public bool IsComplete { get; private set; }
    public bool IsTruncated { get; private set; }
    public bool HasInvalidEvent { get; private set; }

    protected override bool ReceiveData(byte[] data, int length)
    {
        if (data == null || length == 0)
            return true;

        for (int index = 0; index < length; index++)
        {
            if (data[index] == 10)
                ReadLine();
            else
                line.WriteByte(data[index]);
        }
        return true;
    }

    protected override void CompleteContent()
    {
        if (line.Length > 0)
            ReadLine();
    }

    private void ReadLine()
    {
        string text = Encoding.UTF8.GetString(line.GetBuffer(), 0, (int)line.Length);
        line.SetLength(0);
        if (!text.StartsWith("data:", StringComparison.Ordinal))
            return;

        string payload = text.Substring(5).Trim();
        if (payload == "[DONE]")
        {
            IsComplete = true;
            return;
        }

        ChatStreamChunk chunk;
        try
        {
            chunk = JsonUtility.FromJson<ChatStreamChunk>(payload);
        }
        catch (ArgumentException)
        {
            HasInvalidEvent = true;
            return;
        }

        if (chunk?.choices == null)
        {
            HasInvalidEvent = true;
            return;
        }

        if (chunk.choices.Length == 0 || chunk.choices[0] == null)
            return;

        var choice = chunk.choices[0];
        reply.Append(choice.delta?.content);
        if (choice.finish_reason == "stop")
            IsComplete = true;
        else if (!string.IsNullOrEmpty(choice.finish_reason))
            IsTruncated = true;
    }
}
