---
Title: BGud Mobile — Tampilan Rincian Hasil Sinkronisasi Data
Code: CR-001-SYNC-RESULT-BGUD
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-09
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Mengimplementasikan tampilan rincian hasil sinkronisasi data yang detail dan kuantitatif pada layar `SynchronizationScreen` di aplikasi BGud Mobile (`src/BGud`), mencakup data yang diunggah (Return Order, Registrasi Barcode) dan data yang diunduh (Katalog Barang, Barcode Registry, Referensi Retur) beserta indikator status (Sukses Penuh, Selesai Sebagian, Gagal).

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- ISSUE: [CR-001-SYNC-RESULT-BGUD.md](file:///d:/Project.Private/btr-platform/docs/issues/CR-001-SYNC-RESULT-BGUD.md)
- FEASIBILITY-ASSESSMENT: [CR-001-SYNC-RESULT-BGUD-FEASIBILITY-ASSESSMENT.md](file:///d:/Project.Private/btr-platform/docs/issues/CR-001-SYNC-RESULT-BGUD-FEASIBILITY-ASSESSMENT.md)

Architecture Applicability: ARCHITECTURE-NOT-REQUIRED

No architectural target-state artifact was required. Implementation relies on existing technical structure. Approved feasibility decisions are authoritative for the change. The current codebase is the source of current technical truth.

---

# 2. Planning Scope

Paket pekerjaan ini mencakup pembaruan lapisan ViewModel dan UI Jetpack Compose pada aplikasi Android BGud:
- Penambahan model data ringkasan sinkronisasi (`BarcodeSyncSummary` dan `ReturnOrderSyncSummary`) pada ViewModel.
- Ekstraksi seluruh pasangan data metrik dari `workInfo.outputData` saat worker WorkManager (`BarcodeSyncWorker` dan `ReturnOrderSyncWorker`) menyelesaikan tugasnya.
- Pembuatan komponen `SyncResultCard` berbasis `IndustrialCard` dengan indikator status tematik (Hijau: Sukses, Oranye: Selesai Sebagian, Merah: Gagal).
- Penggantian banner feedback sederhana lama pada `SynchronizationScreen.kt` dengan `SyncResultCard` yang selalu terbuka (*expanded*).
- Pengujian unit (*unit test*) untuk memastikan ekstraksi metrik output berjalan akurat.

**Target repository:** `src/BGud` (Android Jetpack Compose codebase)

**Batasan Cakupan:**
- In: `SynchronizationViewModel.kt`, `ReturnOrderSyncViewModel.kt`, `SynchronizationScreen.kt`, serta unit test terkait di `src/BGud`.
- Out: Tidak ada perubahan pada `BarcodeSyncWorker.kt`, `ReturnOrderSyncWorker.kt`, skema database Room, endpoint REST API Cloud backend, atau DataStore preferences.

---

# 3. Dependencies

Semua pekerjaan berada di dalam satu modul aplikasi `src/BGud`.

Untuk dependensi antar-slice:
- `Depends On` mendeklarasikan prasyarat implementasi.
- Dependensi hanya mereferensikan Slice ID yang valid.
- Kepuasan dependensi mensyaratkan slice yang dirujuk memiliki status implementasi `IMPLEMENTED`.
- Dependensi mencerminkan prasyarat teknis nyata antar komponen kode.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
|-------|----------------------|---------------|----------|
| P1 - ViewModel State & Metric Mapping | IMPLEMENTED | GO | 2/2 |
| P2 - UI Component & Screen Integration | IMPLEMENTED | GO | 2/2 |
| P3 - Verification & Tests | IMPLEMENTED | GO | 1/1 |

---

# 5. Phases

## P1 - ViewModel State & Metric Mapping

Implementation Status: IMPLEMENTED  
Review Status: GO

### P1-S01

**Title:** Ekstraksi metrik hasil sinkronisasi Barcode ke `SynchronizationViewModel`

**Implementation Status:** IMPLEMENTED  
**Review Status:** GO  

**Objective:**
Menambahkan data class `BarcodeSyncSummary` dan mengekspos `syncSummary: StateFlow<BarcodeSyncSummary?>` pada `SynchronizationViewModel` yang diekstrak dari `workInfo.outputData` saat worker selesai.

**Depends On:** None

**Repository:** `src/BGud`

**Completion Criteria:**
- Model data `BarcodeSyncSummary` didefinisikan dengan properti:
  - `submitted: Int`
  - `submitSkipped: Int`
  - `submitFailed: Int`
  - `barcodeCount: Int`
  - `barangCount: Int`
  - `synced: Int`
  - `rejected: Int`
  - `errors: String`
- `_syncSummary = MutableStateFlow<BarcodeSyncSummary?>(null)` dan `val syncSummary: StateFlow<BarcodeSyncSummary?>` ditambahkan pada `SynchronizationViewModel`.
- Pada `syncNow()`, `_syncSummary.value` direset ke `null`.
- Pada `observeRun()`, saat status `WorkInfo.State.SUCCEEDED` maupun `FAILED`, `outputData` dipetakan ke instans `BarcodeSyncSummary` dan disimpan ke `_syncSummary.value`.

**Implementation Notes:**
- Ditambahkan data class `BarcodeSyncSummary` dengan 8 properti metrik kuantitatif dan helper `fromOutputData(data: Data)`.
- Ditambahkan `_syncSummary = MutableStateFlow<BarcodeSyncSummary?>(null)` serta diekspos melalui `val syncSummary: StateFlow<BarcodeSyncSummary?>`.
- `_syncSummary.value` direset ke `null` pada `syncNow()`.
- Metrik hasil diekstrak dan disimpan ke `_syncSummary.value` pada event worker `SUCCEEDED` maupun `FAILED` di `observeRun()`.

**Changed Files:**
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/SynchronizationViewModel.kt`

**Notes:**
Memenuhi keputusan GAP-002 dan ASM-001 dari FEASIBILITY-ASSESSMENT.

---

### P1-S02

**Title:** Ekstraksi metrik hasil sinkronisasi Return Order ke `ReturnOrderSyncViewModel`

**Implementation Status:** IMPLEMENTED  
**Review Status:** GO  

**Objective:**
Menambahkan data class `ReturnOrderSyncSummary` dan mengekspos `syncSummary: StateFlow<ReturnOrderSyncSummary?>` pada `ReturnOrderSyncViewModel` yang diekstrak dari `workInfo.outputData` saat worker selesai.

**Depends On:** None

**Repository:** `src/BGud`

**Completion Criteria:**
- Model data `ReturnOrderSyncSummary` didefinisikan dengan properti:
  - `submitted: Int`
  - `submitFailed: Int`
  - `customerCount: Int`
  - `salesPersonCount: Int`
  - `driverCount: Int`
  - `errors: String`
- `_syncSummary = MutableStateFlow<ReturnOrderSyncSummary?>(null)` dan `val syncSummary: StateFlow<ReturnOrderSyncSummary?>` ditambahkan pada `ReturnOrderSyncViewModel`.
- Pada `syncNow()`, `_syncSummary.value` direset ke `null`.
- Pada `observeRun()`, saat status `WorkInfo.State.SUCCEEDED` maupun `FAILED`, `outputData` dipetakan ke instans `ReturnOrderSyncSummary` dan disimpan ke `_syncSummary.value`.

**Notes:**
Memenuhi keputusan GAP-002 dan ASM-001 dari FEASIBILITY-ASSESSMENT. Dapat dijalankan secara paralel dengan P1-S01.

**Implementation Notes:**
- Menambahkan data class `ReturnOrderSyncSummary` dengan 6 field metrik kuantitatif.
- Menambahkan `_syncSummary` dan mengekspos `syncSummary: StateFlow<ReturnOrderSyncSummary?>` pada `ReturnOrderSyncViewModel`.
- Reset `_syncSummary.value = null` di dalam `syncNow()`.
- Ekstraksi `outputData` ke instans `ReturnOrderSyncSummary` pada status worker `SUCCEEDED` dan `FAILED`.
- Verifikasi kompilasi dan pengujian unit (`./gradlew testDebugUnitTest`) berhasil.

---

## P2 - UI Component & Screen Integration

Implementation Status: IMPLEMENTED  
Review Status: GO

### P2-S03

**Title:** Implementasi komponen `SyncResultCard` dengan tampilan rincian dua arah

**Implementation Status:** IMPLEMENTED  
**Review Status:** GO  

**Objective:**
Membuat Composable `SyncResultCard` berbasis `IndustrialCard` yang menampilkan ringkasan metrik kuantitatif dua arah (Upload & Download), badge status dinamis (Sukses / Selesai Sebagian / Gagal), serta catatan error jika terdapat kegagalan parsial.

**Depends On:** P1-S01, P1-S02

**Repository:** `src/BGud`

**Completion Criteria:**
- Komponen `SyncResultCard` menerima parameter data ringkasan barcode dan return order.
- Logika penentuan status visual:
  - **Sukses Penuh** (Badge hijau): Tidak ada error dan tidak ada data yang gagal terkirim.
  - **Selesai Sebagian** (Badge oranye/kuning): Terdapat data yang berhasil disinkronkan, namun ada item yang gagal terkirim atau salah satu worker mengembalikan error.
  - **Gagal** (Badge merah): Kedua worker gagal atau tidak ada data yang berhasil disinkronkan saat status failed.
- Menampilkan ringkasan metrik total:
  - Total Data Diunggah (Return Order + Barcode dikirim).
  - Total Data Diunduh (Katalog Barang + Barcode Registry + Referensi).
- Menampilkan rincian per item data secara terbuka (*expanded*):
  - Bagian Pengiriman (Upload): Return Order terkirim (dan gagal jika ada), Registrasi Barcode terkirim (dan skipped/failed jika ada).
  - Bagian Pembaruan (Download): Katalog Barang, Barcode Registry, Referensi Pelanggan, Salesman, Driver.
- Menampilkan pesan error secara rapi jika terdapat pesan kesalahan.

**Implementation Notes:**
- Membuat komponen Composable `SyncResultCard.kt` dalam package `com.elsasa.bgud.ui.component` berbasis `IndustrialCard`.
- Menerima parameter `barcodeSummary: BarcodeSyncSummary?`, `returnOrderSummary: ReturnOrderSyncSummary?`, serta parameter opsional `modifier`, `isFailed`, dan `errorMessage`.
- Mengimplementasikan logika penentuan status visual: Sukses Penuh (Badge hijau), Selesai Sebagian (Badge oranye/kuning), dan Gagal (Badge merah).
- Menghadirkan ringkasan metrik total: Total Data Diunggah (Return Order + Registrasi Barcode) dan Total Data Diunduh (Katalog Barang + Barcode Registry + Referensi).
- Menampilkan rincian per kategori secara terbuka (*expanded*): Bagian Pengiriman (Upload) dan Bagian Pembaruan (Download).
- Menampilkan kontainer pesan kesalahan jika terdapat kendala/error.
- Verifikasi berhasil melalui kompilasi Kotlin (`compileDebugKotlin`) dan pengujian unit (`testDebugUnitTest`).

**Changed Files:**
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/component/SyncResultCard.kt`

**Notes:**
Memenuhi keputusan GAP-001, GAP-003, OQ-001, OQ-002, OQ-004, OQ-005 dari FEASIBILITY-ASSESSMENT.

---

### P2-S04

**Title:** Integrasi `SyncResultCard` ke dalam `SynchronizationScreen`

**Implementation Status:** IMPLEMENTED  
**Review Status:** GO  

**Objective:**
Mengganti banner status feedback lama pada `SynchronizationScreen.kt` dengan `SyncResultCard`, serta menyelaraskan status loading saat proses sinkronisasi sedang berjalan.

**Depends On:** P2-S03

**Repository:** `src/BGud`

**Completion Criteria:**
- `SynchronizationScreen` mengumpulkan state `barcodeSummary` dan `returnOrderSummary` dari kedua ViewModel.
- Selama `isSyncing` aktif, tetap menampilkan banner progress loading yang ada.
- Saat sinkronisasi selesai dan setidaknya salah satu summary tersedia (atau status `isSynchronized`/`isFailed`), menampilkan `SyncResultCard`.
- Banner statis lama ("Semua data berhasil disinkronkan..." dan kotak error sederhana) digantikan sepenuhnya oleh `SyncResultCard`.
- Posisi kartu berada di bawah Card 3 (Antrean Registrasi Barcode) dan di atas tombol CTA "Sinkronkan Sekarang".

**Implementation Notes:**
- Mengumpulkan state `barcodeSummary` dan `returnOrderSummary` dari `SynchronizationViewModel` dan `ReturnOrderSyncViewModel` menggunakan `collectAsState()`.
- Menggantikan banner status statis lama dengan `SyncResultCard` ketika proses sinkronisasi selesai dan summary tersedia, atau status `isSynchronized` / `isFailed`.
- Tetap mempertahankan banner animasi progress loading saat `isSyncing` bernilai true.
- Meletakkan `SyncResultCard` di antara Card 3 (Antrean Registrasi Barcode) dan tombol CTA sinkronisasi.
- Verifikasi kompilasi dan unit tests berhasil dijalankan melalui `./gradlew compileDebugKotlin testDebugUnitTest`.

**Changed Files:**
- `src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/SynchronizationScreen.kt`

**Notes:**
Memenuhi OQ-002 dari FEASIBILITY-ASSESSMENT.

---

## P3 - Verification & Tests

Implementation Status: IMPLEMENTED  
Review Status: GO

### P3-S05

**Title:** Unit test untuk ekstraksi summary pada ViewModel

**Implementation Status:** IMPLEMENTED  
**Review Status:** GO  

**Objective:**
Menuliskan pengujian unit untuk memverifikasi bahwa `SynchronizationViewModel` dan `ReturnOrderSyncViewModel` memetakan `outputData` dari worker ke `syncSummary` dengan benar.

**Depends On:** P1-S01, P1-S02

**Repository:** `src/BGud`

**Completion Criteria:**
- Test case memverifikasi parsing seluruh field output dari WorkManager `Data` ke model summary.
- Test case memverifikasi reset summary ke `null` saat `syncNow()` dipanggil.
- Semua pengujian unit berjalan sukses.

**Implementation Notes:**
- Ditambahkan `ReturnOrderSyncSummary.fromOutputData(data: Data)` dengan nilai default 0 / string kosong untuk parsing output worker secara konsisten dengan `BarcodeSyncSummary`.
- Ditambahkan `handleWorkInfo(workInfo: WorkInfo?)` dan `setOnlineForTesting(online: Boolean)` pada `SynchronizationViewModel` dan `ReturnOrderSyncViewModel` untuk pengujian unit pemetaan worker output ke `syncSummary` serta verifikasi reset state.
- Diselaraskan reset `_syncSummary.value = null` pada `SynchronizationViewModel.syncNow()` agar dieksekusi sebelum validasi konfigurasi server.
- Dibuat unit test lengkap di `SynchronizationViewModelTest.kt` (6 test cases): parsing seluruh 8 field `BarcodeSyncSummary`, fallback default saat data kosong, pemetaan output saat SUCCEEDED dan FAILED, verifikasi reset `syncSummary` ke `null` saat `syncNow()` dipanggil, serta verifikasi summary tidak direset saat offline.
- Dibuat unit test lengkap di `ReturnOrderSyncViewModelTest.kt` (6 test cases): parsing seluruh 6 field `ReturnOrderSyncSummary`, fallback default saat data kosong, pemetaan output saat SUCCEEDED dan FAILED, verifikasi reset `syncSummary` ke `null` saat `syncNow()` dipanggil, serta verifikasi summary tidak direset saat offline.
- Seluruh pengujian unit berjalan sukses (34 tests passed, 0 failures) via `./gradlew testDebugUnitTest`.

**Changed Files:**
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/SynchronizationViewModel.kt`
- `src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/ReturnOrderSyncViewModel.kt`
- `src/BGud/app/src/test/java/com/elsasa/bgud/viewmodel/SynchronizationViewModelTest.kt`
- `src/BGud/app/src/test/java/com/elsasa/bgud/viewmodel/ReturnOrderSyncViewModelTest.kt`

**Notes:**
Memastikan kualitas dan ketahanan kode sebelum rilis.

---

# 6. Change Log

- 2026-10-09: Inisialisasi rencana implementasi awal untuk `CR-001-SYNC-RESULT-BGUD` dengan status `Execution Approval: APPROVED` dan `Status: NOT-STARTED`.
