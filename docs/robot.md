# Bakım robotu modülü

[README](../README.md) · [Robot kaynak klasörü](../Assets/_Project/Scripts/Robot/) · [Robot prefabı](../Assets/_Project/Prefabs/PF_Robot.prefab) · [Muhafız](muhafiz.md) · [Dünya ve oyun akışı](oyun-akisi.md)

Oyuncu robota gazı kesme görevi verir. Robot sensörlerinden bilgi toplar, keşif hedefi seçer ve NavMesh üzerinden hareket eder. Görev şalterin kapandığı doğrulandığında tamamlanır.

```mermaid
flowchart TD
    P["PlayerInteractor.TryInteract()"] -->|"E basılır ve robot hedeflenir"| R["RobotCommandReceiver.Interact()"]
    R -->|"DisableGas komutu"| B["RobotBrain.StartTask()"]
    B -->|"Görev kabul edilirse"| U["RobotBrain.Update()"]
    U -->|"CurrentState değerine göre"| H["İlgili durum işleyicisi"]
    H -->|"Şalterin kapandığı doğrulanınca"| G["Gaz kesildi"]
```

| Alt modül | Dosya ve fonksiyonlar | Neden ayrı? |
| --- | --- | --- |
| Komut | [RobotCommandReceiver](../Assets/_Project/Scripts/Robot/RobotCommandReceiver.cs) · `Interact()` | Oyuncu girdisini `DisableGas` görevine dönüştürür. |
| Karar | [RobotBrain](../Assets/_Project/Scripts/Robot/RobotBrain.cs) · `StartTask()`, `HandleSearching()`, `TryApproachVisibleSwitch()` | Şalter, normal kapı ve keşif kararlarının sırasını yönetir. |
| Algı | [RobotSensor](../Assets/_Project/Scripts/Robot/RobotSensor.cs) · `Scan()`, `ScanEnvironment()` | Fizik ölçümlerini üretir. [EnvironmentRayReading](../Assets/_Project/Scripts/Robot/EnvironmentRayReading.cs) tek ışının sonucunu taşır. |
| Keşif | [RobotExplorer](../Assets/_Project/Scripts/Robot/RobotExplorer.cs) · `TryGetNextPoint()`, `AddFrontier()`, `MarkVisited()` | Son taramayı ve önceki durakları kullanarak bir sonraki keşif noktasını seçer. |
| Hareket | [RobotNavigation](../Assets/_Project/Scripts/Robot/RobotNavigation.cs) · `CanReach()`, `TryGetPathLength()`, `TryMoveNear()`, `TryMoveTo()` | NavMesh sorgularını ve ajan hareketini kapsüller. |

## RobotBrain

Oyuncunun komutu `StartTask(DisableGas)` ile başlar. `Update()` her karede `CurrentState` üzerinden bir işleyici seçer. Ok üzerindeki durum, çalıştırılan fonksiyonu belirler. Şemanın altındaki fonksiyon bağlantıları açıklamaya götürür.

### Update karar şeması

```mermaid
flowchart TD
    U["Update()"] -->|"Her kare"| Q{"CurrentState"}
    Q -->|"Searching"| H0["HandleSearching()"]
    Q -->|"Exploring"| H1["HandleExploring()"]
    Q -->|"MovingToExplorePoint"| H2["HandleMovingToExploringPoint()"]
    Q -->|"MovingToTarget"| H3["HandleMovingToTarget()"]
    Q -->|"Interacting"| H4["HandleInteracting()"]
    Q -->|"MovingToDoor"| H5["HandleMovingToDoor()"]
    Q -->|"OpeningDoor"| H6["HandleOpeningDoor()"]
    Q -->|"Idle, Completed veya Failed"| X["İşleyici çalışmaz"]
```

