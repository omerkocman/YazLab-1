using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class LLMService : MonoBehaviour
{
    [Header("Local LLM Settings")]
    [SerializeField] private string serverURL = "http://localhost:11434/v1";
    [SerializeField] private string modelName = "llama3.2";
    [SerializeField, Min(1)] private int timeoutSeconds = 30;

    private const string ServerKey = "LLM.ServerUrl";
    private const string ModelKey = "LLM.ModelName";
    private const string TimeoutKey = "LLM.TimeoutSeconds";

    public string ServerUrl => PlayerPrefs.GetString(ServerKey, serverURL);
    public string ModelName => PlayerPrefs.GetString(ModelKey, modelName);
    public int TimeoutSeconds => PlayerPrefs.GetInt(TimeoutKey, timeoutSeconds);

    public bool TrySaveSettings(string url, string model, int timeout, out string error)
    {
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri address) ||
            (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrEmpty(address.Host) || address.UserInfo.Length != 0 ||
            address.Query.Length != 0 || address.Fragment.Length != 0)
        {
            error = "Enter a valid server URL. Example: http://localhost:11434/v1";
            return false;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            error = "Enter the model name used by the server.";
            return false;
        }

        if (timeout < 1 || timeout > 300)
        {
            error = "Timeout must be between 1 and 300 seconds.";
            return false;
        }

        PlayerPrefs.SetString(ServerKey, address.AbsoluteUri.TrimEnd('/'));
        PlayerPrefs.SetString(ModelKey, model.Trim());
        PlayerPrefs.SetInt(TimeoutKey, timeout);
        PlayerPrefs.Save();

        error = "";
        return true;
    }

    public async Awaitable<NPCGuardResponse> ChatAsync(
        List<LLMMessage> messages, Action<string> showPreview, CancellationToken cancellationToken)
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, destroyCancellationToken);

        var data = new ChatRequest { model = ModelName, messages = messages.ToArray() };
        string baseUrl = ServerUrl.Trim().TrimEnd('/');
        if (!baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            baseUrl += "/v1";

        using var request = new UnityWebRequest(baseUrl + "/chat/completions", UnityWebRequest.kHttpVerbPOST);
        var stream = new ChatStreamHandler();
        request.downloadHandler = stream;
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "text/event-stream");
        request.timeout = TimeoutSeconds;

        var operation = request.SendWebRequest();
        string previousJson = "";
        string previousPreview = "";
        try
        {
            while (!operation.isDone)
            {
                await Awaitable.NextFrameAsync(lifetime.Token);

                string json = stream.Reply;
                if (json == previousJson)
                    continue;

                previousJson = json;
                string preview = NPCGuardResponse.ReadPreview(json);
                if (preview != previousPreview)
                {
                    previousPreview = preview;
                    showPreview?.Invoke(preview);
                }
            }

            lifetime.Token.ThrowIfCancellationRequested();
            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException($"Local model request failed. HTTP {request.responseCode}: {request.error}");

            if (!stream.IsComplete || stream.IsTruncated || stream.HasInvalidEvent)
                throw new FormatException("The model response stream was incomplete or invalid.");

            return NPCGuardResponse.Parse(stream.Reply);
        }
        finally
        {
            if (!operation.isDone)
                request.Abort();
        }
    }
}
