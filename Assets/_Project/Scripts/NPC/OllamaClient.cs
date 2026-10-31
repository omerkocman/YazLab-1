using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaClient : MonoBehaviour
{
    [Header("Ollama Sunucu Ayarlarý")]
    [SerializeField] private string apiUrl = "http://localhost:11434/api/generate";

    // Ýstek atan ve yanýtý dönen ana servis metodu
    public async Task<NPCResponseData> SendRequestAsync(string modelName, string systemPrompt, string userMessage)
    {
        // 1. Prompt birleþtirme (System Prompt + User Message)
        string fullPrompt = $"{systemPrompt}\n\nOyuncu: {userMessage}\nNPC:";

        // 2. Ýstek paketini hazýrlama
        OllamaRequestData requestBody = new OllamaRequestData
        {
            model = modelName,
            prompt = fullPrompt,
            stream = false
        };

        string jsonPayload = JsonUtility.ToJson(requestBody);

        // 3. HTTP POST isteði gönderme
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"HTTP Hatasý: {request.error} | Detay: {request.downloadHandler.text}");
            }

            // 4. Yanýtý çözme ve temizleme
            OllamaResponseWrapper responseWrapper = JsonUtility.FromJson<OllamaResponseWrapper>(request.downloadHandler.text);

            // OllamaClient.cs içindeki bu bloðu güncelle:
            if (responseWrapper != null && !string.IsNullOrEmpty(responseWrapper.response))
            {
                string cleanJson = CleanJsonString(responseWrapper.response);

                try
                {
                    return JsonUtility.FromJson<NPCResponseData>(cleanJson);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"JSON Parse Edilemedi! Gelen Ham Metin: {cleanJson} | Hata: {ex.Message}");
                    // Hata durumunda varsayýlan bir yanýt dönerek oyunun çökmesini önlüyoruz:
                    return new NPCResponseData { mesaj = cleanJson, duygu = "nötr" };
                }
            }

            return null;
        }
    }

    // JSON dýþýndaki gereksiz týrnak veya markdown iþaretlerini temizleyen yardýmcý yöntem
    private string CleanJsonString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        // Metin içindeki ilk '{' ve son '}' karakterlerinin konumunu bulur
        int firstOpen = input.IndexOf('{');
        int lastClose = input.LastIndexOf('}');

        // Eðer geçerli bir JSON bloðu varsa sadece o aralýðý alýr
        if (firstOpen != -1 && lastClose != -1 && lastClose > firstOpen)
        {
            return input.Substring(firstOpen, (lastClose - firstOpen) + 1);
        }

        return input.Trim();
    }
}

// Ollama API Ýstek ve Yanýt Veri Sýnýflarý
[Serializable]
public class OllamaRequestData
{
    public string model;
    public string prompt;
    public bool stream;
}

[Serializable]
public class OllamaResponseWrapper
{
    public string response;
}

[Serializable]
public class NPCResponseData
{
    public string mesaj;
    public string duygu;
}