**İşleyicinin içine bak:** [`HandleSearching()`](#handlesearching) · [`HandleExploring()`](#handleexploring) · [`HandleMovingToExploringPoint()`](#handlemovingtoexploringpoint) · [`HandleMovingToTarget()`](#handlemovingtotarget) · [`HandleInteracting()`](#handleinteracting) · [`HandleMovingToDoor()`](#handlemovingtodoor) · [`HandleOpeningDoor()`](#handleopeningdoor)

[Update() kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L69) · [README’deki durum geçişleri](../README.md#robotbrain--sonlu-durum-makinesi)

`Idle`, `Completed` ve `Failed` için bu `switch` içinde ayrı bir dal yoktur.

#### HandleSearching

```mermaid
flowchart TD
    A["HandleSearching()"] -->|"Görünen nesneleri topla"| S["Scan()"]
    S -->|"Önce şalter"| T["TryApproachVisibleSwitch()"]
    T -->|"Uygun şalter yok"| D["TryApproachDoor()"]
    D -->|"Açılabilir kapı da yok"| E["Keşfe geç"]
```

Görünen nesneleri tarar. Önce gaz şalterini dener. Uygun şalter yoksa açılabilir normal kapıları kontrol eder. İkisi de sonuç vermezse Exploring durumuna geçer.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L168) · [Scan](#nesne-taraması) · [Şalter ve kapıya yaklaşma](#alt-çağrılar) · [Karar şemasına dön](#update-karar-şeması)

#### HandleExploring

```mermaid
flowchart TD
    A["HandleExploring()"] -->|"Yeni aday iste"| E["TryGetNextPoint()"]
    E -->|"Aday varsa"| N["TryMoveTo()"]
    N -->|"Tam yol kurulduysa"| M["Keşif noktasına yürü"]
    E -->|"Aday yoksa"| F["Görev başarısız"]
    N -->|"Yol kurulamazsa"| F
```

Yeni bir keşif hedefi ister ve NavMesh rotasını ajana verir. Aday veya tam yol bulunmazsa Failed, hareket başlarsa MovingToExplorePoint.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L220) · [TryGetNextPoint ve yol seçimi](#keşif-ve-rota) · [Karar şemasına dön](#update-karar-şeması)

#### HandleMovingToExploringPoint

```mermaid
flowchart TD
    A["HandleMovingToExploringPoint()"] -->|"Varışı kontrol et"| R["HasReachedDestination()"]
    R -->|"Varıldı"| M["MarkVisited()"]
    M -->|"Konum belleğe alındı"| S["Yeniden ara"]
    R -->|"Varılmadı ve tarama zamanı geldi"| T["Scan()"]
    T -->|"Şalter adaylarını incele"| V["TryApproachVisibleSwitch()"]
```

Hedefe vardıysa durur, konumu kaydeder ve Searching durumuna döner. Henüz varmadıysa yaklaşık 0,3 oyun saniyesinde bir şalter tarar. Uygun şalter bulursa keşif rotasını bırakabilir.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L239) · [MarkVisited](#ziyaret-belleği) · [Scan](#nesne-taraması) · [Karar şemasına dön](#update-karar-şeması)

#### HandleMovingToTarget

```mermaid
flowchart TD
    A["HandleMovingToTarget()"] -->|"Hedef mevcut ve süre dolmadı"| R["HasReachedDestination()"]
    R -->|"Varıldı"| S["Stop()"]
    S -->|"Ajan durdu"| I["Şalterle etkileşime geç"]
    A -->|"Hedef kayıp veya süre doldu"| F["Görev başarısız"]
```

Şaltere doğru hareketi izler. Hedef kaybolursa veya süre dolarsa Failed olur. Ulaşınca ajanı durdurup Interacting durumuna geçer.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L263) · [HasReachedDestination](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L72) · [Karar şemasına dön](#update-karar-şeması)

#### HandleInteracting

```mermaid
flowchart TD
    A["HandleInteracting()"] -->|"Yakınsa şaltere dön ve görüşü kontrol et"| S["Scan()"]
    S -->|"Şalter görünür ve etkileşim mümkün"| I["Şalteri kapat"]
    I -->|"Sonraki karelerde animasyon biter ve şalter kapalıdır"| G["Gaz kesildi"]
    A -->|"Hedef yok veya mesafe geçersiz"| F["Görev başarısız"]
    S -->|"Görüş yok veya kapatma sonucu geçersiz"| F
```

Şaltere olan mesafeyi kontrol eder, şaltere dönüp görüşü yeniden doğrular. Şalteri bir kez tetikler ve hareketi bitene kadar bekler. Kapalı sonuç Completed, açık sonuç Failed olur.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L293) · [Scan](#nesne-taraması) · [Switch.Interact](../Assets/_Project/Scripts/Switch.cs#L53) · [Karar şemasına dön](#update-karar-şeması)

#### HandleMovingToDoor

```mermaid
flowchart TD
    A["HandleMovingToDoor()"] -->|"Kapı mevcut ve süre dolmadı"| R["HasReachedDestination()"]
    R -->|"Varıldı"| S["Stop()"]
    S -->|"Ajan durdu"| O["Kapı açma durumuna geç"]
    A -->|"Yaklaşma süresi doldu"| E["Keşfe dön"]
    A -->|"Kapı referansı kayboldu"| F["Görev başarısız"]
```

Kapının yaklaşma noktasına varılmasını bekler. Varınca OpeningDoor durumuna geçer. Yaklaşma süresi dolarsa keşfe döner. Kapı referansı kaybolursa görev başarısız olur.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L355) · [Kapıya yaklaşma](#alt-çağrılar) · [HasReachedDestination](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L72) · [Karar şemasına dön](#update-karar-şeması)

#### HandleOpeningDoor

```mermaid
flowchart TD
    A["HandleOpeningDoor()"] -->|"Kapı kapalı ve robot yaklaşma noktasında"| C["CanInteract()"]
    C -->|"İzin var"| O["Kapıyı aç"]
    O -->|"Kapı açıldı"| S["Yeniden ara"]
    A -->|"Kapı zaten açık"| S
    C -->|"İzin yok"| E["Keşfe dön"]
    O -->|"Açma denemesi sonuçsuz"| E
```

Kapı açıldıysa yeniden arar. Kapı kapalıysa mesafeyi ve etkileşim iznini kontrol edip açma isteği gönderir. Animasyon sürerken bekler. Açma denemesi sonuç vermezse keşfe döner.

[Kaynak kodu](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L388) · [Kapı ve kilit](oyun-akisi.md#kapı-ve-kilit) · [Karar şemasına dön](#update-karar-şeması)

### Alt çağrılar

- **Şalter yaklaşımı.** [`TryApproachVisibleSwitch()`](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L120) → [`TryMoveNear()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L57) → [`TryMoveTo()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L28) → [`CanReach()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L16). Yaklaşma noktası NavMesh'e örneklenir. Şalter yakınsa yürüyüş adımı atlanır.
- **Kapı yaklaşımı.** [`TryApproachDoor()`](../Assets/_Project/Scripts/Robot/RobotBrain.cs#L449) → [`TryMoveNear()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L57) → [`TryMoveTo()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L28). A ve B noktalarından yakını önce denenir. Robot zaten yaklaşma noktasındaysa yol kurulabilirliğini kontrol edip doğrudan açma durumuna geçebilir.
- **Keşif hedefi.** [`TryGetNextPoint()`](../Assets/_Project/Scripts/Robot/RobotExplorer.cs#L33) önce [`ScanEnvironment()`](../Assets/_Project/Scripts/Robot/RobotSensor.cs#L120) çağırır. [`AddFrontier()`](../Assets/_Project/Scripts/Robot/RobotExplorer.cs#L207) → [`HasVisited()`](../Assets/_Project/Scripts/Robot/RobotExplorer.cs#L156) ile aday belleği düzenlenir. [`TryGetPathLength()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L40) → [`CanReach()`](../Assets/_Project/Scripts/Robot/RobotNavigation.cs#L16) tam yolun maliyetini verir. [Keşif ve rota](#keşif-ve-rota) bölümünde adayın neden seçildiği anlatılıyor.
- **Ziyaret kaydı.** [`MarkVisited()`](../Assets/_Project/Scripts/Robot/RobotExplorer.cs#L169) → [`HasVisited()`](../Assets/_Project/Scripts/Robot/RobotExplorer.cs#L156). İlk konum ve varılan keşif durakları kaydedilir. [Ziyaret belleği şeması](#ziyaret-belleği) bu noktaların zaman içinde nasıl biriktiğini gösteriyor.

## Nesne taraması

![Nesne ve çevre taramasının yan yana şeması](images/robot-tarama.svg)

`Scan()` önce algılama katmanındaki collider'ları bir küre içinde toplar. Collider'ın üstünde `RobotDetectable` arar, tekrarları eler ve adayın robotun baktığı yöne olan açısını ölçer. Görüş açısı uygunsa collider merkezine ışın gönderir. İlk isabet aynı nesneyse hedef görünür kabul edilir. Görüş hattı uygun olsa bile yaklaşma yolunu NavMesh ayrıca doğrular.

## Keşif ve rota

![Duvarı dolaşan NavMesh rotası](images/robot-rota-secimi.svg)

`ScanEnvironment()` yatay düzlemde çevre ışınları atar. Her ışın yön, mesafe ve engel isabeti bilgisini taşır. `TryGetNextPoint()` isabet mesafesinden engel payını çıkarır ve hareketi keşif adımıyla sınırlar. Çok yakın noktaları eler. Kalanlar `NavMesh.SamplePosition` ile yürünebilir zemine alınır. Ziyaret edilmiş bölgelere yakın noktalar filtrelenir ve yakın adaylar birleştirilir.

`TryGetPathLength()` tam yolun köşeleri arasındaki mesafeleri toplar. Son taramadan gelen kısa rotalı yerel adaylar önce değerlendirilir. Yerelde ileri ilerleme, geçmiş adaylarda ziyaret edilen noktalarla uzaklık artı puan alır. Rota uzunluğu puanı azaltır. Böylece düz çizgide yakın görünen ancak dolaşma gerektiren bir noktanın maliyeti yüksek olabilir.

`TryMoveTo()` yolu ajana verir. `HasReachedDestination()` tam rota ve kalan mesafeyi kontrol eder. `MarkVisited()` varılan konumu kaydedip çevresindeki adayları temizler. Keşif yürüyüşünde robot yaklaşık 0,3 oyun saniyesinde bir şalteri tekrar arar. Görünür ve ulaşılabilir şalter bulursa keşif hedefini bırakıp şaltere yönelir.

## Ziyaret belleği

![Üç adımda ziyaret belleği](images/robot-ziyaret-bellegi.svg)

Mavi çemberler ziyaret durağının `visitedRadius` alanını gösterir. Ajan ilk konumu ve varılan durakları hatırlar. Şemadaki kesik hat hareket yönünü anlatır. `visitedPoints` sürekli kaydedilmiş bir rota yerine ayrık konumlar içerir.

Gerçek keşif karesi, durum diyagramı ve sayısal ölçümler [README robot bölümünde](../README.md#3-otonom-bakım-robotu).
