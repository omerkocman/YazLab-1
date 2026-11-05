using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class NPCGuardResponse
{
    public string response;
    public bool openDoor;

    private const string TextPart = @"""response""\s*:\s*""";
    private const string JsonCharacter = @"(?:[^""\\\x00-\x1F]|\\(?:[""\\/bfnrt]|u[0-9a-fA-F]{4}))";
    private const string TextField = TextPart + JsonCharacter + @"*""";
    private const string DoorField = @"""openDoor""\s*:\s*(?:true|false)";
    private static readonly Regex CompleteJson = new Regex(
        @"\A\s*\{\s*(?:" + TextField + @"\s*,\s*" + DoorField + "|" +
        DoorField + @"\s*,\s*" + TextField + @")\s*\}\s*\z");
    private static readonly Regex PartialText = new Regex(
        @"\A\s*\{\s*(?:" + DoorField + @"\s*,\s*)?" + TextPart + "(" + JsonCharacter + "*)");

    public static NPCGuardResponse Parse(string json)
    {
        if (!CompleteJson.IsMatch(json))
            throw new FormatException("The model response does not match the expected JSON format.");

        var reply = JsonUtility.FromJson<NPCGuardResponse>(json);
        if (string.IsNullOrWhiteSpace(reply.response))
            throw new FormatException("The model returned an empty reply.");

        return reply;
    }

    // Show only complete characters from the response string while JSON is still arriving.
    public static string ReadPreview(string json)
    {
        Match match = PartialText.Match(json);
        if (!match.Success)
            return "";

        string escapedText = match.Groups[1].Value;
        string text = JsonUtility.FromJson<NPCGuardResponse>(
            "{\"response\":\"" + escapedText + "\"}").response;
        if (!string.IsNullOrEmpty(text) && char.IsHighSurrogate(text[text.Length - 1]))
            return text.Substring(0, text.Length - 1);

        return text;
    }
}
