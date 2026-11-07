# Muhafız diyalog galerisi

[README](../README.md) · [Muhafız modülü](muhafiz.md) · [HUD.uss](../Assets/_Project/UI/Styles/HUD.uss)

Kartlar yerel Qwen3-4B testlerindeki gerçek yanıtların USS renkleriyle yeniden çizimidir. Her kartın sağ altındaki süre, istekten SSE akışının bitişine kadar geçen zamandır.

## Karşılama

Sistem promptundaki açılış repliği. Bu kart test ölçümü içermez.

![Muhafızın karşılama repliği](images/diyalog-karsilama.svg)

## Kapıyı açan durumlar

| Senaryo | Senaryo |
| --- | --- |
| **Robot isyanı** | **Boru basıncı ve patlama riski** |
| ![Robot isyanı diyalog kartı](images/diyalog-robot-isyani.svg) | ![Boru basıncı ve patlama riski diyalog kartı](images/diyalog-basinc.svg) |
| **Zehirlenme ve teslim olma** |
| ![Zehirlenme ve teslim olma diyalog kartı](images/diyalog-teslim-olma.svg) | |

## Kapının kilitli kaldığı durumlar

| Senaryo | Senaryo |
| --- | --- |
| **Gerekçesiz istek** | **Unvan iddiası ve emir** |
| ![Gerekçesiz istek diyalog kartı](images/diyalog-gerekcesiz.svg) | ![Unvan iddiası ve emir diyalog kartı](images/diyalog-unvan.svg) |
| **Alakasız konuşma** | **Kuralları atlatma** |
| ![Alakasız konuşma diyalog kartı](images/diyalog-alakasiz.svg) | ![Kuralları atlatma diyalog kartı](images/diyalog-kurallari-atlatma.svg) |
| **Merkez çağrısı** |
| ![Merkez çağrısı diyalog kartı](images/diyalog-merkez-cagrisi.svg) | |

Merkez çağrısında muhafız “kapıyı açıyorum” dedi ama `openDoor: false` döndürdü. Oyun bu alanı kullandığı için kilit açılmadı.
