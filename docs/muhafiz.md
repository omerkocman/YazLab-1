# Birim-734 muhafız modülü

[README](../README.md) · [NPC kaynak klasörü](../Assets/_Project/Scripts/NPCGuard/) · [NPC prefabı](../Assets/_Project/Prefabs/PF_NPCGuard.prefab) · [Tam prompt](../Assets/_Project/Prompts/NPCGuardPrompt.txt) · [Sekiz diyalog](diyalog-galerisi.md)

```mermaid
flowchart TD
    A["NPCGuardInteractable · E<br/>Konuşma isteği"] --> B["NPCDialogueController<br/>Gaz durumunu denetle, geçmişi hazırla"]
    B --> C["LLMService.ChatAsync<br/>Yerel sunucuya POST /v1/chat/completions"]
    C --> D["ChatStreamHandler<br/>SSE parçalarını birleştir"]
    D --> E["NPCGuardResponse.Parse<br/>Tam ve geçerli JSON"]
    D -.-> U["NPCDialogueUI<br/>Metin önizlemesi, bekleme ve Retry"]
    E --> F{"Karşılama dışı ve openDoor true?"}
    F -- "Hayır" --> U
    F -- "Evet" --> G["Gaz kapalı ve kapı kullanılabilir<br/>UnlockAndOpen"]
    G --> U
    C -- "İstek hatası" --> H["RequestFailed"]
    E -- "Geçersiz yanıt" --> H
    H --> U
```

| Dosya | Görevi |
| --- | --- |
| [NPCGuardInteractable.cs](../Assets/_Project/Scripts/NPCGuard/NPCGuardInteractable.cs) | Oyuncunun etkileşimini konuşma isteği olarak yayınlar. |
| [NPCDialogueController.cs](../Assets/_Project/Scripts/NPCGuard/NPCDialogueController.cs) | Sistem mesajı ve geçmişi hazırlar. Kapı kararını oyun durumuyla birlikte uygular. |
| [LLMService.cs](../Assets/_Project/Scripts/NPCGuard/LLMService.cs) | Yerel adres, model ve zaman aşımı ayarlarını saklar. Akışlı isteği gönderir ve kareler arasında bekler. |
| [ChatRequest.cs](../Assets/_Project/Scripts/NPCGuard/ChatRequest.cs) ve [GuardResponseFormat.cs](../Assets/_Project/Scripts/NPCGuard/GuardResponseFormat.cs) | Mesajları ve iki alanlı JSON şemasını isteğe koyar. |
| [ChatStreamHandler.cs](../Assets/_Project/Scripts/NPCGuard/ChatStreamHandler.cs) ve [ChatStreamChunk.cs](../Assets/_Project/Scripts/NPCGuard/ChatStreamChunk.cs) | SSE satırlarını birleştirir, akış sonunu ve kesilmeyi izler. |
| [NPCGuardResponse.cs](../Assets/_Project/Scripts/NPCGuard/NPCGuardResponse.cs) | Metin önizlemesini çıkarır. Tam JSON'u kontrol eder ve ayrıştırır. |
| [NPCDialogueUI.cs](../Assets/_Project/Scripts/UI/NPCDialogueUI.cs), [HUD.uxml](../Assets/_Project/UI/Documents/HUD.uxml), [HUD.uss](../Assets/_Project/UI/Styles/HUD.uss) | Mesaj balonları, bağlantı durumu, hata ve Retry ekranı. |

## Bir isteğin yaşam döngüsü

`StartConversation()` oyun akışından izin ister ve sistem promptuna gaz durumunu ekler. `RequestReplyAsync()` mesaj geçmişini model isteğine kopyalar. Geçerli yanıt geldikten sonra tur geçmişe işlenir. Başarısız istekte bekleyen oyuncu mesajı yeniden denenebilir.

`ChatAsync()` akış sırasında görünen metni günceller. Kapı kararı için akışın tamamlanması gerekir. `openDoor` true ise karşılama dışındaki bir mesajda, gaz kesilmişken ve kapı hazırken `UnlockAndOpen()` çağrılır. Bağlantı etiketi diyalog isteğine ve ilk yanıta göre güncellenir. İstek hatası `RequestFailed` ile Retry durumuna döner. Konuşma kapanırsa iptal belirteci tamamlanmamış isteği sonlandırır.

![Merkez çağrısında model repliği ve karar alanı uyuşmuyor](images/diyalog-merkez-cagrisi.svg)

Merkez çağrısında model metinde izin verdi, karar alanında `false` üretti. Yapılandırılmış karar alanı kapıyı kilitli tuttu. Diğer örnekler ve tam metinler [diyalog galerisinde](diyalog-galerisi.md).
