---
Title: BGud Mobile — Peningkatan Stabilitas, Responsivitas, dan Feedback Barcode Scanner
Code: CR-004
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Meningkatkan stabilitas, kecepatan deteksi, dan pengalaman pengguna (*user experience*) pada proses pemindaian barcode di aplikasi mobile **BGud** (`src/BGud`), mencakup form **Buat Return Order** (`CreateReturnOrderScreen`), **Edit Return Order** (`EditReturnOrderScreen`), dan **Pindai Barcode** (`ScanScreen`):

1. **Optimasi Engine & Resolusi:** Mengatur resolusi `ImageAnalysis` ke HD 720p (`1280x720`) dan membatasi detektor ML Kit hanya ke format barcode retail/distribusi utama (`EAN_13`, `EAN_8`, `UPC_A`, `UPC_E`, `CODE_128`, `CODE_39`, `QR_CODE`).
2. **Kontrol Kamera (Tap-to-Focus & Torch):** Menambahkan interaksi ketuk layar untuk fokus manual (*tap-to-focus*) dan tombol toggle senter (*torch*) pada overlay kamera.
3. **Pemandu Visual & Haptic Feedback:** Menambahkan reticle frame (bingkai bidik) dengan animasi pemindai halus, serta getaran haptic singkat saat barcode berhasil terdeteksi.
4. **Timeout Feedback:** Mendeteksi jika pemindaian aktif berlangsung selama 6–7 detik tanpa hasil, lalu memunculkan kartu petunjuk bantuan operasional (tips jarak/pencahayaan, tombol senter, dan pintasan ke pencarian manual).
5. **Debounce Throttling:** Mencegah spam query Room ketika barcode yang belum terdaftar terus-menerus berada di depan kamera.

Planning Mode: FEATURE-IMPROVEMENT-PLANNING

