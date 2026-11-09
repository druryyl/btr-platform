---
Title: BGud Mobile — Tampilan Rincian Hasil Sinkronisasi Data
Code: CR-001-SYNC-RESULT-BGUD
Artifact: FEASIBILITY-ASSESSMENT
Version: 1.0
LastUpdated: 2026-10-09
Status: READY-FOR-PLANNING
---

# 1. Request Summary

Feature being assessed: **Tampilan Rincian Hasil Sinkronisasi Data pada BGud Mobile (SCR-MOB-007 / SCR-MOB-RO-005)**

Referenced artifacts:

- ISSUE: [CR-001-SYNC-RESULT-BGUD.md](file:///d:/Project.Private/btr-platform/docs/issues/CR-001-SYNC-RESULT-BGUD.md)
- DOMAIN: Mobile Warehouse / Barcode Registry & Return Order
- FEATURE: Synchronization & Offline Operational Queues

## Objective

Menilai kelayakan teknis dan operasional untuk menambahkan kartu rincian hasil sinkronisasi data kuantitatif pada layar `SynchronizationScreen` di aplikasi BGud Mobile. Tampilan ini memberikan konfirmasi transparan kepada operator gudang mengenai jumlah data yang berhasil diunggah (Return Order, Registrasi Barcode) dan diunduh (Katalog Barang, Barcode Registry, Referensi Retur), termasuk penanganan status sukses penuh, parsial, dan gagal.

---

# 2. Current State

Fakta temuan dari artefak dan basis kode saat ini:

## Existing Behavior

1. **Antarmuka Layar (`SynchronizationScreen.kt`)**:
   - Memiliki Card 1: Data Master (menampilkan waktu sinkronisasi terakhir: Katalog Barang, Barcode Registry, Referensi Retur).
   - Memiliki Card 2: Antrean Return Order (menampilkan jumlah draft menunggu pengiriman dan jumlah dokumen tersinkronkan).
   - Memiliki Card 3: Antrean Registrasi Barcode (menampilkan jumlah menunggu, berhasil, ditolak).
   - Banner feedback di bawah Card 3 (baris 240–312) hanya mengevaluasi status biner melalui `when`:
     - `isSyncing`: Menampilkan indikator loading berputar + teks `"Proses sinkronisasi data sedang berjalan..."`.
     - `isFailed`: Menampilkan kotak merah bertuliskan `"Gagal Sinkronisasi"` beserta teks error mentah.
     - `isSynchronized`: Menampilkan kotak hijau tipis bertuliskan `"Semua data berhasil disinkronkan dengan server."`.
   - Tombol *CTA* "Sinkronkan Sekarang" memicu eksekusi paralel ke `viewModel.syncNow(context)` dan `returnOrderViewModel.syncNow(context)`.

2. **Lapisan ViewModel (`SynchronizationViewModel.kt` & `ReturnOrderSyncViewModel.kt`)**:
   - `SynchronizationViewModel` menjadwalkan `BarcodeSyncWorker` dan memantau status `WorkInfo`. Saat status `SUCCEEDED`, hanya memeriksa apakah `OUTPUT_ERRORS` kosong, lalu menyetel `_syncState.value = SyncState.SYNCHRONIZED`.
   - `ReturnOrderSyncViewModel` menjadwalkan `ReturnOrderSyncWorker` dan melakukan hal serupa.
   - Tidak ada `StateFlow` di kedua ViewModel yang mengekspos metrik data kuantitatif yang dikembalikan oleh masing-masing worker.

3. **Lapisan Worker (`BarcodeSyncWorker.kt` & `ReturnOrderSyncWorker.kt`)**:
   - `BarcodeSyncWorker` pada `toOutputData()` telah memetakan:
     - `OUTPUT_SUBMITTED` (`submittedCount`)
     - `OUTPUT_SUBMIT_SKIPPED` (`submitSkippedCount`)
     - `OUTPUT_SUBMIT_FAILED` (`submitFailedCount`)
     - `OUTPUT_BARCODE_COUNT` (`barcodeCount`)
     - `OUTPUT_BARANG_COUNT` (`barangCount`)
     - `OUTPUT_SYNCED` (`syncedCount`)
     - `OUTPUT_REJECTED` (`rejectedCount`)
     - `OUTPUT_ERRORS` (`errors`)
   - `ReturnOrderSyncWorker` pada `toOutputData()` telah memetakan:
     - `OUTPUT_SUBMITTED` (`submittedCount`)
     - `OUTPUT_SUBMIT_FAILED` (`submitFailedCount`)
     - `OUTPUT_CUSTOMER_COUNT` (`customerCount`)
     - `OUTPUT_SALESPERSON_COUNT` (`salesPersonCount`)
     - `OUTPUT_DRIVER_COUNT` (`driverCount`)
     - `OUTPUT_ERRORS` (`errors`)

## Existing Constraints

1. **Teknis**:
   - Sinkronisasi berjalan secara *asynchronous* melalui WorkManager Android (`BarcodeSyncWorker` dan `ReturnOrderSyncWorker` adalah dua *OneTimeWorkRequest* terpisah).
   - Kedua worker dapat selesai pada waktu yang sedikit berbeda.
   - Perangkat dapat berada dalam kondisi *offline*, sehingga tombol sinkronisasi dinonaktifkan saat tidak ada koneksi valid.
2. **Operasional / UX**:
   - Operator memerlukan konfirmasi instan di layar tanpa perlu berpindah ke menu lain atau membuka dialog tambahan yang menghalangi alur kerja.
   - Data kuantitatif harus mudah dibaca (angka tegas dengan warna kontras industrial: hijau untuk sukses, merah/oranye untuk perhatian/kegagalan).
3. **Bisnis**:
   - Tidak ada perubahan pada aturan bisnis sinkronisasi, skema Room database, kontrak payload REST API, maupun alur verifikasi data di backend.

---

# 3. Gap Analysis

| ID | Severity | Gap |
|------|------|------|
| GAP-001 | MAJOR | **Ketiadaan Tampilan Rincian Kuantitatif di UI**: Layar `SynchronizationScreen` hanya menampilkan banner teks sederhana tanpa rincian angka data yang baru saja diproses. |
| GAP-002 | MAJOR | **Metrik Worker Belum Diekspos oleh ViewModel**: `SynchronizationViewModel` dan `ReturnOrderSyncViewModel` membuang data hasil kuantitatif dari `workInfo.outputData` dan hanya membaca pesan error. |
| GAP-003 | MINOR | **Ketiadaan Status Parsial (Selesai Sebagian)**: UI hanya mengenal kondisi biner (Semua Sukses atau Gagal), sehingga jika satu antrean berhasil namun antrean lain gagal, pengguna tidak mendapatkan konfirmasi atas data yang berhasil masuk. |

---

# 4. Open Questions

| ID | Question | Impact |
|------|------|------|
| OQ-001 | Apakah metrik hasil sinkronisasi mencakup data dua arah (Upload dan Download) atau hanya data yang diunggah? | Mempengaruhi struktur model hasil dan komponen kartu yang ditampilkan di layar. |
| OQ-002 | Di mana posisi dan format visual komponen hasil sinkronisasi diletakkan? | Mempengaruhi hierarki tata letak (*layout*) `SynchronizationScreen`. |
| OQ-003 | Apakah data hasil sinkronisasi terakhir perlu disimpan persisten (DataStore) atau cukup *in-memory*? | Mempengaruhi kebutuhan penyimpanan lokal (*preferences* / *database*). |
| OQ-004 | Bagaimana respons antarmuka ketika terjadi kegagalan parsial (misal salah satu worker gagal)? | Menentukan logika status badge (Sukses, Selesai Sebagian, Gagal) dan penyajian daftar error. |
| OQ-005 | Apakah rincian per kategori item data disajikan secara terbuka (*expanded*) atau dapat diciutkan? | Mempengaruhi interaksi pengguna dan kepadatan informasi layar. |

---

# 5. Assumptions

| ID | Assumption |
|------|------|
| ASM-001 | Worker latar belakang (`BarcodeSyncWorker` dan `ReturnOrderSyncWorker`) selalu mengembalikan output data dengan kunci (*keys*) yang konsisten pada saat selesai (`SUCCEEDED` maupun `FAILED`). |
| ASM-002 | Tidak diperlukan penambahan endpoint API baru maupun perubahan skema database Room lokal; semua metrik sudah tersedia di level worker client. |
| ASM-003 | Sesi ViewModel cukup untuk menampung riwayat hasil sinkronisasi selama operator berada di layar tersebut. |

---

# 6. Risks

| ID | Risk | Impact | Mitigation |
|------|------|------|------|
| RISK-001 | **Asynchronous Completion Lag**: `BarcodeSyncWorker` dan `ReturnOrderSyncWorker` selesai pada detik yang berbeda, memicu perubahan status parsial sementara sebelum keduanya tuntas. | UI dapat berkedip atau menampilkan status parsial sesaat sebelum worker kedua selesai. | UI menggabungkan status kedua worker; selama salah satu worker masih berstatus `SYNCHRONIZING`, status umum tetap dianggap `isSyncing`. Kartu hasil lengkap baru dimutakhirkan secara terpadu setelah keduanya selesai. |
| RISK-002 | **Pesan Error Terlalu Panjang**: Pesan kegagalan dari backend berpotensi merusak tata letak kartu hasil jika tidak dibatasi. | Tampilan kartu menjadi terlalu panjang dan mendorong tombol aksi keluar layar. | Menggunakan gaya teks ringkas dan pemisah titik bulat (*bullet*) atau pembungkus baris yang rapi sesuai tema industrial. |

---

# 7. Recommendations

## Opsi A (Direkomendasikan) - Kartu Khusus 'Hasil Sinkronisasi' Terpadu di Layar Utama

Menghadirkan `IndustrialCard` khusus di bawah Card 3 dan di atas tombol CTA yang menampilkan:
- Badge Status: 🟢 Sukses Penuh / 🟠 Selesai Sebagian / 🔴 Gagal Sinkronisasi.
- Baris Ringkasan Total Data (Total Diunggah & Total Diunduh).
- Rincian Data Diunggah (Return Order terkirim/gagal, Registrasi Barcode terkirim/dilewati/gagal).
- Rincian Data Diunduh (Katalog Barang, Barcode Registry, Referensi Pelanggan, Salesman, Driver).
- Catatan pesan error jika terdapat kegagalan parsial.

### Kelebihan
- Informasi kuantitatif langsung terlihat jelas (*expanded*) tanpa memblokir operator dengan modal/dialog.
- Konsisten dengan pola visual modern industrial aplikasi BGud.
- Memberikan kepastian operasional yang tinggi bagi operator gudang.

### Kekurangan
- Menambah panjang vertikal layar yang memerlukan *scroll* halus saat kartu muncul.

## Opsi B - Dialog Pop-up Ringkasan Sinkronisasi

Menampilkan pop-up dialog / bottom-sheet segera setelah kedua worker selesai.

### Kelebihan
- Fokus penuh pada hasil sinkronisasi sesaat setelah tombol ditekan.

### Kekurangan
- Menambah friksi interaksi operator (harus menekan tombol tutup untuk melanjutkan pekerjaan).
- Tidak sesuai dengan preferensi operator gudang yang membutuhkan alur kerja cepat.

---

# 8. Gap Closure

Resolusi seluruh GAP dan OQ berdasarkan hasil kesepakatan sesi klarifikasi (/grill-me):

## GAP-001
- **Status**: CLOSED
- **Decision**: Menambahkan Kartu Khusus 'Hasil Sinkronisasi' (`IndustrialCard`) pada `SynchronizationScreen.kt`.
- **Rationale**: Menyediakan visibilitas kuantitatif lengkap bagi operator gudang mengenai data yang baru saja diproses.
- **Impact**: UI layar `SynchronizationScreen` dimodifikasi menggantikan banner teks statis lama.
- **Architecture Impact**: Memerlukan penyelarasan data flow dari ViewModel ke Composable.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## GAP-002
- **Status**: CLOSED
- **Decision**: Memperbarui `SynchronizationViewModel` dan `ReturnOrderSyncViewModel` agar mengekstrak seluruh metrik dari `workInfo.outputData` ke data class model hasil sinkronisasi dan mengeksposnya via `StateFlow`.
- **Rationale**: Data metrik sudah diproduksi oleh worker namun sebelumnya tidak diteruskan ke lapisan presentasi.
- **Impact**: Penambahan model data ringkasan dan StateFlow pada kedua ViewModel.
- **Architecture Impact**: Tidak ada perubahan arsitektur besar; memanfaatkan pola reaktif StateFlow yang sudah ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## GAP-003
- **Status**: CLOSED
- **Decision**: Mengimplementasikan evaluasi status gabungan di UI: Sukses Penuh (hijau), Selesai Sebagian (oranye/kuning), dan Gagal (merah).
- **Rationale**: Mencegah kebingungan operator ketika sebagian data berhasil diproses namun data lain gagal.
- **Impact**: Kartu hasil tetap menampilkan angka data yang berhasil diproses disertai pesan error yang jelas.
- **Architecture Impact**: Tidak ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## OQ-001
- **Status**: CLOSED
- **Decision**: Menampilkan rincian dua arah secara lengkap (Upload: Return Order & Barcode; Download: Katalog Barang, Barcode, & Referensi Pelanggan/Sales/Driver).
- **Rationale**: Memberikan kepastian menyeluruh baik untuk data antrean lokal maupun data master terbaru dari server.
- **Impact**: Model ringkasan memuat kedua kelompok kategori data.
- **Architecture Impact**: Tidak ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## OQ-002
- **Status**: CLOSED
- **Decision**: Ditempatkan sebagai Kartu Khusus di layar utama, menggantikan posisi banner feedback lama di bawah Kartu 3 dan di atas tombol CTA.
- **Rationale**: Menjaga hierarki alami: Status Antrean -> Hasil Eksekusi -> Tombol Aksi.
- **Impact**: Tata letak layar tetap bersih dan proporsional.
- **Architecture Impact**: Tidak ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## OQ-003
- **Status**: CLOSED
- **Decision**: Disimpan secara *in-memory* saja pada state ViewModel selama layar aktif.
- **Rationale**: Sesuai kebutuhan operasional langsung pasca-sinkronisasi tanpa membebani penyimpanan preferensi lokal.
- **Impact**: Tidak memerlukan modifikasi `SessionPreferencesDataSource` atau Room DB.
- **Architecture Impact**: Tidak ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## OQ-004
- **Status**: CLOSED
- **Decision**: Status 'Selesai Sebagian' (kuning/oranye) atau 'Gagal' (merah), dengan tetap mencantumkan item yang berhasil dan merinci pesan kegagalan.
- **Rationale**: Transparansi penuh atas operasi jaringan yang berhasil dan yang perlu dicoba ulang.
- **Impact**: Komponen UI mendukung pewarnaan tematik kondisional.
- **Architecture Impact**: Tidak ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

## OQ-005
- **Status**: CLOSED
- **Decision**: Selalu terbuka (*expanded* secara default).
- **Rationale**: Mengurangi ketukan jari (*tap*) operator di lingkungan kerja gudang yang dinamis.
- **Impact**: Semua baris rincian dirender langsung saat kartu tampil.
- **Architecture Impact**: Tidak ada.
- **Resolved By**: Operator & Developer (/grill-me session)
- **Resolved Date**: 2026-10-09

---

# 9. Architecture Applicability

## Decision
**ARCHITECTURE-NOT-REQUIRED**

## Rationale
Perubahan ini adalah peningkatan pada lapisan presentasi (*presentation/UI layer*) dan penyaluran data (*ViewModel state mapping*) dari metrik yang sudah diproduksi oleh worker yang ada. Tidak ada komponen baru yang diciptakan, tidak ada kontrak API backend yang diubah, tidak ada modifikasi skema database lokal, dan batasan integrasi tetap utuh. Perubahan dapat langsung dieksekusi melalui rencana implementasi terarah (*Implementation Plan*).

---

# 10. Planning Readiness

## Readiness Checklist

- [x] All critical gaps resolved
- [x] All required decisions recorded
- [x] All blocking open questions resolved
- [x] Architecture / Technical realization path is clear and ready for planning

## Status

**READY-FOR-PLANNING**

## Notes

Seluruh kesepakatan dan analisis kebutuhan telah tuntas ditutup tanpa pertanyaan terbuka yang tersisa. Gerbang READY-FOR-PLANNING telah diberikan oleh `ica-architect`. Sesuai keputusan ARCHITECTURE-NOT-REQUIRED, perencanaan langsung dilanjutkan ke pembuatan IMPLEMENTATION-PLAN.

---

# 11. References

Referenced artifacts:
- [CR-001-SYNC-RESULT-BGUD.md](file:///d:/Project.Private/btr-platform/docs/issues/CR-001-SYNC-RESULT-BGUD.md)

Referenced codebase locations:
- [SynchronizationScreen.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/ui/screen/SynchronizationScreen.kt)
- [SynchronizationViewModel.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/SynchronizationViewModel.kt)
- [ReturnOrderSyncViewModel.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/viewmodel/ReturnOrderSyncViewModel.kt)
- [BarcodeSyncWorker.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/sync/BarcodeSyncWorker.kt)
- [ReturnOrderSyncWorker.kt](file:///d:/Project.Private/btr-platform/src/BGud/app/src/main/java/com/elsasa/bgud/sync/ReturnOrderSyncWorker.kt)
