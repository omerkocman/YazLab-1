using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Threading.Tasks;

//ollamaoptions sýnýfý eklendi 
[Serializable]
public class OllamaOptions
{ 
    public int num_predict = 1000; // Maksimum token sayýsý
}

// Ollama'ya göndereceðimiz veri yapýsý
[Serializable]
public class OllamaRequestData
{
    public string model;
    public string prompt;
    public bool stream = false; // Tek seferde yanýt almak için þart
    public string format = "json"; // Yanýtýn JSON olmasýný zorunlu kýlar
    public OllamaOptions options =new OllamaOptions(); // Ek ayarlar için opsiyonel alan
}

// Ollama'dan dönen ana yanýt yapýsý
[Serializable]
public class OllamaResponseWrapper
{
    public string response;
    public string error;
}

// Bizim NPC diyalog veri yapýmýz
[Serializable]
public class NPCResponseData
{
    public string mesaj;
    public string duygu;
}

public class NPCBrain : MonoBehaviour
{
    [Header("Ollama Ayarlarý")]
    [SerializeField] private string apiUrl = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "qwen3.5:4b"; // Bilgisayarýnda yüklü model adý (örn: llama3, mistral, gemma vb.)

    public async void StartConversation()
    {
        
        try
        {
            string prompt = "doðrudan cevap ver "+
                            "Sen bir oyun NPC'sisin. Oyuncu sana 'Merhaba, kentinize yeni geldim' dedi. " +
                            "Cevabýný SADECE aþaðýdaki JSON formatýnda ver, ekstra açýklama yazma: " +"cevabýn kýsa olsun "+
                            "Direkt,net ve doðal konuþ "+
                            "{\"mesaj\": \"kýsa cevabýn\", \"duygu\": \"mutlu/üzgün/korkmuþ/sinirli/nötr\"}";
            NPCResponseData response = await SendRequestToOllamaAsync(prompt);


            if (response != null && !string.IsNullOrEmpty(response.mesaj))
            {
                Debug.Log($"<color=green>NPC Yanýtý:</color> {response.mesaj} | <color=cyan>Duygu:</color> {response.duygu}");
            }
            else
            {
                Debug.LogError("Ollama'dan gelen cevap okunamadý veya format hatalý!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ollama Baðlantý Hatasý: {e.Message}");
        }
    }

    private async Task<NPCResponseData> SendRequestToOllamaAsync(string systemPrompt)
    {
        OllamaRequestData requestBody = new OllamaRequestData
        {
            model = modelName,
            prompt = systemPrompt,
            stream = false,
            format = "json",
            options =new OllamaOptions { num_predict =1000}//en fazla 300 token kullanýr
        };

        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                // Ollama'dan gelen ham metni konsola yazdýrýyoruz
                Debug.Log("<color=orange>Ollama Ham Yanýt:</color> " + request.downloadHandler.text);
                throw new Exception($"HTTP Hatasý: {request.error} | Detay: {request.downloadHandler.text}");
            }
            Debug.Log("<color=orange>Ollama Ham Yanýt:</color> " + request.downloadHandler.text);

            // 1. Dýþ paketi çöz
            OllamaResponseWrapper wrapper = JsonUtility.FromJson<OllamaResponseWrapper>(request.downloadHandler.text);

            // Ollama model ismi bulunamadýysa veya iç hata verdiyse hatayý yakala
            if (wrapper != null && !string.IsNullOrEmpty(wrapper.error))
            {
                throw new Exception($"Ollama Model Hatasý: {wrapper.error}");
            }

            if (wrapper == null || string.IsNullOrEmpty(wrapper.response))
            {
                throw new Exception($"Ollama boþ yanýt döndürdü. Gelen Ham Metin: {request.downloadHandler.text}");
            }

            // 2. Qwen'in fazlalýklarýný temizle
            string cleanedJson = CleanJsonString(wrapper.response);

            // 3. Temizlenmiþ JSON'ý oyuna aktar
            NPCResponseData npcData = JsonUtility.FromJson<NPCResponseData>(cleanedJson);
            return npcData;
        }

    }

    // JSON metnini fazlalýklardan temizleyen yardýmcý metot
    private string CleanJsonString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        string cleaned = input.Trim();
        if (cleaned.StartsWith("```json")) cleaned = cleaned.Substring(7);
        else if (cleaned.StartsWith("```")) cleaned = cleaned.Substring(3);
        if (cleaned.EndsWith("```")) cleaned = cleaned.Substring(0, cleaned.Length - 3);
        return cleaned.Trim();
    }

}