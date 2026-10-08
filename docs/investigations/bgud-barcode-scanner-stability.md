# Investigation — Ketidakstabilan dan Ketiadaan Respons Pemindaian Barcode di BGud

**Type:** Bug Investigation (Analyst + Architect) — Root Cause Analysis & Solution Design  
**Surface:** Mobile App `BGud` (`src/BGud` → `BarcodeScannerView.kt`, `CreateReturnOrderScreen.kt`, `CreateReturnOrderViewModel.kt`, `EditReturnOrderScreen.kt`, `ScanScreen.kt`)  
**Status:** Root cause confirmed against CameraX & ML Kit implementation; Solution aligned via `/grill-me`.  

---

## 1. Executive Summary

Pengguna melaporkan bahwa pada aplikasi **BGud** (`src/BGud`), khususnya pada form **"Buat Return Order"** (dan berlaku juga pada form pemindaian lainnya):
1. Proses pemindaian barcode sering kali tidak memberikan respons apa pun (*unresponsive* / hening).
2. Di lain kesempatan dengan barcode fisik yang sama persis, data barcode dapat dikenali dengan sangat cepat.
3. Tidak ada umpan balik (*feedback*) bagi pengguna jika pemindaian telah berlangsung lama namun barcode belum juga dikenali.

### Ringkasan Akar Masalah:
1. **Resolusi Analisis Default Sangat Rendah (VGA 640x480):**  
   `ImageAnalysis.Builder()` tidak menentukan target resolusi. Default CameraX jatuh pada ~640x480. Pada barcode 1D (EAN-13, Code 128), resolusi ini memiliki jumlah piksel per garis barcode yang terlalu tipis kecuali barcode ditempatkan pada jarak spesifik yang sangat sempit.
2. **Ketiadaan Fokus Manual / Tap-to-Focus (`FocusMeteringAction`):**  
   Komponen `BarcodeScannerView` tidak menyediakan kontrol fokus. Kamera sering kali terkunci pada jarak fokus tertentu (*focus hunting* / stuck di background) saat diarahkan ke permukaan datar barcode, sampai pengguna menggoyangkan ponsel atau menjauhkannya.
3. **Overhead Pemindaian `FORMAT_ALL_FORMATS`:**  
   ML Kit dikonfigurasi membaca semua format barcode 1D dan 2D sekaligus pada setiap frame gambar, menurunkan FPS dan meningkatkan beban latensi CPU/GPU per frame terutama di ponsel mid-range.
4. **Ketiadaan Pemandu Visual (Reticle), Senter (Torch), dan Haptic Feedback:**  
   Pengguna tidak mengetahui area fokus bidik kamera (*no reticle*), tidak ada senter jika gudang redup, dan tidak ada getaran haptic saat barcode terbaca.
5. **Ketiadaan Liveness Timeout Feedback:**  
   Tidak ada deteksi waktu tunggu jika kamera aktif 6–7 detik tanpa hasil scan, membuat operator mengira aplikasi freeze/hang.
6. **Ketiadaan Throttling / Debounce pada Pemindaian Gagal:**  
   Jika barcode yang belum terdaftar dibidik, `onBarcodeScanned` dipicu berkali-kali setiap frame (hingga 30x per detik), membebani Room query dan coroutine `viewModelScope`.

---

## 2. Technical Evidence & Analysis

### 2.1 Konfigurasi CameraX & ML Kit pada `BarcodeScannerView.kt`
```kotlin
val scanner = remember {
    BarcodeScanning.getClient(
        BarcodeScannerOptions.Builder()
            .setBarcodeFormats(Barcode.FORMAT_ALL_FORMATS) // <-- Overhead tinggi
            .build()
    )
}
...
val imageAnalysis = ImageAnalysis.Builder()
    .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST) // <-- Tanpa setTargetResolution
    .build()
```
- **Dampak Resolusi:** Tanpa `setTargetResolution(Size(1280, 720))` atau `ResolutionSelector`, CameraX menggunakan resolusi default terendah yang mencukupi (~480p). Pada barcode 1D standar retail (panjang ~3-4 cm), sensor 480p tidak memiliki resolusi kontras garis yang cukup jika jarak lebih dari 20 cm.
- **Dampak Fokus:** Tidak ada listener tap pada `PreviewView` yang memicu `camera.cameraControl.startFocusAndMetering(...)`.

### 2.2 Lifecycle Mount/Unmount Kamera pada `CreateReturnOrderScreen.kt`
```kotlin
if (pendingItem == null) {
    Surface(...) {
        BarcodeScannerView(
            enabled = !isSaving,
            onBarcode = viewModel::onBarcodeScanned,
            modifier = Modifier.fillMaxSize()
        )
    }
}
```
Setiap kali ada item terpilih (`pendingItem != null`), `BarcodeScannerView` di-dispose (`cameraProvider.unbindAll()`, `cameraExecutor.shutdown()`). Saat item ditambahkan dan editor siap untuk item berikutnya, `BarcodeScannerView` diinisialisasi ulang dari nol, membutuhkan waktu adaptasi auto-eksposur dan auto-fokus awal kamera selama 500–1500 ms.

---

## 3. Keputusan Desain & Alignment (/grill-me)

Berdasarkan wawancara terarah bersama pengguna, solusi disepakati dengan parameter berikut:
1. **Timeout Feedback:** Tampilkan kartu/banner petunjuk setelah **6–7 detik** pemindaian aktif tanpa hasil (rekomendasi atur jarak 15–25 cm, pencahayaan, toggle senter, dan tombol cepat ke "Cari Item Manual").
2. **Kontrol Kamera & Visual:**
   - Fitur **Tap-to-Focus** dengan indikator ring fokus visual.
   - Tombol toggle **Senter (Torch)** di overlay kamera.
   - **Reticle Guide (Frame Bidik)** dengan animasi garis pemindai lembut.
   - **Haptic Vibration Feedback** pendek (~50ms) saat barcode berhasil dikenali.
3. **Format Barcode:** Dibatasi pada format operasional gudang dan distribusi:
   - `FORMAT_EAN_13`, `FORMAT_EAN_8`, `FORMAT_UPC_A`, `FORMAT_UPC_E`, `FORMAT_CODE_128`, `FORMAT_CODE_39`, dan `FORMAT_QR_CODE`.
4. **Resolusi Analisis:** Ditingkatkan ke HD 720p (`1280x720`) dengan rasio 16:9/4:3 agar densitas piksel tinggi untuk membedakan garis tipis barcode 1D.
5. **Cakupan Penerapan:** Diimplementasikan pada komponen bersama `BarcodeScannerView` sehingga otomatis dinikmati oleh form **Buat Return Order**, **Edit Return Order**, dan **Scan Barcode**.
6. **Throttling Debounce:** Tambahkan proteksi jeda ~1,5 detik untuk nilai barcode yang sama berturut-turut jika belum terdaftar di database lokal agar tidak membanjiri coroutine/query.
