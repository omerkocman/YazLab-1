using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OllamaMessage
{
    public string role;
    public string content;

    public OllamaMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}

[Serializable]
public class NPCGuardResponse
{
    public string response;
    public bool openDoor;
}

public class OllamaService : MonoBehaviour
{
    [Header("Ollama Settings")]
    [SerializeField] private string serverURL = "http://localhost:11434";
    [SerializeField] private string modelName = "llama3.2";
    [SerializeField, Min(1)] private int timeoutSeconds = 30;

    [Serializable]
    private class  FieldSchema
    {
        public string type;
        public FieldSchema(string type)
        {
            this.type = type;
        }
    }

    [Serializable]
    private class GuardProperties
    {
        public FieldSchema response = new FieldSchema("string");
        public FieldSchema openDoor = new FieldSchema("boolean");
    }

    [Serializable]
    private class GuardSchema
    {
        public string type = "object";
        public GuardProperties properties = new GuardProperties();
        public string[] required = new string[] { "response", "openDoor" };
        public bool additionalProperties = false;
    }

    [Serializable]
    private class JsonSchemaOptions
    {
        public string name = "npc_guard_response";
        public bool strict = true;
        public GuardSchema schema = new GuardSchema();
    }

    [Serializable]
    private class ResponseFormat
    {
        public string type = "json_schema";
        public JsonSchemaOptions json_schema = new JsonSchemaOptions();
    }

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public OllamaMessage[] messages;
        public bool stream = false;
        public ResponseFormat response_format = new ResponseFormat();
    }

    [Serializable]
    private class ChatChoice
    {
        public OllamaMessage message;
    }

    [Serializable]
    private class ChatResponse
    {
        public ChatChoice[] choices;
    }

    public async Awaitable<string> ChatAsync(List<OllamaMessage> messages)
    {
        if(messages == null || messages.Count == 0)
        {
            throw new ArgumentException("Messages cannot be null or empty.");
        }

        ChatRequest requestData = new ChatRequest
        {
            model = modelName,
            messages = messages.ToArray(),
        };

        string json = JsonUtility.ToJson(requestData);
        string url = serverURL.TrimEnd('/') + "/chat/completions";

        using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

        byte[] body = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        request.timeout = timeoutSeconds;

        Debug.Log($"[OLLAMA SERVICE] Sending request to {url} with model {modelName} and messages: {json}");

        await Awaitable.FromAsyncOperation(request.SendWebRequest(), destroyCancellationToken);

        if(request.result != UnityWebRequest.Result.Success)
        {
            throw new Exception($"Request failed. HTTP {request.responseCode}: {request.error} | {request.downloadHandler.text}");
        }

        ChatResponse result = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);

        if (result == null || result.choices == null || result.choices.Length == 0 || result.choices[0] == null || result.choices[0].message == null || string.IsNullOrWhiteSpace(result.choices[0].message.content))
        {
            throw new Exception($"Invalid response from LM Studio: {request.downloadHandler.text}");

        }

            return result.choices[0].message.content;
    }

    [ContextMenu("Test/Ollama Chat")]
    private async void TestOllamaChat()
    {
        List<OllamaMessage> messages = new List<OllamaMessage>
        {
            new OllamaMessage(
                "system",
                "You are a security guard protecting a locked door in an industrial facility. " +
                "You are cautious and professional. " +
                "Do not open the door without a convincing reason or authorization. " +
                "Respond in Turkish. " +
                "Return only JSON with response (string) and openDoor (boolean)."
            ),

            new OllamaMessage(
                "user",
                "Merhaba, bu kapıdan geçmem gerekiyor. Benimle konuşabilir misiniz?"
            )
        };

        try
        {
            string json = await ChatAsync(messages);

            NPCGuardResponse reply = JsonUtility.FromJson<NPCGuardResponse>(json);

            if(reply == null || string.IsNullOrWhiteSpace(reply.response))
            {
                throw new Exception($"Invalid response structure: {json}");
            }

            Debug.Log($"[NPC] Response: {reply.response}");
            Debug.Log($"[NPC] Open Door: {reply.openDoor}");
            Debug.Log($"[OLLAMA] Raw JSON: {json}");
        }

        catch(OperationCanceledException)
        {
            Debug.LogWarning("[OLLAMA SERVICE] Request was canceled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[OLLAMA SERVICE] Error: {ex.Message}");
        }
    }
}
