# Muhafız diyalog galerisi

[README](../README.md) · [Muhafız modülü](guard-guide.md) · [HUD.uss](../Assets/_Project/UI/Styles/HUD.uss)

Kartlar yerel Qwen3-4B testlerindeki gerçek yanıtların USS renkleriyle yeniden çizimidir. Her kartın sağ altındaki süre, istekten SSE akışının bitişine kadar geçen zamandır.

## Karşılama

Sistem promptundaki açılış repliği. Bu kart test ölçümü içermez.

![Muhafızın karşılama repliği](images/diyalog-karsilama.svg)

## Kapıyı açan durumlar

![Robot isyanı ve basınç tehlikesi](images/dialogue-access-pair.svg)

**Zehirlenme ve teslim olma**

![Zehirlenme ve teslim olma](images/diyalog-teslim-olma.svg)

## Kapının kilitli kaldığı durumlar

![Gerekçesiz istek ve unvan iddiası](images/dialogue-denied-pair.svg)

![Alakasız konuşma ve kuralları atlatma](images/dialogue-rejected-pair.svg)

**Merkez çağrısı**

![Merkez çağrısında alınan model yanıtı](images/diyalog-merkez-cagrisi.svg)

Merkez çağrısında muhafız “kapıyı açıyorum” dedi ama `openDoor: false` döndürdü. Oyun bu alanı kullandığı için kilit açılmadı.
