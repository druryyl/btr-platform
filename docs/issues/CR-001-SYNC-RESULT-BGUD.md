# ISSUE

## Metadata

ID: `CR-001-SYNC-RESULT-BGUD`
Type: `CHANGE-REQUEST`
Status: `New`
Title: **BGud Mobile — Tampilan Rincian Hasil Sinkronisasi Data**

## Source

Reported By: Repository owner / User request
Reported Date: 2026-10-09

## Description

Pada aplikasi mobile BGud (`src/BGud`), operator gudang yang melakukan sinkronisasi data melalui layar Sinkronisasi Data (`SynchronizationScreen.kt`) memerlukan visibilitas dan konfirmasi yang lebih detail terkait hasil proses sinkronisasi.

Saat ini, layar hanya memberikan umpan balik berupa pesan status sederhana (misalnya "Semua data berhasil disinkronkan dengan server" atau pesan kesalahan umum). Pengguna meminta agar sistem menampilkan hasil sinkronisasi secara lebih detail dan kuantitatif, mencakup jumlah data yang berhasil disinkronkan untuk setiap entitas maupun ringkasan data yang diunggah dan diunduh.

## Desired Outcome

1. **Rincian Data Dua Arah**:
   - Menampilkan jumlah data yang diunggah/dikirim ke server (*Upload*): Dokumen Return Order yang terkirim/gagal, serta pengajuan registrasi barcode yang terkirim/dilewati/gagal.
   - Menampilkan jumlah data master yang diunduh/diperbarui dari server (*Download*): Katalog barang, Barcode registry, dan referensi retur (pelanggan, salesperson, driver).
2. **Komponen Hasil Visual**:
   - Tampilan berupa kartu khusus hasil sinkronisasi (*IndustrialCard*) di layar utama, menggantikan banner feedback satu baris yang ada saat ini.
   - Ditempatkan secara proporsional di bawah kartu antrean dan di atas tombol aksi *CTA*.
3. **Indikator Status Jelas**:
   - Menampilkan badge/status yang membedakan kondisi Sukses Penuh, Selesai Sebagian (parsial, jika ada dokumen/antrean yang gagal terkirim namun data lain sukses), dan Gagal Total.
   - Jika terdapat kegagalan parsial, kartu tetap menampilkan rincian data yang berhasil disertai rincian pesan error/kegagalan.
4. **Kepadatan Informasi**:
   - Rincian metrik kuantitatif disajikan terbuka secara langsung (*expanded*) agar operator segera mengetahui hasil pembaruan tanpa langkah klik tambahan.
5. **Siklus Hidup Hasil**:
   - Tampilan rincian hasil bersifat *in-memory* yang aktif setelah proses tombol sinkronisasi selesai dijalankan pada sesi tersebut.

## Current Situation

1. Layar `SynchronizationScreen.kt` (baris 240–312) hanya memiliki banner kondisi `when (isSyncing / isFailed / isSynchronized)` dengan baris teks statis ("Semua data berhasil disinkronkan dengan server." atau teks kegagalan).
2. Worker sinkronisasi latar belakang (`BarcodeSyncWorker.kt` dan `ReturnOrderSyncWorker.kt`) sebenarnya sudah menghitung dan mengembalikan metrik hasil sinkronisasi melalui `workInfo.outputData` (seperti `OUTPUT_SUBMITTED`, `OUTPUT_BARCODE_COUNT`, `OUTPUT_BARANG_COUNT`, `OUTPUT_CUSTOMER_COUNT`, dll.).
3. ViewModel (`SynchronizationViewModel.kt` dan `ReturnOrderSyncViewModel.kt`) saat ini hanya membaca `OUTPUT_ERRORS` dari `workInfo.outputData` dan belum mengekspos rincian jumlah item data tersebut ke StateFlow untuk dikonsumsi oleh antarmuka Compose.

## Evidence

- [SynchronizationScreen.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/SynchronizationScreen.kt) — Bagian feedback banner belum menyajikan metrik kuantitatif.
- [SynchronizationViewModel.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/SynchronizationViewModel.kt) — `observeRun` mengabaikan metrik output worker selain pengecekan error.
- [ReturnOrderSyncViewModel.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/ReturnOrderSyncViewModel.kt) — `observeRun` mengabaikan metrik output worker selain pengecekan error.
- [BarcodeSyncWorker.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/sync/BarcodeSyncWorker.kt) — Memiliki konstanta output data (`OUTPUT_SUBMITTED`, `OUTPUT_SUBMIT_SKIPPED`, `OUTPUT_SUBMIT_FAILED`, `OUTPUT_BARCODE_COUNT`, `OUTPUT_BARANG_COUNT`, `OUTPUT_SYNCED`, `OUTPUT_REJECTED`).
- [ReturnOrderSyncWorker.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/sync/ReturnOrderSyncWorker.kt) — Memiliki konstanta output data (`OUTPUT_SUBMITTED`, `OUTPUT_SUBMIT_FAILED`, `OUTPUT_CUSTOMER_COUNT`, `OUTPUT_SALESPERSON_COUNT`, `OUTPUT_DRIVER_COUNT`).

## Notes

1. **Solution-neutrality**: Dokumen ini mendefinisikan kebutuhan perubahan perilaku dan visibilitas pada antarmuka operator, tanpa mengunci implementasi internal worker atau kontrak jaringan.
2. **Next Workflow Stage**:
   - Target rute alur kerja berikutnya: `ISSUE (CHANGE-REQUEST)` $\rightarrow$ `ica-analyst` (tahap Discovery / Feasibility Assessment).
