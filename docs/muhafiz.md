# NPC muhafız

[README](../README.md) · [NPC kaynak klasörü](../Assets/_Project/Scripts/NPCGuard/) · [NPC prefabı](../Assets/_Project/Prefabs/PF_NPCGuard.prefab) · [Tam prompt](../Assets/_Project/Prompts/NPCGuardPrompt.txt) · [Sekiz diyalog](diyalog-galerisi.md)

```mermaid
flowchart TD
    A["NPCGuardInteractable.Interact()"] -->|"E ile konuşma isteği"| B["StartConversation()"]
    B -->|"Görev konuşmaya izin verirse"| C["RequestReplyAsync()"]
    C -->|"Prompt, geçmiş ve yeni mesaj"| D["ChatAsync()"]
    D -->|"Akış sürerken"| P["Metin önizlemesi"]
    D -->|"Akış bitince"| E["Parse()"]
    E -->|"openDoor false"| R["Kapı kilitli kalır"]
    E -->|"openDoor true ve oyun koşulları uygun"| G["UnlockAndOpen()"]
    G -->|"Kilit kalkar ve animasyon başlar"| O["Güç odası açılır"]
    D -->|"İstek veya doğrulama hatası"| H["Retry gösterilir"]
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

## Çağrı zinciri

```mermaid
flowchart TD
    A["StartConversation()"] -->|"Karşılama"| C["RequestReplyAsync()"]
    B["SendMessageAsync() / RetryAsync()"] -->|"Yeni veya bekleyen girdi"| C
    C -->|"Mesajlar ve JSON şeması"| D["ChatAsync()"]
    D -->|"Ağ verisi gelir"| E["ReceiveData()"]
    E -->|"SSE satırı tamamlanır"| F["ReadLine()"]
    F -->|"Kısmi yanıt"| G["ReadPreview()"]
    F -->|"Tam akış"| H["Parse()"]
    H -->|"Geçerli izin ve uygun oyun koşulları"| I["UnlockAndOpen()"]
```

| Adım | Kodun içine git | Ne geçiyor? |
| --- | --- | --- |
| Konuşmayı başlat | [`Interact()`](../Assets/_Project/Scripts/NPCGuard/NPCGuardInteractable.cs#L19) → [`StartConversation()`](../Assets/_Project/Scripts/NPCGuard/NPCDialogueController.cs#L61) → [`TryBeginDialogue()`](../Assets/_Project/Scripts/GameFlowManager.cs#L106) | Görev izin verirse gaz durumu sistem promptuna eklenir. |
| Mesajı hazırla | [`SendMessageAsync()`](../Assets/_Project/Scripts/NPCGuard/NPCDialogueController.cs#L82) → [`RequestReplyAsync()`](../Assets/_Project/Scripts/NPCGuard/NPCDialogueController.cs#L98) → [`ChatAsync()`](../Assets/_Project/Scripts/NPCGuard/LLMService.cs#L55) | Sistem mesajı, geçerli geçmiş ve yeni mesaj yerel sunucuya gider. Retry aynı bekleyen girdiyi kullanır. |
| SSE parçalarını birleştir | [`ReceiveData()`](../Assets/_Project/Scripts/NPCGuard/ChatStreamHandler.cs#L17) → [`ReadLine()`](../Assets/_Project/Scripts/NPCGuard/ChatStreamHandler.cs#L38) | Ağdan gelen parçalar satırlara ayrılır, yanıt metni birikir. |
| Önizle ve doğrula | [`ReadPreview()`](../Assets/_Project/Scripts/NPCGuard/NPCGuardResponse.cs#L34) · [`Parse()`](../Assets/_Project/Scripts/NPCGuard/NPCGuardResponse.cs#L21) | Önizleme akış sırasında gösterilir. Parse tam yanıtı ve boolean alanını denetler. |
| Kilidi aç | [`RequestReplyAsync()`](../Assets/_Project/Scripts/NPCGuard/NPCDialogueController.cs#L98) → [`UnlockAndOpen()`](../Assets/_Project/Scripts/DoorInteractable.cs#L89) | Karşılama dışı turda karar true, gaz kapalı ve kapı kullanılabilir olmalı. |

## Bir isteğin yaşam döngüsü

`StartConversation()` oyun akışından izin ister ve sistem promptuna gaz durumunu ekler. `RequestReplyAsync()` mesaj geçmişini model isteğine kopyalar. Geçerli yanıt geldikten sonra tur geçmişe işlenir. Başarısız istekte bekleyen oyuncu mesajı yeniden denenebilir.

`ChatAsync()` akış sırasında görünen metni günceller. Kapı kararı için akışın tamamlanması gerekir. `openDoor` true ise karşılama dışındaki bir mesajda, gaz kesilmişken ve kapı hazırken `UnlockAndOpen()` çağrılır. Bağlantı etiketi diyalog isteğine ve ilk yanıta göre güncellenir. İstek hatası `RequestFailed` ile Retry durumuna döner. Konuşma kapanırsa iptal belirteci tamamlanmamış isteği sonlandırır.

![Merkez çağrısında model repliği ve karar alanı uyuşmuyor](images/diyalog-merkez-cagrisi.svg)

Merkez çağrısında model metinde izin verdi, karar alanında `false` üretti. Yapılandırılmış karar alanı kapıyı kilitli tuttu. Diğer örnekler ve tam metinler [diyalog galerisinde](diyalog-galerisi.md).