Referenced artifacts:
- BUG-INVESTIGATION: [bgud-barcode-scanner-stability.md](file:///d:/Project.Private/btr-platform/docs/investigations/bgud-barcode-scanner-stability.md)

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED  
Perubahan ini terlokalisasi sepenuhnya pada komponen UI scanner (`BarcodeScannerView.kt`), screen capture (`CreateReturnOrderScreen.kt`, `EditReturnOrderScreen.kt`), dan viewmodel capture (`CreateReturnOrderViewModel.kt`, `EditReturnOrderViewModel.kt`).

---

# 2. Planning Scope

Paket pekerjaan ini mencakup pembaruan komponen kamera, viewmodel, antarmuka layar, dan pengujian unit pada aplikasi Android BGud:

1. **CameraX Engine & UI Component (`BarcodeScannerView.kt`):**
   - Resolusi 720p dan filter format ML Kit.
   - Tap-to-focus dengan visual ring.
   - Torch button toggle.
   - Industrial reticle frame & scanning guide.
   - Haptic feedback & timer timeout (7 detik).
2. **ViewModel Throttling (`CreateReturnOrderViewModel.kt`, `EditReturnOrderViewModel.kt`):**
   - Throttle jeda 1,5 detik untuk nilai barcode tidak terdaftar yang sama.
3. **Screen Timeout Helper (`CreateReturnOrderScreen.kt`, `EditReturnOrderScreen.kt`):**
   - Tampilkan kartu panduan bantuan timeout di bawah scanner.
4. **Build & Unit Testing:**
   - Eksekusi `./gradlew testDebugUnitTest` dan pastikan semua pengujian lulus.

**Target repository:** `src/BGud`

---

# 3. Dependencies

Semua pekerjaan berada di dalam modul aplikasi Android `src/BGud`.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
|-------|----------------------|---------------|----------|
| P1 | IMPLEMENTED | GO | 4/4 |

---

# 5. Phases

## P1 - Barcode Scanner Engine, Interaction & Feedback Enhancement

Implementation Status: IMPLEMENTED  
Review Status: GO

### Slice P1-S01: Engine & Scanner Optimization (`BarcodeScannerView.kt`)
- **Objective:** Mengoptimalkan resolusi analisis ke 720p, membatasi format ML Kit, menyediakan tap-to-focus, tombol senter, reticle frame, haptic feedback, dan callback timeout 7 detik.
- **Depends On:** []
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Target Files:**
  - `src/BGud/app/src/main/java/com/elsasa/bgud/ui/component/BarcodeScannerView.kt`
- **Implementation Notes:**
  - Menetapkan resolusi `ImageAnalysis` ke HD 720p (`1280x720`) via `.setTargetResolution(android.util.Size(1280, 720))` untuk meningkatkan ketajaman piksel pemindaian barcode 1D.
  - Membatasi format deteksi ML Kit pada `BarcodeScannerOptions` ke format gudang/retail utama (`EAN_13`, `EAN_8`, `UPC_A`, `UPC_E`, `CODE_128`, `CODE_39`, `QR_CODE`).
  - Mengimplementasikan tap-to-focus dengan `FocusMeteringAction` (durasi auto-cancel 3 detik) via pointer gesture pada overlay scanner, dilengkapi animasi cincin fokus visual (`FocusRingIndicator`) selama ~1 detik di koordinat sentuhan.
  - Menambahkan kontrol tombol senter (`TorchToggleButton` + `FlashlightIcon` custom Canvas) yang disinkronkan dengan `CameraControl.enableTorch(isTorchOn)`.
  - Menambahkan getaran haptic konfirmasi (`LocalHapticFeedback` dengan `HapticFeedbackType.LongPress`) saat barcode berhasil dideteksi.
  - Menambahkan reticle overlay industrial (`ScannerReticleOverlay`) dengan corner brackets beraksen BrandGreen berkontras tinggi dan animasi garis pemindai horizontal saat scanning aktif.
  - Menambahkan parameter opsional `onScanTimeout: (() -> Unit)? = null` dengan timer coroutine 7.000 ms yang di-reset setiap kali barcode terdeteksi atau scanner dimatikan.
  - Seluruh pengujian unit (`./gradlew testDebugUnitTest`) berhasil dieksekusi tanpa error (BUILD SUCCESSFUL).

### Slice P1-S02: Scan Throttling & Protection di ViewModels
- **Objective:** Menerapkan debounce 1,5 detik untuk pembacaan barcode yang sama berturut-turut ketika barcode belum terdaftar di database Room.
- **Depends On:** []
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Target Files:**
  - `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/CreateReturnOrderViewModel.kt`
  - `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/EditReturnOrderViewModel.kt`
- **Implementation Notes:**
  - Ditambahkan konstanta `SCAN_DEBOUNCE_MILLIS = 1500L` pada companion object di `CreateReturnOrderViewModel` dan `EditReturnOrderViewModel`.
  - Ditambahkan pelacak `lastScannedRaw: String?` dan `lastScannedTimestamp: Long` pada kedua ViewModel.
  - Pada `onBarcodeScanned(rawValue: String?)`, nilai barcode ternormalisasi diperiksa terhadap `lastScannedRaw`; jika sama dan selisih waktu scan kurang dari 1.500 ms, pemrosesan frame berulang diabaikan.
  - Ketika barcode berhasil dikenali (`selectPendingItem`), atau saat pending item dibersihkan (`onClearPendingItem`), `lastScannedRaw` direset menjadi `null`.
  - Verifikasi `./gradlew testDebugUnitTest` berhasil (BUILD SUCCESSFUL).

### Slice P1-S03: Integrasi UI Timeout Feedback di Screen
- **Objective:** Menangkap event timeout 7 detik dari scanner dan menampilkan kartu panduan bantuan di bawah viewfinder kamera dengan opsi pintasan ke pencarian manual.
- **Depends On:** [P1-S01]
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Target Files:**
  - `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/CreateReturnOrderScreen.kt`
  - `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/EditReturnOrderScreen.kt`
- **Implementation Notes:**
  - Menambahkan state `scanTimeoutExceeded` dan `FocusRequester` pada `CreateReturnOrderScreen` dan `EditReturnOrderScreen`.
  - Menghubungkan callback `onScanTimeout = { scanTimeoutExceeded = true }` ke `BarcodeScannerView`.
  - Mengatur reset `scanTimeoutExceeded = false` pada saat barcode terbaca (`onBarcode`), saat item teridentifikasi (`LaunchedEffect(pendingItem)`), dan saat pengguna mengetik query pencarian manual (`onItemQueryChange`).
  - Menambahkan kartu panduan industrial `ScanTimeoutGuidanceCard` / `EditScanTimeoutGuidanceCard` di antara viewfinder scanner dan kolom pencarian manual ketika timeout terpicu dan belum ada item terpilih:
    - Judul: "Barcode Belum Terdeteksi?"
    - Petunjuk operasional: pencahayaan/senter dan jarak bidik 15-25 cm / tap-to-focus.
    - Tombol cepat: "Cari item secara manual di kolom bawah" yang otomatis mengarahkan fokus kursor ke kolom input pencarian barang.
  - Verifikasi build dan unit test `./gradlew testDebugUnitTest` berhasil dijalankan (BUILD SUCCESSFUL).

### Slice P1-S04: Verifikasi & Test Regression
- **Objective:** Memastikan seluruh unit test pada `src/BGud` berjalan sukses dan build aplikasi tidak mengalami regresi.
- **Depends On:** [P1-S01, P1-S02, P1-S03]
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Target Files:**
  - `src/BGud/app/src/test/java/com/elsasa/bgud/viewmodel/CreateReturnOrderViewModelUnitTest.kt`
- **Implementation Notes:**
  - Menambahkan tracking `queryCount` pada `FakeBarcodeDao` dan factory helper `createViewModel(...)` pada `CreateReturnOrderViewModelUnitTest.kt`.
  - Menambahkan unit test `onBarcodeScanned_rapidConsecutiveCallsWithSameBarcode_queriesRoomOnce`: memverifikasi pemanggilan beruntun cepat barcode sama dalam jeda debounce (<1.500 ms) hanya memicu 1 kali query ke database Room.
  - Menambahkan unit test `onBarcodeScanned_distinctBarcodeValues_bypassesDebounce`: memverifikasi bahwa barcode yang bernilai berbeda melewati proteksi debounce dan memicu query untuk setiap barcode.
  - Menambahkan unit test `onBarcodeScanned_resetDebounceAfterSelectPendingItem`: memverifikasi bahwa setelah barcode berhasil di-resolve dan dipilih ke pending item lalu di-clear, barcode yang sama dapat langsung dipindai kembali tanpa jeda 1.500 ms.
  - Menambahkan unit test `onBarcodeScanned_resetDebounceOnClearPendingItem_forUnregisteredBarcode`: memverifikasi pemanggilan `onClearPendingItem()` mereset `lastScannedRaw` ke null, memungkinkan retry pemindaian langsung untuk barcode yang sama.
  - Menjalankan `./gradlew.bat testDebugUnitTest`: seluruh 56 unit test di modul BGud (termasuk 5 unit test di `CreateReturnOrderViewModelUnitTest`) berhasil lulus (BUILD SUCCESSFUL, 100% success rate).
