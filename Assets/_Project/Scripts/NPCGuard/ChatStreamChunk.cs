using System;

[Serializable]
internal class ChatStreamChunk
{
    public Choice[] choices;

    [Serializable]
    public class Choice
    {
        public LLMMessage delta;
        public string finish_reason;
    }
}
