using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NPCBrain : MonoBehaviour
{
    [Header("NPC Kimliði ve Modeli")]
    [SerializeField] private NPCPersonaSO persona;
    [SerializeField] private string modelName = "llama3.2:1b";

    [Header("Hafýza Ayarlarý")]
    [Tooltip("Hatýrlanacak maksimum mesaj sayýsý (Oyuncu + NPC)")]
    [SerializeField] private int maxHistoryTurns = 6;

    [Header("Servis Baðlantýsý")]
    [SerializeField] private OllamaClient ollamaClient;

    // Konuþma geçmiþini tutan liste
    private List<string> conversationHistory = new List<string>();

    private void Awake()
    {
        if (ollamaClient == null)
        {
            ollamaClient = GetComponent<OllamaClient>();
        }
    }

    public async void StartConversation(string playerMessage = "Merhaba!")
    {
        if (persona == null || ollamaClient == null)
        {
            Debug.LogError("Persona veya OllamaClient eksik!");
            return;
        }

        // 1. Oyuncunun yeni mesajýný hafýzaya ekle
        conversationHistory.Add($"Oyuncu: {playerMessage}");

        // 2. Geçmiþ diyaloglarý Ollama için tek bir metne dönüþtür
        string formattedContext = BuildContextFromHistory();

        Debug.Log($"<color=yellow>{persona.npcName} düþünüyor (Hafýzadaki mesaj sayýsý: {conversationHistory.Count})...</color>");

        try
        {
            // 3. Geçmiþi de içeren isteði servis katmanýna gönder
            NPCResponseData response = await ollamaClient.SendRequestAsync(modelName, persona.systemPrompt, formattedContext);

            if (response != null && !string.IsNullOrEmpty(response.mesaj))
            {
                // 4. NPC'nin cevabýný da hafýzaya ekle
                conversationHistory.Add($"NPC: {response.mesaj}");

                Debug.Log($"<color=green>{persona.npcName}:</color> {response.mesaj} | <color=cyan>Duygu:</color> {response.duygu}");

                // 5. Hafýza sýnýrýný aþtýysak en eski mesajlarý temizle
                TrimHistoryIfNeeded();
            }
            else
            {
                Debug.LogError("Ollama'dan boþ yanýt geldi!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"NPC Diyalog Hatasý: {e.Message}");
        }
    }

    // Geçmiþ mesajlarý tek bir metin bloðu haline getirir
    private string BuildContextFromHistory()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Önceki Konuþma Geçmiþi:");

        foreach (string entry in conversationHistory)
        {
            sb.AppendLine(entry);
        }

        // Model formatý unutmasýn diye her turun sonuna bu zorlamayý ekliyoruz:
        sb.AppendLine("\nSADECE þu JSON formatýnda yanýt ver: {\"mesaj\": \"cevabýn\", \"duygu\": \"nötr\"}");

        return sb.ToString();
    }

    // Belirlenen sýnýrdan eski mesajlarý listeden çýkarýr
    private void TrimHistoryIfNeeded()
    {
        while (conversationHistory.Count > maxHistoryTurns)
        {
            // En eski mesajý (listenin ilk elemanýný) sil
            conversationHistory.RemoveAt(0);
        }
    }

    // Ýstenirse NPC'nin hafýzasýný sýfýrlamak için dýþarýdan çaðrýlabilir
    public void ClearMemory()
    {
        conversationHistory.Clear();
        Debug.Log($"<color=orange>{persona.npcName} hafýzasý temizlendi!</color>");
    }
}