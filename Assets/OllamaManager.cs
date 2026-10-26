using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class OllamaRequest
{
    public string model;
    public string prompt;
    public bool stream;
    public string format; // Ollama'yý kesinlikle JSON üretmeye zorlayan parametre
}

[System.Serializable]
public class OllamaResponse
{
    public string model;
    public string response;
    public bool done;
}

// YENÝ: Yapay zekanýn kararýný okuyacaðýmýz C# kalýbý (Blueprint)
[System.Serializable]
public class NpcKarari
{
    public string diyalog;
    public string eylem;
}

public class OllamaManager : MonoBehaviour
{
    void Start()
    {
        // Oyuncu, muhafýza kapýyý açmasýný söylüyor gibi test ediyoruz
        string sistemKomutu = "Sen kapýyý koruyan bir muhafýzsýn. Oyuncu senden kapýyý açmaný isterse kapýyý aç, istemezse bekle. Sadece þu JSON formatýnda cevap ver: {\"diyalog\": \"cevap cümlen\", \"eylem\": \"kapi_ac veya bekle\"}. Oyuncunun mesajý: Lütfen kapýyý açar mýsýn?";

        StartCoroutine(OllamaIleKonus(sistemKomutu));
    }

    IEnumerator OllamaIleKonus(string soru)
    {
        string url = "http://localhost:11434/api/generate";

        OllamaRequest istekVerisi = new OllamaRequest
        {
            model = "qwen3.5:4b",
            prompt = soru,
            stream = false,
            format = "json" // API'ye verinin standart metin deðil, JSON olmasý gerektiðini bildiriyoruz
        };

        string jsonIstek = JsonUtility.ToJson(istekVerisi);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonGonder = new System.Text.UTF8Encoding().GetBytes(jsonIstek);
            request.uploadHandler = new UploadHandlerRaw(jsonGonder);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // 1. Ollama'nýn genel HTTP paketini açýyoruz
                OllamaResponse cevapObjesi = JsonUtility.FromJson<OllamaResponse>(request.downloadHandler.text);

                // 2. Yapay zekanýn ürettiði metni alýp bizim "NpcKarari" deðiþkenlerimize dönüþtürüyoruz
                NpcKarari karar = JsonUtility.FromJson<NpcKarari>(cevapObjesi.response);

                Debug.Log("NPC'nin Sözü: " + karar.diyalog);
                Debug.Log("Sistemin Algýladýðý Eylem: " + karar.eylem);
            }
            else
            {
                Debug.LogError("Baðlantý Hatasý: " + request.error);
            }
        }
    }
}