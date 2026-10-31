using UnityEngine;

// "Scriptable Objects/" takýsýný ekledik ki doðrudan baktýðýn menünün içine düþsün:
[CreateAssetMenu(fileName = "NewNPCPersona", menuName = "Scriptable Objects/NPC/Persona")]
public class NPCPersonaSO : ScriptableObject
{
    [Header("NPC Bilgileri")]
    public string npcName = "Köy Muhafýzý";

    [Header("Kiþilik ve Talimat (Prompt)")]
    [TextArea(4, 10)]
    public string systemPrompt = "Sen bir oyun NPC'sisin. Türkçe konuþursun. " +
                                 "SADECE þu JSON formatýnda cevap ver: {\"mesaj\": \"kýsa cevabýn\", \"duygu\": \"mutlu/üzgün/korkmuþ/sinirli/nötr\"}";

    [Header("Varsayýlan Ayarlar")]
    public string defaultEmotion = "nötr";
}