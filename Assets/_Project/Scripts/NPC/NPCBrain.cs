using UnityEngine;
using System;
using System.Threading.Tasks;

[Serializable]
public class NPCResponseData
{
    public string mesaj;
    public string duygu;
}

public class NPCBrain : MonoBehaviour
{
    public async void StartConversation()
    {
        Debug.Log("LLM düþünmeye baþladý...");

        try
        {
            string rawJson = await SendRequestToLLMAsync();
            NPCResponseData response = JsonUtility.FromJson<NPCResponseData>(rawJson);
            Debug.Log($"Gelen Mesaj: {response.mesaj} | Duygu: {response.duygu}");
        }
        catch (Exception e)
        {
            Debug.LogError($"LLM Hatasý: {e.Message}");
            Debug.Log("Gelen Mesaj: Þu an kafam biraz karýþýk, sonra gel. | Duygu: kafasý_karýþýk");
        }
    }

    private async Task<string> SendRequestToLLMAsync()
    {
        await Task.Delay(2000);
        return "{\"mesaj\": \"Sisteme baþarýyla baðlandým.\", \"duygu\": \"mutlu\"}";
    }
}