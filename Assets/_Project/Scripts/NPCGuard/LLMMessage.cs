using System;

[Serializable]
public class LLMMessage
{
    public string role;
    public string content;

    public LLMMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}
