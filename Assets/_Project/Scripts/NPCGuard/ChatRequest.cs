using System;

[Serializable]
internal class ChatRequest
{
    public string model;
    public LLMMessage[] messages;
    public bool stream = true;
    public int max_tokens = 256;
    public GuardResponseFormat response_format = new GuardResponseFormat();
}
