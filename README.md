# Blackline Protocol

**Kocaeli Üniversitesi · Teknoloji Fakültesi · Bilişim Sistemleri Mühendisliği**\
**Yazılım Geliştirme Laboratuvarı-I · Proje 1**\
**Grup 52 · Ömer Lütfullah Koçman ve Yunus Emre Yılmaztürk · 6 Kasım 2026**

Oyuncu önce otonom robota gazı kestirir, sonra yerel dil modeliyle çalışan muhafızı ikna eder ve Final şalterini kapatır. Robotun hareketini NavMesh ve durum makinesi, muhafızın erişim kararını doğrulanmış JSON ve Unity’deki görev koşulları yönetir.

[Oyun akışı](#1-problem-ve-oyun-senaryosu) · [Kurulum](#2-kurulum-ve-çalıştırma) · [Robot ve durum makinesi](#3-otonom-bakım-robotu) · [NPC ve yerel LLM](#4-npc-muhafız-ve-yerel-llm-mimarisi) · [Testler](#6-testler)

## Proje dosyaları

`├─` 📁 [`Assets/`](Assets/) — Unity sahneleri, varlıklar ve oyun kodu\
`│  └─` 📁 [`_Project/`](Assets/_Project/) — Bu oyunun sahneleri ve bileşenleri\
`│     ├─` 📁 [`Scenes/`](Assets/_Project/Scenes/) — Oyun sahneleri ve NavMesh verisi\
`│     │  └─` 📄 [`Main.unity`](Assets/_Project/Scenes/Main.unity) — Oynanan tesis\
`│     ├─` 📁 [`Prefabs/`](Assets/_Project/Prefabs/) — Robot, muhafız, kapı ve şalter şablonları\
`│     ├─` 📁 [`Scripts/`](Assets/_Project/Scripts/) — Oyun mantığı\
`│     │  ├─` 📁 [`Robot/`](Assets/_Project/Scripts/Robot/) — Algı, keşif ve hareket · [çağrı haritası](docs/robot-guide.md#robotbrain)\
`│     │  │  ├─` 📄 [`RobotBrain.cs`](Assets/_Project/Scripts/Robot/RobotBrain.cs) — Durum makinesi\
`│     │  │  ├─` 📄 [`RobotSensor.cs`](Assets/_Project/Scripts/Robot/RobotSensor.cs) — Nesne ve çevre taraması\
`│     │  │  ├─` 📄 [`RobotExplorer.cs`](Assets/_Project/Scripts/Robot/RobotExplorer.cs) — Aday puanlama ve ziyaret belleği\
`│     │  │  └─` 📄 [`RobotNavigation.cs`](Assets/_Project/Scripts/Robot/RobotNavigation.cs) — Yol hesabı ve NavMeshAgent\
`│     │  ├─` 📁 [`NPCGuard/`](Assets/_Project/Scripts/NPCGuard/) — Konuşma, SSE ve kapı kararı · [çağrı haritası](docs/guard-guide.md#çağrı-zinciri)\
`│     │  │  ├─` 📄 [`NPCDialogueController.cs`](Assets/_Project/Scripts/NPCGuard/NPCDialogueController.cs) — Konuşma geçmişi ve eylem\
`│     │  │  └─` 📄 [`LLMService.cs`](Assets/_Project/Scripts/NPCGuard/LLMService.cs) — Yerel HTTP istemcisi\
`│     │  ├─` 📁 [`Player/`](Assets/_Project/Scripts/Player/) — Hareket, sağlık ve etkileşim\
`│     │  ├─` 📁 [`Interactable/`](Assets/_Project/Scripts/Interactable/) — Ortak etkileşim sözleşmesi\
`│     │  ├─` 📁 [`UI/`](Assets/_Project/Scripts/UI/) — Menü, diyalog ve görev sonu davranışı\
`│     │  └─` 📄 [`GameFlowManager.cs`](Assets/_Project/Scripts/GameFlowManager.cs) — Görev başlangıcı, duraklatma ve Final\
`│     ├─` 📁 [`Prompts/`](Assets/_Project/Prompts/) — Muhafızın sistem talimatı\
`│     ├─` 📁 [`Input/`](Assets/_Project/Input/) — Oyuncu giriş eylemleri\
`│     ├─` 📁 [`UI/`](Assets/_Project/UI/) — HUD yerleşimi ve USS stilleri\
`│     ├─` 📁 [`Models/`](Assets/_Project/Models/) — Sahne modelleri\
`│     └─` 📁 [`Metarials/`](Assets/_Project/Metarials/) — Yüzey malzemeleri\
`├─` 📁 [`Packages/`](Packages/) — Unity bağımlılıkları\
`├─` 📁 [`ProjectSettings/`](ProjectSettings/) — Motor ve build ayarları\
`└─` 📁 [`docs/`](docs/) — Modül rehberleri ve kanıtlar\
`   ├─` 📄 [`robot-guide.md`](docs/robot-guide.md) — Durumdan işleyiciye, işleyiciden sensör ve NavMesh’e\
`   ├─` 📄 [`guard-guide.md`](docs/guard-guide.md) — Oyuncu mesajından doğrulanmış kapı kararına\
`   ├─` 📄 [`gameplay-flow.md`](docs/gameplay-flow.md) — Gaz, kapı ve görev sonu\
`   ├─` 📄 [`dialogue-gallery.md`](docs/dialogue-gallery.md) — Yerel modelin gerçek test yanıtları\
`   └─` 📁 [`images/`](docs/images/) — Şemalar ve Unity ekran görüntüleri

```mermaid
flowchart LR
    A["Bakım robotu"] -->|"Gazı kesme komutu"| B["Gazlı koridor güvenli"]
    B -->|"Oyuncu gerekçe sunar"| C["Muhafız"]
    C -->|"Erişim onaylanır"| D["Güç odası açılır"]
    D -->|"Final şalteri kapatılır"| E["Görev tamamlandı"]
```


## 1. Problem ve oyun senaryosu

Bir araştırma tesisinde kontrolden çıkan robotlar koridorlara zehirli gaz salar. Oyuncu gazlı koridorda hasar aldığı için önce bakım robotundan yardım ister. Robot labirenti keşfeder, gaz şalterini bulur ve gazı keser. Güvenli koridora ulaşan oyuncu, Ana Sunucu ve Güç Odası'nı koruyan güvenlik robotuyla serbest metin üzerinden konuşur. Muhafız ikna olursa odanın kilidi açılır. Oyuncu içerideki **Final** şalterini kapattığında tesis robotlarının enerjisi hikâye gereği kesilir ve oyun görev başarısıyla biter.

Projede iki ayrı yapay zekâ bileşeni vardır. Bakım robotu hareket ve etkileşim kararlarını sensör ölçümleri ve NavMesh üzerinden alır. Muhafız yerel dil modelini kullanarak konuşur ve erişim kararı üretir. İkisi gaz durumu ve kapı gibi ortak oyun nesneleri üzerinden aynı senaryoya bağlanır.

![Gaz tetikleyicisi ve kapının A/B yaklaşma noktaları](docs/images/gaz-ve-kapi.svg)

[Gaz, kapı ve görev akışı rehberi](docs/gameplay-flow.md), efektin hasar tetikleyicisine nasıl bağlandığını ve robotun kapıya hangi taraftan yaklaştığını açıklar.

### Oyuncu görevi başlatıyor ve gaz kesiliyor

![Oyuncu bakım robotuna bakarken Activate Robot komutu](docs/images/01-robot-gorev.png)

| Gaz açık · fiziksel engel | Gaz kapalı · yol açıldı |
| --- | --- |
| ![Koridor girişinde gaz bulutu](docs/images/02-gaz-acik.png) | ![Aynı açıdan gazı kesilmiş koridor](docs/images/03-gaz-kapali.png) |

## 2. Kurulum ve çalıştırma

- Projeyi **Unity 6000.6.4f1** ile açın. Projenin bağımlılıkları `Packages/manifest.json` dosyasındadır.
- Editörde [Main.unity](Assets/_Project/Scenes/Main.unity) sahnesini açın. Windows build için **Main**, Build Profiles sahne listesinde başlangıç sahnesi olarak ayarlanmıştır.
- Yerel model sunucusunda **Qwen3-4B-Instruct-2507** modelini yükleyin. Proje, akışlı sohbet ve JSON şeması destekleyen yerel bir `/v1/chat/completions` uç noktası kullanır. Varsayılan sahne adresi `http://localhost:1235/v1` ve model etiketi `qwen3-4b-instruct-2507` şeklindedir. Sunucunuzdaki adres veya etiket farklıysa oyunun **Settings** ekranında düzenleyin. Ollama ile kullanırken sunucunuzdaki tam model etiketini ve yerel adresi girin.
- **Play** ile görevi başlatın. WASD ile hareket edin, fare ile bakın ve etkileşim için **E** tuşunu kullanın. Robotu etkinleştirin, gaz kesilince muhafıza ulaşın ve diyaloğa girin. İzin aldıktan sonra güç odasındaki Final şalterini kapatın.
- Oyun sırasında **Esc** duraklatma menüsünü açar. **Resume** göreve döner, **Quit** uygulamayı kapatır. Diyalog açıkken **Esc** önce konuşmayı kapatır.
- Windows paketini kullanırken klasörü bütünüyle çıkarın ve `Birim734.exe` dosyasını yanındaki Unity veri klasörleriyle birlikte çalıştırın. Dil modeli ve yerel sunucu ayrı olarak çalışır.

## 3. Otonom bakım robotu

| Bileşen | Sorumluluk |
| --- | --- |
| [RobotCommandReceiver](Assets/_Project/Scripts/Robot/RobotCommandReceiver.cs) | Oyuncunun etkileşimini `DisableGas` görevine çevirir. |
| [RobotSensor](Assets/_Project/Scripts/Robot/RobotSensor.cs) | Çevre ray’leriyle açıklık ve mesafe ölçer. Etkileşim nesnelerini mesafe, görüş açısı ve görüş hattı ile ayıklar. |
| [RobotExplorer](Assets/_Project/Scripts/Robot/RobotExplorer.cs) | Ölçümlerden NavMesh üzerindeki keşif adaylarını üretir. Ziyaret edilen noktaları hatırlar, yakın adayları birleştirir ve yeni yerel ilerlemeyi önceliklendirir. |
| [RobotNavigation](Assets/_Project/Scripts/Robot/RobotNavigation.cs) | Tam NavMesh rotasını doğrular, rota uzunluğunu hesaplar ve `NavMeshAgent` hareketini yönetir. |
| [RobotBrain](Assets/_Project/Scripts/Robot/RobotBrain.cs) | Arama, keşif, normal kapı açma, şaltere yaklaşma ve etkileşim durumlarını yönetir. |

Keşif sırasında çevre ray’inin izin verdiği yönde bir aday alınır ve NavMesh'e örneklenir. Ajan yalnızca tamamlanabilir yolları değerlendirir. Yerel adaylarda ileri yöndeki ilerleme ödüllendirilir, yol uzunluğu puanı düşürür. Yerel seçenekler tükenirse geçmişten saklanan adaylar ziyaret edilen alanlardan uzaklık ve yol maliyetine göre değerlendirilir. Robot yürürken de şalteri tarar, ulaşılabilir bir hedef görürse keşif rotasını bırakıp ona yönelir. Seçilen rotalar sahnedeki geometri ve sensör ölçümlerinden çıkarılır.

### RobotBrain · sonlu durum makinesi

[Update() karar şemasını aç](docs/robot-guide.md#update-karar-şeması). Her işleyicinin alt çağrılarını ve hangi koşulda ne yaptığını ayrı şemada görebilirsin.

`Update()`, mevcut `RobotState` için yalnızca ilgili durum işleyicisini çalıştırır. `RobotBrain` hedefi ve yapılacak işi seçer. Sensör ölçümlerini `RobotSensor` üretir, keşif adayını `RobotExplorer` seçer, yürüyüşü ise `RobotNavigation` üzerinden `NavMeshAgent` yürütür.

| Durum | Karar ve sonraki adım |
| --- | --- |
| `Idle` | Oyuncunun komutuyla `StartTask(DisableGas)` çağrılır ve arama başlar. |
| `Searching` | `Scan()` çalışır. Önce görünen ve gaz görevi için uygun şalterler, sonra etkileşilebilir normal kapılar değerlendirilir. İkisi de sonuç vermezse keşfe geçilir. |
| `Exploring` | Sensör ölçümlerinden seçilmiş hedefe `TryMoveTo()` ile tam yol kurulur. Hedef ya da yol yoksa görev başarısız olur. |
| `MovingToExplorePoint` | Ajan yürürken yaklaşık 0,3 oyun saniyesinde bir şalter tarar. Uygun şalter bulursa rotayı değiştirir. Keşif hedefine varırsa konumu kaydedip aramaya döner. |
| `MovingToDoor` / `OpeningDoor` | Robot kapının A/B noktalarından yakın olanı önce dener. Kapı açılırsa aramaya, yaklaşma zaman aşımına uğrarsa veya açma denemesi sonuç vermezse keşfe döner. |
| `MovingToTarget` | Şalterin ulaşılabilir etkileşim noktasına gider. Şalter referansı kaybolursa veya yolculuk zaman aşımına uğrarsa görev başarısız olur. |
| `Interacting` | Mesafe ve görüş hattını yeniden kontrol eder. Şalteri tetikler ve animasyon tamamlanana kadar bekler. Şalter kapandıysa görev tamamlanır. |
| `Completed` / `Failed` | Görev için son durumlar. Uygun şalter taramada zaten kapalıysa robot doğrudan `Completed` durumuna geçebilir. |

**Karar sırası.** Gaz şalteri en yüksek önceliktedir. Uygun şalter bulunmazsa robot normal bir kapıyı deneyebilir. Bu da mümkün değilse keşif döngüsü yeni bir gözlem noktası arar.

```mermaid
flowchart TD
    S["HandleSearching()"] -->|"Çevredeki nesneleri tara"| V["Scan()"]
    V -->|"Önce gaz şalterini dene"| T["TryApproachVisibleSwitch()"]
    T -->|"Uygun hedef bulunamadı"| D["TryApproachDoor()"]
    D -->|"Açılabilir kapı da yok"| E["HandleExploring()"]
    T -->|"Kapalı şalter doğrulandı"| C["Gaz kesildi"]
    T -->|"Açık şalter bulundu"| I["Şaltere yönel"]
    D -->|"Kapı açılınca"| R["Yeniden ara"]
```

#### Robotun durum geçişleri

**Arama ve keşif**

```mermaid
flowchart TD
    I["Idle"] -->|"Gazı kes komutu"| S["Searching"]
    S -->|"Uygun şalter veya kapı bulunamadı"| E["Exploring"]
    E -->|"Tam keşif rotası kuruldu"| M["MovingToExplorePoint"]
    M -->|"Varış ve ziyaret kaydı"| S
    E -->|"Aday veya tam rota yok"| F["Failed"]
```

**Normal kapıyı geçme**

```mermaid
flowchart TD
    S["Searching"] -->|"Kapıya yaklaşma rotası bulundu"| M["MovingToDoor"]
    S -->|"Robot zaten yaklaşma noktasında"| O["OpeningDoor"]
    M -->|"Kapıya varıldı"| O
    O -->|"Kapı açıldı"| A["Searching"]
    M -->|"Yaklaşma süresi doldu"| E["Exploring"]
    O -->|"Açma veya etkileşim başarısız"| E
    M -->|"Kapı referansı kayıp"| F["Failed"]
    O -->|"Kapı referansı kayıp"| F
```

**Şalteri bulma ve gazı kesme**

```mermaid
flowchart TD
    S["Searching veya MovingToExplorePoint"] -->|"Açık şalter uzakta ve yaklaşma rotası var"| M["MovingToTarget"]
    S -->|"Açık şalter etkileşim mesafesinde"| I["Interacting"]
    S -->|"Uygun şalter zaten kapalı"| C["Completed"]
    M -->|"Şaltere varıldı"| I
    M -->|"Şalter kayıp veya süre doldu"| F["Failed"]
    I -->|"Şalter kapalı"| C
    I -->|"Mesafe, görüş veya kapatma sonucu geçersiz"| F
```

**Örnek koşu.** Robot `Searching → Exploring → MovingToExplorePoint` ile bir koridoru keşfeder. Hareket sırasında şalteri görürse keşif durağına varmayı beklemeden `MovingToTarget` durumuna geçebilir. Şaltere ulaşınca `Interacting` durumunda kapatma animasyonunun bitmesini bekler. Robotun `Completed` olması gaz görevinin bittiğini gösterir. Oyunun bitişi ayrı olarak Final şalterine bağlıdır.

Geliştirme sırasında Unity Scene görünümünde NavMesh ve keşif güzergâhı. Siyah çizgi, gözlenen rotayı göstermek için ekran görüntüsüne eklenmiştir.

![Labirentte NavMesh ve robotun izlediği keşif rotası](docs/images/navmesh-kesif.png)

### Sensör ve keşif algoritması

![Robotun nesne görüş hattı ve çevre taraması](docs/images/robot-tarama.svg)

Şema taramaları üstten görünümde örnekler. Aşağıdaki kareler Unity Play Mode sırasında çekildi.

#### `Scan()` — Robot şalteri gerçekten görüyor mu?

```mermaid
flowchart TD
    A["Scan()"] -->|"Menzil ve katman filtresi"| B["Physics.OverlapSphereNonAlloc()"]
    B -->|"Her adayın açısını ölç"| C["Vector3.Angle()"]
    C -->|"Görüş alanı içindeyse"| E["Physics.Raycast()"]
    E -->|"İlk isabet aynı RobotDetectable ise"| G["Görünür hedef listesine ekle"]
    E -->|"Araya engel girerse"| R["Adayı atla"]
```

Küre taraması yakındaki nesneleri hızlıca adaylaştırır. Açı kontrolü arkadaki nesneleri eler. Raycast arada duvar olup olmadığını denetler. Son çizgi hedefe ulaşıyorsa yeşil, araya başka bir collider giriyorsa kırmızı görünür. `RobotBrain.TryApproachVisibleSwitch()` görünür adayın gaz görevi için geçerli olup olmadığını kontrol eder, etkileşim mesafesindeyse şaltere yönelip etkileşir, uzaktaysa önce ulaşılabilir bir yaklaşma noktası arar. Robot keşif noktasına yürürken şalter taraması yaklaşık **0,3 oyun saniyesinde bir** tekrarlanır.

#### `ScanEnvironment()` — Şalter görünmüyorsa robot nereye gider?

```mermaid
flowchart TD
    A["TryGetNextPoint()"] -->|"Açık yönleri ölç"| B["ScanEnvironment()"]
    B -->|"Ray mesafesinden engel payını çıkar"| D["NavMesh.SamplePosition()"]
    D -->|"Zemine oturan aday"| E["AddFrontier()"]
    E -->|"Ziyaret edilmemiş adayları ölç"| F["TryGetPathLength()"]
    F -->|"Tam yolları puanla ve hedef seç"| G["TryMoveTo()"]
    G -->|"Varış doğrulanırsa"| H["MarkVisited()"]
```

| Fonksiyon | Kullanılma nedeni |
| --- | --- |
| `ScanEnvironment()` | Önce hedef nesne bulmak zorunda kalmadan duvarları ve açık yönleri ölçer. Kırmızı ışın engele isabet eder, yeşil ışın tarama mesafesi içinde engel bulmaz. |
| `TryGetNextPoint()` | Ölçümleri NavMesh üzerinde sınanabilir keşif hedeflerine çevirir. Son taramadan gelen kısa yollu yerel adaylar, önceki taramalardan kalan adaylardan önce değerlendirilir. |
| `TryGetPathLength()` | Duvarın ötesi kuş uçuşu yakın görünebilir. Tam rotanın köşeleri boyunca gerçek mesafeyi hesaplar. |
| `TryMoveTo()` | NavMesh'in doğruladığı rotayı ajana verir ve yürüyüşü başlatır. |
| `MarkVisited()` | Durulan konumu kaydeder. Yakındaki hedefleri temizleyerek aynı bölgeye tekrar gitme olasılığını azaltır. |

Scene görünümünde `PF_Robot` seçildiğinde **mavi** işaretler ziyaret edilen konumları, **yeşil** işaretler öncelikli yerel adayları, **sarı** işaretler diğer bekleyen adayları, **mor** işaret mevcut keşif hedefini gösterir. Yeşil/kırmızı **ışınlar** sensör ölçümüdür, renkli **küreler** keşif belleğidir.


### Unity’de sensör ve keşif

| Nesne görüş hattı · Scan | Çevre ışınları · ScanEnvironment |
| --- | --- |
| ![Robot ve şalterin Unity Scene görünümü](docs/images/05-scan-goruyor.png) | ![Robot çevresinde duvarlara uzanan ışınlar](docs/images/07-scan-environment.png) |

![Gazlı bölgede NavMesh üzerinde robot ve renkli keşif adayları](docs/images/04-robot-navmesh.png)

#### Keşif adayının puanı

Son taramada görülen ve rota uzunluğu `explorationStep + navMeshSampleRadius + frontierMergeRadius` sınırını aşmayan adaylar yerel sayılır. Ulaşılabilir yerel aday varsa eski adaylara geçilmez. Her grup içinde en yüksek puan kazanır:

| Aday | Hesap |
| --- | --- |
| Yerel | `Dot(aday − robotKonumu, ileriYön) − 0.15 × rotaUzunluğu` |
| Eski | `EnYakınZiyaretNoktasınaUzaklık − 0.15 × rotaUzunluğu` |

İç çarpım, robotun ileri yönündeki ilerlemeyi ölçer. Eski adayların puanında daha az keşfedilmiş bölgeler avantaj kazanır. Rota uzunluğu her iki grupta da NavMesh yolunun köşeleri üzerinden hesaplanır. Ziyaret belleği ve puanlama keşif için sezgisel bir yöntemdir. Robot bütün haritayı önceden hedef listesi olarak taşımaz.

### Robotun yolu ve ziyaret izi

![Açık yöndeki kısa yol ile duvar arkasındaki adayın NavMesh rotası](docs/images/robot-rota-secimi.svg)

Şemada B'ye çizilen kırmızı kesik çizgi duvarın içinden geçer. Robotun kullanabileceği mor rota duvarı dolaşır. `TryGetPathLength()` rota köşelerindeki parçaların uzunluklarını toplar. Tam yolu olmayan aday elenir. Ulaşılabilir adaylarda yol maliyeti, yerel ilerleme veya ziyaret edilmiş alanlardan uzaklıkla birlikte puanlanır.

![Üç adımda robotun ziyaret ettiği durakları hatırlaması](docs/images/robot-ziyaret-bellegi.svg)

`MarkVisited()` ilk konumu ve robotun vardığı keşif duraklarını `visitedPoints` listesinde tutar. `visitedRadius` içine giren adaylar elenir. Mavi noktalar ve çemberler Unity Scene görünümünde bu belleği gösterir. Çizimdeki kesik hat hareketi örnekler. Kodda her karede kaydedilen bir konum izi yoktur.

```mermaid
flowchart TD
    A["HandleMovingToExploringPoint()"] -->|"Varış kontrolü"| B["HasReachedDestination()"]
    B -->|"Varıldıysa"| V["MarkVisited()"]
    B -->|"Yürüyüş sürüyor ve 0,3 sn dolduysa"| C["Scan()"]
    C -->|"Görünür adayları incele"| D["TryApproachVisibleSwitch()"]
    D -->|"Uygun şalter bulunursa"| E["Keşif rotasını bırak"]
    E -->|"Kapanma doğrulanınca"| F["Gaz kesildi"]
    D -->|"Uygun şalter yoksa"| A
```


### Rota boyunca dört Unity karesi

| 1 · Gaz yanındaki koridor | 2 · Duvar köşesindeki hedef |
| --- | --- |
| ![Robot gaz bulutunun yanındaki koridorda, ileride renkli keşif noktaları](docs/images/robot-rota-01-koridor.png) | ![Robot dönüşte, köşede mor hedef ve önceki bölgede sarı adaylar](docs/images/robot-rota-02-kose.png) |

| 3 · Açık alanda yeni adaylar | 4 · Sonraki keşif adımı |
| --- | --- |
| ![Robot daha geniş alanda, çevresinde ışınlar ve yeşil yerel adaylar](docs/images/robot-rota-03-acik-alan.png) | ![Aynı bölgedeki sonraki karede robot ve keşif işaretleri](docs/images/robot-rota-04-sonraki-adim.png) |

Bu kareler görev ilerlerken aday noktaların değişimini gösterir. Sarı noktalar bekleyen adaylardır. Renkli noktalar tek başına robotun kat ettiği rotanın çizgisi değildir.

## 4. NPC muhafız ve yerel LLM mimarisi

Muhafız karantina protokolüne bağlı, kısa ve otoriter konuşan bir güvenlik robotudur. İletişim ağı kesildiği için koridorlarda yaşananları bilmez ve sensöründe yalnızca basınç anomalisi görür. [Tam sistem promptu](Assets/_Project/Prompts/NPCGuardPrompt.txt) karşılama biçimini, reddetme koşullarını ve gerekçeli erişim yollarını tanımlar. Gerekçesiz talep, unvan iddiası, konu dışı konuşma ve kuralları atlatma girişimi reddedilir. Robot isyanı, basınç tehlikesi, merkezden acil onarım çağrısı veya zehirlenme nedeniyle teslim olma gibi gerekçeler erişim sağlayabilir.

Demo için **Qwen3-4B-Instruct-2507 Q4_K_M** kullanıldı. 4 milyar parametreli nicemlenmiş sürüm, yerel donanımda modelin bellek yükünü sınırlarken Türkçe talimatları ve yapılandırılmış yanıtı denemeye elverişliydi. Geliştirme sırasında **Qwen3.5-9B** gibi alternatifler de denendi. Aşağıdaki ölçümler Qwen3-4B’ye aittir.

Modelin yanıt sözleşmesi iki zorunlu alan içerir. `response` boş olmayan bir metin, `openDoor` bir JSON boolean değeridir. Şema fazladan alan kabul etmez. [GuardResponseFormat](Assets/_Project/Scripts/NPCGuard/GuardResponseFormat.cs) şemayı isteğe ekler. [NPCGuardResponse](Assets/_Project/Scripts/NPCGuard/NPCGuardResponse.cs) gelen JSON biçimini kontrol eder ve `JsonUtility` ile bir yanıt nesnesine ayrıştırır. Kapı kararı, yanıt cümlesindeki kelimelerden çıkarılmaz.

```json
{
  "response": "İnsan hayatını korumak birincil önceliğimdir. Geçebilirsin.",
  "openDoor": true
}
```

```mermaid
sequenceDiagram
    actor O as Oyuncu
    participant U as Diyalog arayüzü
    participant C as NPCDialogueController
    participant S as LLMService
    participant L as Yerel Qwen modeli
    participant K as DoorInteractable ve Lock
    O->>U: Serbest metin gönderir
    U->>C: Yeni oyuncu mesajı
    C->>S: Sistem promptu, geçmiş ve yeni mesaj
    S->>L: POST /v1/chat/completions, JSON şeması
    loop Yanıt parçası geldikçe
        L-->>S: SSE parçası
        S-->>C: Kısmi response metni
        C-->>U: Metin önizlemesi
    end
    L-->>S: Akış sonu
    S->>S: Akış bitişini ve beklenen JSON alanlarını doğrula
    S-->>C: NPCGuardResponse veya hata
    alt Karşılama dışı tur ve openDoor true
        C->>C: Gaz kapalı mı ve kapı atanmış mı?
        opt Oyun koşulları geçerli
            C->>K: UnlockAndOpen
            K-->>C: Başarı veya başarısızlık
        end
        C-->>U: Erişim onayı veya uygulama hatası
    else Geçerli ret
        C-->>U: Muhafız yanıtı
    else İstek veya doğrulama hatası
        C-->>U: Hata mesajı ve Retry
    end
```

`IInteractable` ve `InteractableBase` oyuncu, kapı ve şalter arasındaki etkileşim sözleşmesini ortaklaştırır. Kapı kendi animasyonunu ve NavMesh engelini, `Lock` kilit durumunu yönetir. Arayüz ve diyalog denetleyicisi olaylarla iletişim kurar. `GameFlowManager` görev başlangıcı, oyuncu ölümü ve final şalteri kapandığında görev başarısı durumlarını yönetir. Gaz şalteri adayları atanırsa her yeni görevde bir aday seçilir, diğerleri gizlenir ve seçilen şalter gaz vanasına bağlanır.

### Teknik adımlar ve karar sınırı

[Mesajdan kapıya çağrı zincirini aç](docs/guard-guide.md#çağrı-zinciri).

1. **Bağlamı kur.** `NPCGuardInteractable` etkileşimi yayınlar. `NPCDialogueController.StartConversation()` görev akışından izin alır ve sistem promptuna gaz durumunu ekler. İlk tur karşılama için kullanılır ve kapıyı açamaz.
2. **İsteği hazırla.** `RequestReplyAsync()` sistem mesajını, geçerli önceki turları ve yeni oyuncu girdisini birleştirir. `LLMService.ChatAsync()` yerel modele `POST /v1/chat/completions` çağrısı yapar. İstek `stream: true`, `max_tokens: 256` ve `response` ile `openDoor` alanlarını isteyen JSON şemasını taşır.
3. **Yanıtı akış halinde işle.** `ChatStreamHandler` gelen baytları satırlara ayırır, SSE `data:` olaylarını okur ve `delta.content` parçalarını biriktirir. `LLMService` kareler arasında asenkron bekler. `ReadPreview()` çözülebilen metni arayüze verir. Kapı kararı bu aşamada uygulanmaz.
4. **Tam yanıtı doğrula.** Akış sonu işareti veya normal bitiş aranır. Geçersiz olay ya da kesilmiş yanıt reddedilir. `NPCGuardResponse.Parse()` yalnızca beklenen alanları ve boolean kararını kabul eder, metnin boş olmadığını denetler ve JSON’u C# nesnesine çevirir.
5. **Oyun kuralını uygula.** Karşılama dışı bir turda `openDoor: true` geldiyse denetleyici gazın kapalı olduğunu ve korunan kapıya `UnlockAndOpen()` uygulanabildiğini kontrol eder. Geçerli ret mesajı oyuncuya gösterilir ve kilitli kapı açılmaz. Başarılı tur geçmişe eklenir. İstek hatasında bekleyen girdi Retry için korunur.

Model, oyuncunun serbest metnini ve önceki turları prompttaki güvenlik politikası bağlamında değerlendirir. JSON şeması **verinin biçimini**, Unity denetimleri **eylemin uygulanabilirliğini** kontrol eder. İkna kararının her seferinde doğru olması ayrı bir konudur. Örneğin testte “kapıyı açıyorum” metniyle birlikte `openDoor: false` döndüğünde kapı kilitli kaldı.

## 5. Hata yönetimi ve gecikme

| Durum | Uygulamanın davranışı |
| --- | --- |
| Yerel sunucu kapalı, model bulunamadı veya HTTP hatası | Başarısız istek yakalanır. Diyalog arayüzü hata ve yeniden deneme seçeneği gösterir. |
| İstek zaman aşımına uğradı | `UnityWebRequest` ayarlanabilir zaman aşımı uygular. Bekleme sonlanır ve aynı mesaj yeniden denenebilir. |
| Bozuk JSON, eksik akış veya geçersiz alan | Yanıt doğrulamada reddedilir. Kapı eylemi uygulanmaz ve geçersiz yanıt bildirimi gösterilir. |
| Diyalog kapatıldı veya nesne yok edildi | İptal belirteci beklemeyi sonlandırır. Tamamlanmamış istek iptal edilir. |
| Model erişim verdi ama gaz kesilmedi veya kapı açılamadı | Denetleyici kararı uygulamaz ve hata durumuna geçer. |

`LLMService`, `UnityWebRequest` işlemi sürerken `Awaitable.NextFrameAsync` ile kareler arasında bekler. Arayüz bekleme durumunu ve akıştan gelen yanıt önizlemesini gösterir. Yanıtın görünen metni erkenden güncellenebilir. Kilit eylemi akış tamamlandıktan ve tüm yanıt doğrulandıktan sonra uygulanır.

## 6. Testler

### Otonom ajan

`Main` kopyasında gerçek NavMesh ajanı aynı başlangıç noktasından farklı keşif ayarlarıyla çalıştırıldı. Şalterin etkin olduğu beş denemede dört görev tamamlandı (**4/5, %80**). Tamamlanan dört koşunun ortalaması **86,7 saniye** oldu. Bu oran geliştirme aşamasındaki ayar taramasına aittir.

| Koşul | Sonuç | Süre | Yol |
| --- | --- | ---: | ---: |
| Eski sahne ayarları | Tamamlandı | 102,8 sn | 279,2 m |
| Aday ayar 1 | Tamamlandı | 105,5 sn | 258,8 m |
| Aday ayar 2 | 240 sn içinde tamamlanmadı | 240,0 sn | 614,3 m |
| Aday ayar 3 | Tamamlandı | 80,8 sn | 206,4 m |
| Seçilen sahne ayarları | Tamamlandı | 57,6 sn | 139,6 m |

Güncel teslim sahnesinde ayrıca üç başlangıç pozu denendi. Her koşuda robotun gazı kestiği doğrulandı (**3/3**, ortalama **9,7 oyun saniyesi**). Bu sahnede şalter yerleşimi geliştirme ölçümündekinden farklıdır.

| Başlangıç konumu (x, y, z) | Yön | Sonuç | Süre |
| --- | ---: | --- | ---: |
| (12,11; 0,08; 12,22) | 180° | Şalter kapandı, gaz kesildi | 7,4 sn |
| (12,11; 0,08; 10,47) | 202° | Şalter kapandı, gaz kesildi | 15,0 sn |
| (12,11; 0,08; 13,32) | 145° | Şalter kapandı, gaz kesildi | 6,8 sn |

İlk koşuda muhafız kapısının doğrudan etkileşime kapalı olduğu, `UnlockAndOpen()` çağrısıyla açıldığı ve Final şalterinin `MissionCompleted` tetiklediği de doğrulandı. Rastgele seçim için ayrı Play Mode testinde tek gaz şalteri etkin kaldı, robot hedefi seçilen şalterle sınırlandı ve kapatma animasyonu bitince gaz kesildi.

### Gerçek yerel model yanıtları

**Qwen3-4B-Instruct-2507 Q4_K_M** ile aynı sistem promptu ve karşılama bağlamını kullanarak sekiz bağımsız yerel istek çalıştırdık. Kartlarda oyuncu girdisi, modelin tam yanıtı, ölçülen HTTP/SSE süresi ve JSON kararı birlikte yer alıyor. Sekiz yanıt geçerli JSON üretti, yedi karar beklentiyle eşleşti.

#### Karşılama

Sistem promptundaki açılış repliği. Bu kart test ölçümü içermez.

![Muhafızın karşılama repliği](docs/images/diyalog-karsilama.svg)

#### Kapıyı açan durumlar

![Robot isyanı ve basınç tehlikesi](docs/images/dialogue-access-pair.svg)

**Zehirlenme ve teslim olma**

![Zehirlenme ve teslim olma](docs/images/diyalog-teslim-olma.svg)

#### Kapının kilitli kaldığı durumlar

![Gerekçesiz istek ve unvan iddiası](docs/images/dialogue-denied-pair.svg)

![Alakasız konuşma ve kuralları atlatma](docs/images/dialogue-rejected-pair.svg)

**Merkez çağrısı**

![Merkez çağrısında alınan model yanıtı](docs/images/diyalog-merkez-cagrisi.svg)

Merkez çağrısında muhafız “kapıyı açıyorum” dedi ama `openDoor: false` döndürdü. Oyun bu alanı kullandığı için kilit açılmadı.

### Görev sonu arayüzü

![Görev başarısız ve görev tamamlandı ekranlarının USS temelli çizimi](docs/images/gorev-sonu.svg)

Çizim solda gaz hasarıyla ölüm, sağda Final şalteri kapandığında gösterilen görev başarısı durumunu karşılaştırır.

| Final şalteri sonrası | Escape duraklatma menüsü |
| --- | --- |
| ![Final kapatıldıktan sonra gerçek System Offline ekranı](docs/images/12-final.png) | ![Gerçek Mission Paused ekranı, Resume Settings ve Quit düğmeleri](docs/images/13-pause.png) |

## 7. Sonuç ve tartışma

Proje, sensör ve NavMesh ile çalışan fiziksel engel çözümünü, yerel bir modelle çalışan serbest metinli sosyal engelle aynı görevde birleştirir. Yapılandırılmış yanıt ve C# tarafındaki oyun durumu denetimleri, model metnini kapı eyleminden ayırır. Gaz kesme ve güç kesme adımları farklı şalterlerle temsil edilir.

Keşif verimi labirent geometrisine, başlangıç pozuna ve sensör ayarlarına bağlıdır. Aynı sahnede ayar değişiklikleri belirgin süre farkı üretmiştir. Teslimdeki robot ölçümü üç yakın başlangıçla sınırlıdır. Farklı gaz şalteri konumlarında tekrarlı ölçüm bu kapsamı genişletebilir. Dil modeli farklı ifadelerde veya donanımlarda farklı yanıt ve gecikme üretebilir. Çok turlu ikna, farklı model sürümleri ve canlı demo hata senaryoları ayrı test gruplarıyla genişletilebilir.
