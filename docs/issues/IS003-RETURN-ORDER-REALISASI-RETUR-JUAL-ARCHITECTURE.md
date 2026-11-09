---
Title: Target Architecture - Realisasi Order Retur Jual ke Retur Jual via ReturJualForm dan List Monitoring Return Order
Code: BTR-RETURN-ORDER-REALISASI-001
Artifact: ARCHITECTURE
Version: 1.0
LastUpdated: 2026-11-09
---

# 1. Overview

Arsitektur ini mendefinisikan realisasi teknis untuk mengalihkan mekanisme realisasi Order Retur Jual (`BTR_ReturnOrder`) menjadi dokumen fisik/akuntansi Retur Jual (`BTR_ReturJual`) di sistem distribusi pusat (`j05-btr-distrib`).

Perubahan ini menyelesaikan permasalahan pada change request `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ISSUE.md` (`BTR-RETURN-ORDER-REALISASI-001`) dengan menerapkan tiga prinsip utama: **Pragmatis**, **Explicit**, dan **Minimalist**:
1. Menggantikan proses batch kaku pada form lama (`GenerateReturnOrderForm.cs`) dengan alur input interaktif dan terpandu langsung pada `ReturJualForm.cs`, mengadopsi pola hubungan `OrderJualId` pada `FakturForm.cs`.
2. Menyimpan referensi asal dokumen secara eksplisit melalui penambahan kolom `ReturnOrderId VARCHAR(26)` pada tabel `BTR_ReturJual` yang diindeks dengan *filtered index*.
3. Menyediakan form monitoring representatif `ListReturnOrderForm.cs` (mengadopsi arsitektur `ListOrderForm.cs`) untuk memantau status penyelesaian order retur (`Synced`, `Partial`, `Imported`) serta nomor Retur Jual terkait, dilengkapi navigasi *double-click* langsung ke form transaksi.
4. Menjaga stabilitas modul persediaan dan generator mutasi stok (`GenStokReturJualWorker`) dengan mempertahankan atribut `JenisRetur` pada level header dokumen `BTR_ReturJual`.
5. Menerapkan manajemen *lifecycle state machine* dua arah yang konsisten pada `BTR_ReturnOrder.Status` (transisi maju saat penyimpanan Retur Jual, dan *revert* status saat pembatalan/void Retur Jual).

**Referenced CHANGE-REQUEST ISSUE:** `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ISSUE.md`  
**Referenced FEASIBILITY-ASSESSMENT:** `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-FEASIBILITY.md`  
**Parent / Baseline ARCHITECTURE:** `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`

---

# 2. Architectural Basis

## Business Context

- **DOMAIN:** `docs/foundation/DOMAIN.md` dan `docs/work/return-order/RETURN-ORDER-DOMAIN.md`:
  - Petugas gudang cabang mencatat fisik barang yang ditarik dari pelanggan melalui aplikasi gudang (`BtrReturGudang.Wpf`). Dokumen fisik ini dicatat sebagai Order Retur Jual dan disinkronkan ke server kantor pusat melalui `j07-btrade-sync`.
  - Pada sistem pusat (`j05-btr-distrib`), data intake tersimpan dalam tabel `BTR_ReturnOrder` dan `BTR_ReturnOrderItem` dengan status awal `'Synced'`.
  - Lokasi fisik gudang cabang tercatat sebagai *Depo* di kantor pusat, sedangkan *Warehouse* pada kantor pusat merupakan entitas *logical warehouse* (contoh: gudang barang baik vs gudang barang rusak / BS).
  - Dokumen Retur Jual (`BTR_ReturJual`) merupakan dokumen transaksi resmi (komersial, akuntansi, dan stok). Pembuatan mutasi kartu stok oleh `GenStokReturJualWorker` bergantung penuh pada penentuan `JenisRetur` dan `WarehouseId` pada header `BTR_ReturJual`.

## Analysis Input

FEASIBILITY-ASSESSMENT pada `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-FEASIBILITY.md` menetapkan keputusan yang telah disepakati:
- **GAP-001 / OQ-001 (Model Relasi & Tracking Status):** Tambahkan kolom `ReturnOrderId VARCHAR(26)` langsung ke tabel `BTR_ReturJual` dengan *filtered non-clustered index*. Status penyelesaian direfleksikan langsung pada kolom `BTR_ReturnOrder.Status` (`Synced` -> `Partial` -> `Imported`). Menghindari penambahan tabel mapping perantara baru.
- **GAP-002 / OQ-002 (Aturan Pengeditan Item & Status):** Pada `ReturJualForm`, pemanggilan `LoadReturnOrder(returnOrderId)` mengisi item barang dan kuantitas sesuai `JenisRetur` terpilih. User melengkapi harga, diskon, PPN, dan menentukan Warehouse logikal. User berhak menghapus baris item yang batal diretur. Transisi ke status `'Imported'` tercapai bila seluruh jenis retur pada order retur tersebut telah diterbitkan Retur Jual-nya; bila baru sebagian, berstatus `'Partial'`.
- **GAP-003 / OQ-003 (Alur Pemilahan JenisRetur Heterogen):** Jika order retur hanya memuat satu jenis retur (homogen), form otomatis mengunci jenis retur dan memuat seluruh item. Jika order memuat item BAGUS dan RUSAK (heterogen): jika salah satu jenis retur sudah terbit, otomatis pilih jenis retur yang tersisa; jika keduanya belum pernah terbit, tampilkan prompt dialog seleksi interaktif kepada user sebelum item dimuat.
- **GAP-004 / OQ-005 (Form Monitoring ListReturnOrderForm):** Gantikan `GenerateReturnOrderForm.cs` secara penuh dengan `ListReturnOrderForm.cs` yang mengadopsi pola `ListOrderForm.cs`. Menampilkan rentang tanggal, pencarian kata kunci, status realisasi (`Synced`, `Partial`, `Imported`), visualisasi nomor Retur Jual terkait, serta navigasi *double-click* ke `ReturJualForm`.
- **GAP-005 / OQ-006 (Pelengkapan Salesman & Driver):** Data Salesman dan Driver yang belum terisi dari gudang dilengkapi langsung oleh user di `ReturJualForm.cs` saat proses realisasi berlangsung, memanfaatkan kontrol lookup yang sudah ada di form.
- **GAP-006 / OQ-004 (Penanganan Void Retur Jual):** Saat dokumen Retur Jual yang memiliki `ReturnOrderId` dibatalkan (di-void), periksa keberadaan Retur Jual aktif lain. Jika masih ada Retur Jual aktif -> status `BTR_ReturnOrder` menjadi `'Partial'`; jika tidak ada lagi Retur Jual aktif -> status kembali ke `'Synced'`.

```text
ISSUE (BTR-RETURN-ORDER-REALISASI-001)
                  +
FEASIBILITY-ASSESSMENT (IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-FEASIBILITY)
                  +
BASELINE ARCHITECTURE (RETURN-ORDER-ARCHITECTURE)
                  ↓
             ARCHITECTURE
```

---

# 3. Scope

## Included

1. **Database Schema & Migration (`btr.sql`):**
   - Penambahan kolom `ReturnOrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_ReturJual_ReturnOrderId DEFAULT('')` pada tabel `BTR_ReturJual`.
   - Pembuatan index `IX_BTR_ReturJual_ReturnOrderId` bertapis (`WHERE ReturnOrderId <> ''`).
   - Pembuatan skrip migrasi idempotent `Alter_BTR_ReturJual_Add_ReturnOrderId.sql` pada `btr.sql/Scripts/`.
   - Pembaruan baseline DDL pada `btr.sql/Tables/InventoryContext/BTR_ReturJual.sql`.

2. **Domain Layer (`btr.domain`):**
   - Penambahan properti `ReturnOrderId` pada model entitas `ReturJualModel`.
   - Standardisasi konstanta status `ReturnOrderStatus` (`Synced`, `Partial`, `Imported`) pada agregat `ReturnOrder`.

3. **Application Layer (`btr.application`):**
   - Penambahan method query pada `IReturJualDal` untuk mengambil/mendaftar Retur Jual berdasarkan `ReturnOrderId` (`ListByReturnOrderId` atau `ListActiveByReturnOrderId`).
   - Ekstensi `IReturJualBuilder` dan implementasinya `ReturJualBuilder` untuk mendukung penetapan `ReturnOrderId`.
   - Pembuatan worker baru `ReturnOrderStatusWorker` (`IReturnOrderStatusWorker`) untuk mengevaluasi dan memperbarui status `BTR_ReturnOrder` berdasarkan status `BTR_ReturnOrderItem.JenisRetur` dan dokumen aktif `BTR_ReturJual`.
   - Pembuatan query monitoring `ListReturnOrderMonitoringQuery` (atau penyempurnaan `ListReturnOrderQuery`) yang mengembalikan daftar order retur beserta data penyelesaian Retur Jual terkait untuk form monitoring.
   - Deprecasi / penonaktifan `GenerateSalesReturnFromReturnOrderCommand`.

4. **Infrastructure Layer (`btr.infrastructure`):**
   - Pembaruan Dapper queries pada `ReturJualDal` (`Insert`, `Update`, `GetData`) untuk mengikutsertakan kolom `ReturnOrderId`.
   - Implementasi query `ListByReturnOrderId` pada `ReturJualDal`.
   - Pembaruan query pemuatan `ReturnOrderDal` untuk mendukung kebutuhan query monitoring berdasarkan rentang periode.

5. **Presentation Layer WinForms (`btr.distrib`):**
   - `ReturJualForm.cs` & `ReturJualForm.Designer.cs`:
     - Penambahan kontrol input `ReturnOrderIdText`, tombol lookup `ReturnOrderButton`, dan label terkait.
     - Implementasi method `public void LoadReturnOrder(string returnOrderId)` dan event handler `ReturnOrderIdText_Validated`.
     - Implementasi dialog / prompt pemilihan `JenisRetur` untuk order retur heterogen.
     - Penyesuaian `BuildReturJual` agar mengikat nilai `ReturnOrderId`.
     - Penyesuaian `SaveButton_Click` agar menjalankan pembaruan status `BTR_ReturnOrder` via `ReturnOrderStatusWorker` di dalam scope transaksi (`TransHelper.NewScope()`).
     - Penyesuaian `DeleteReturJual` (Void) agar menjalankan evaluasi penurunan/revert status `BTR_ReturnOrder` via `ReturnOrderStatusWorker` di dalam scope transaksi.
   - `ListReturnOrderForm.cs` & `ListReturnOrderForm.Designer.cs`:
     - Form baru monitoring Order Retur dengan pola UI `ListOrderForm.cs`.
     - Filter rentang tanggal (`PeriodeStartDatePicker`, `PeriodeEndDatePicker`) dan teks pencarian.
     - DataGridView menampilkan nomor order, tanggal, customer, sales, driver, status (`Synced`, `Partial`, `Imported`), dan info nomor Retur Jual terkait (BAGUS & RUSAK).
     - Event `DoubleClick` dan context menu: membuka `ReturJualForm` dan memanggil `LoadReturnOrder` (untuk status `Synced`/`Partial`) atau menampilkan dokumen terkait (untuk status `Imported`).
     - Fitur Export to Excel.
   - `MainForm.cs`:
     - Pembaruan menu item `RT3` dari `RT3GenerateReturnOrder` menjadi membuka `ListReturnOrderForm`.
     - Penghapusan ketergantungan pada `GenerateReturnOrderForm.cs`.
   - Deprecasi / penghapusan form lama `GenerateReturnOrderForm.cs` beserta DTO pendukungnya.

## Excluded

- Aplikasi gudang cabang (`BtrReturGudang.Wpf`) dan sinkronisasi `j07-btrade-sync`: alur intake dan pengiriman data ke pusat tetap berjalan seperti semula.
- Cloud API (`j06-pkl-btrade-api`).
- Generator mutasi stok `GenStokReturJualWorker` dan `RemoveRollbackStokWorker`: tidak ada perubahan alur stok, kartu stok, maupun formula mutasi.
- Perubahan penempatan `JenisRetur` pada level item `BTR_ReturJual` (tetap dipertahankan di level header).
- Modul akuntansi / jurnal keuangan (`RT2-Posting Retur`).

---

# 4. Technical Decisions

| Decision ID | Topic | Technical Realization |
| :--- | :--- | :--- |
| **TD-001** | **Skema Relasi ReturnOrderId pada BTR_ReturJual** | Menambahkan kolom `ReturnOrderId VARCHAR(26) NOT NULL DEFAULT('')` ke tabel `BTR_ReturJual`. Menambahkan *filtered non-clustered index* `IX_BTR_ReturJual_ReturnOrderId ON BTR_ReturJual(ReturnOrderId) WHERE ReturnOrderId <> '' WITH (FILLFACTOR=75)`. Pola ini mematuhi prinsip *Minimalist* (tanpa tabel perantara) dan mendukung relasi 1-to-many secara alami (1 `ReturnOrder` dapat menghasilkan hingga 2 dokumen `ReturJual`). |
| **TD-002** | **Isolasi State Machine ReturnOrder via ReturnOrderStatusWorker** | Seluruh logika evaluasi dan transisi status `BTR_ReturnOrder.Status` diisolasi dalam satu worker terdedikasi `ReturnOrderStatusWorker` (`IReturnOrderStatusWorker`). Aturan evaluasi: <br>1. Ambil seluruh distinct `JenisRetur` dari detail item order (`BTR_ReturnOrderItem`).<br>2. Ambil seluruh dokumen `BTR_ReturJual` aktif (`VoidDate = '3000-01-01'`) yang merujuk pada `ReturnOrderId` tersebut.<br>3. Jika seluruh `JenisRetur` yang ada pada order sudah memiliki Retur Jual aktif -> set `Status = 'Imported'`.<br>4. Jika baru sebagian `JenisRetur` yang memiliki Retur Jual aktif -> set `Status = 'Partial'`.<br>5. Jika tidak ada Retur Jual aktif yang tersisa -> kembalikan `Status = 'Synced'`. |
| **TD-003** | **Alur Interaktif & Guided Input pada ReturJualForm** | Mengimplementasikan method `LoadReturnOrder(string returnOrderId)` pada `ReturJualForm.cs` (mengadopsi pola `LoadOrder` pada `FakturForm.cs`):<br>1. Memuat header `ReturnOrder` dan daftar itemnya.<br>2. Mengisi default `CustomerIdText`, `SalesIdText` (jika tersedia), dan `DriverIdText` (jika tersedia). Field `WarehouseIdText` tetap kosong/wajib dipilih manual oleh user kantor pusat untuk menentukan gudang logikal.<br>3. Pemilahan item:<br>&nbsp;&nbsp;a. Jika order homogen (hanya memuat satu jenis retur): otomatis set `JenisReturCombo` dan muat seluruh item ke grid.<br>&nbsp;&nbsp;b. Jika order heterogen (memuat BAGUS dan RUSAK): jika salah satu jenis retur sudah pernah diterbitkan Retur Jual aktif sebelumnya, otomatis pilih jenis retur yang tersisa; jika keduanya belum pernah diterbitkan, tampilkan dialog konfirmasi pemilihan: *"Order Retur memuat item BAGUS dan RUSAK. Pilih jenis retur yang ingin diproses: [BAGUS] [RUSAK]"*.<br>4. Grid memuat item terfilter; user melengkapi harga dan diskon, serta dapat menghapus baris item yang dibatalkan. |
| **TD-004** | **Form Monitoring Representatif (ListReturnOrderForm)** | Membuat `ListReturnOrderForm.cs` menggantikan `GenerateReturnOrderForm.cs` dengan spesifikasi yang meniru kenyamanan `ListOrderForm.cs`:<br>1. Filter rentang tanggal (maksimal 31 hari) dan kotak pencarian multi-kata (No Order, Customer, Salesman).<br>2. Menampilkan DataGridView dengan pewarnaan status visual: `Synced` (standar), `Partial` (kuning muda), `Imported` (hijau muda).<br>3. Menampilkan kolom informasi dokumen Retur Jual yang sudah terbit (`ReturJualBagusCode`, `ReturJualRusakCode`).<br>4. Navigasi aksi: Double-click pada baris `Synced` / `Partial` langsung membuka `ReturJualForm` dan memicu `LoadReturnOrder(returnOrderId)`. Double-click pada baris `Imported` membuka Retur Jual yang telah terbit. |
| **TD-005** | **Atomisitas Transaksi Simpan dan Void** | Operasi simpan dan void Retur Jual wajib atomik dengan pembaharuan status `ReturnOrder`: <br>1. Pada `SaveButton_Click`: penyimpanan `ReturJual`, pembuatan mutasi stok (`_genStokReturJualWorker`), dan eksekusi `_returnOrderStatusWorker.UpdateStatus(returJual.ReturnOrderId)` dieksekusi di dalam satu `TransHelper.NewScope()`.<br>2. Pada `DeleteReturJual`: pengeset-an `VoidDate`, pembatalan mutasi stok (`_removeRollbackStokWorker`), dan eksekusi `_returnOrderStatusWorker.UpdateStatus(returJual.ReturnOrderId)` dieksekusi di dalam satu `TransHelper.NewScope()`. |
| **TD-006** | **Pelengkapan SalesPerson & Driver Terpadu** | Pelengkapan data Salesman dan Driver yang belum terisi saat pencatatan di gudang dilakukan langsung di form transaksi `ReturJualForm.cs`. Saat penyimpanan Retur Jual berhasil, jika `BTR_ReturnOrder` sebelumnya belum memiliki Salesman atau Driver, `ReturnOrderStatusWorker` menyinkronkan data tersebut kembali ke header `BTR_ReturnOrder`. |
| **TD-007** | **Pembersihan dan Penggantian Komponen Lama** | Menghapus atau mendeprecate form `GenerateReturnOrderForm.cs` beserta kelas DTO khususnya (`GenerateReturnOrderFormItemDto`, `GenerateReturnOrderFormOrderDto`, `GenerateReturnOrderFormResultDto`). Menonaktifkan `GenerateSalesReturnFromReturnOrderCommand` dan mengalihkan routing menu ribbon `MainForm` ke `ListReturnOrderForm`. |

---

# 5. Component Responsibilities

| Component | Layer / Project | Technical Responsibility |
| :--- | :--- | :--- |
| `Alter_BTR_ReturJual_Add_ReturnOrderId.sql` | `btr.sql` | Skrip migrasi DDL idempotent: menambahkan kolom `ReturnOrderId VARCHAR(26)` dan filtered index `IX_BTR_ReturJual_ReturnOrderId` pada tabel `BTR_ReturJual`. |
| `BTR_ReturJual.sql` | `btr.sql` | Script DDL baseline tabel `BTR_ReturJual` yang telah diperbarui dengan kolom `ReturnOrderId`. |
| `ReturJualModel` | `btr.domain` | Model domain entitas Retur Jual; mengekspos properti `ReturnOrderId`. |
| `ReturnOrderStatus` | `btr.domain` | Definisi konstanta nilai status resmi `ReturnOrder` (`Synced`, `Partial`, `Imported`). |
| `IReturJualDal` / `ReturJualDal` | `btr.application` / `btr.infrastructure` | Data Access Layer Retur Jual: mengelola persistensi kolom `ReturnOrderId` pada `Insert`, `Update`, `GetData`, serta method baru `ListByReturnOrderId(string returnOrderId)`. |
| `IReturJualBuilder` / `ReturJualBuilder` | `btr.application` | Fluent builder entitas `ReturJualModel`; mengekspos method `.ReturnOrderId(string returnOrderId)`. |
| `IReturnOrderStatusWorker` / `ReturnOrderStatusWorker` | `btr.application` | Worker domain/aplikasi; mengevaluasi kuadran `JenisRetur` yang ada pada detail order retur terhadap Retur Jual aktif, lalu memperbarui status `BTR_ReturnOrder` (`Synced`, `Partial`, `Imported`) secara deterministik. |
| `ListReturnOrderMonitoringQuery` / `Handler` | `btr.application` | Handler query monitoring; mengambil data daftar Order Retur beserta informasi dokumen Retur Jual terkait (`Bagus` & `Rusak`) dalam rentang tanggal tertentu. |
| `ListReturnOrderDto` | `btr.distrib` / `btr.application` | Data Transfer Object untuk tampilan grid monitoring pada `ListReturnOrderForm`. |
| `ReturJualForm` | `btr.distrib` | Form entri transaksi Retur Jual; menyediakan kontrol `ReturnOrderId`, navigasi `LoadReturnOrder`, validasi kondisi barang (homogen vs heterogen), penentuan warehouse logikal, dan koordinasi transaksi simpan/void. |
| `ListReturnOrderForm` | `btr.distrib` | Form WinForms monitoring daftar Order Retur; menyediakan filter periode, pencarian, pewarnaan status, ekspor Excel, dan navigasi *double-click* ke `ReturJualForm`. |
| `MainForm` | `btr.distrib` | Shell utama MDI; mengarahkan klik menu `RT3` ke `ListReturnOrderForm` dan menyediakan referensi pembukaan `ReturJualForm`. |

---

# 6. Integration Design

| Source | Target | Interaction / Protocol | Purpose |
| :--- | :--- | :--- | :--- |
| `MainForm` Menu `RT3` | `ListReturnOrderForm` | In-process WinForms MDI activation | Membuka form monitoring daftar Order Retur dari ribbon menu. |
| `ListReturnOrderForm` | `IMediator` / `ListReturnOrderMonitoringQuery` | In-process MediatR query call | Mengambil data Order Retur dan status Retur Jual terkait berdasarkan rentang periode. |
| `ListReturnOrderForm` | `ReturJualForm.LoadReturnOrder` | In-process Form method invocation | Membuka form Retur Jual dan memuat data Order Retur saat user melakukan *double-click* pada baris berstatus `Synced` / `Partial`. |
| `ListReturnOrderForm` | `ReturJualForm.ShowReturJual` | In-process Form method invocation | Membuka dokumen Retur Jual yang sudah terbit saat user melakukan *double-click* pada baris berstatus `Imported`. |
| `ReturJualForm.LoadReturnOrder` | `ReturnOrderBuilder` / `IReturnOrderDal` & `IReturnOrderItemDal` | In-process repository query | Menarik informasi header dan detail item barang dari `BTR_ReturnOrder` dan `BTR_ReturnOrderItem`. |
| `ReturJualForm.LoadReturnOrder` | `IReturJualDal.ListByReturnOrderId` | In-process DAL query | Memeriksa apakah sebagian jenis retur dari order tersebut sudah pernah diterbitkan Retur Jual aktif sebelumnya. |
| `ReturJualForm.SaveButton_Click` | `IReturJualWriter.Save` | In-process application writer call | Menyimpan header `BTR_ReturJual` (termasuk `ReturnOrderId`), item, dan diskon ke database. |
| `ReturJualForm.SaveButton_Click` | `IGenStokReturJualWorker.Execute` | In-process stock worker call | Membentuk mutasi penambahan stok berdasarkan `JenisRetur` dan `WarehouseId` header dokumen. |
| `ReturJualForm.SaveButton_Click` | `IReturnOrderStatusWorker.Execute` | In-process domain worker call | Mengevaluasi dan memperbarui kolom `Status` pada `BTR_ReturnOrder` di dalam transaksi simpan. |
| `ReturJualForm.DeleteReturJual` (Void) | `IReturJualDal.Update` & `IRemoveRollbackStokWorker.Execute` | In-process DAL & worker call | Menandai `VoidDate` dan membatalkan mutasi kartu stok retur jual. |
| `ReturJualForm.DeleteReturJual` (Void) | `IReturnOrderStatusWorker.Execute` | In-process domain worker call | Mengevaluasi ulang ketersediaan Retur Jual aktif dan me-revert status `BTR_ReturnOrder` (`Partial` atau `Synced`) di dalam transaksi void. |

---

# 7. Data Ownership

| Data Element | Authoritative Owner | Consumer / Referencer |
| :--- | :--- | :--- |
| **Order Retur Asal (`BTR_ReturnOrder`, `BTR_ReturnOrderItem`)** | Agregat `ReturnOrder` (`j05-btr-distrib`) | Dibaca oleh `ReturJualForm` dan `ListReturnOrderForm`. Nilai status `Status` diperbarui secara eksklusif oleh `ReturnOrderStatusWorker`. |
| **Status Lifecycle Order Retur (`Status`)** | Agregat `ReturnOrder` (`j05-btr-distrib`) | Direfleksikan pada `ListReturnOrderForm` untuk kontrol operasional realisasi (`Synced`, `Partial`, `Imported`). |
| **Dokumen Retur Jual (`BTR_ReturJual`, `BTR_ReturJualItem`)** | Agregat `ReturJual` (`j05-btr-distrib`) | Dokumen transaksi komersial dan akuntansi pusat; memiliki referensi opsional `ReturnOrderId` ke dokumen asal. |
| **Gudang Logikal (`BTR_Warehouse`)** | Modul Master Warehouse (`j05-btr-distrib`) | Ditentukan oleh user pada `ReturJualForm.WarehouseIdText` saat realisasi; tidak dipetakan otomatis dari kode depo gudang. |
| **Kondisi Barang (`JenisRetur`)** | Header `BTR_ReturJual` | Menentukan jenis mutasi persediaan pada kartu stok (`GenStokReturJualWorker`). Bersumber dari filter item `BTR_ReturnOrderItem.JenisRetur`. |

---

# 8. Database Design

## Modified Tables

### `BTR_ReturJual`

Tabel transaksi Retur Jual dimodifikasi dengan penambahan kolom referensi dokumen order retur:

| Column | Change Type | Data Type | Nullable | Default | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ReturnOrderId` | **ADD** | `VARCHAR(26)` | NO | `''` | ULID / ID dokumen asal `BTR_ReturnOrder`. Berisi string kosong jika retur dibuat manual. |

### Indexes pada `BTR_ReturJual`

| Index Name | Type | Key Columns | Filter Condition | Options |
| :--- | :--- | :--- | :--- | :--- |
| `IX_BTR_ReturJual_ReturnOrderId` | Non-Clustered | `ReturnOrderId`, `VoidDate` | `WHERE ReturnOrderId <> ''` | `WITH (FILLFACTOR = 75)` |

*Catatan: Kolom `VoidDate` disertakan untuk mempercepat evaluasi Retur Jual aktif (`VoidDate = '3000-01-01'`) saat penentuan transisi status order retur.*

## Relationships

- Relasi logikal dari `BTR_ReturJual.ReturnOrderId` ke `BTR_ReturnOrder.ReturnOrderId` berkarakteristik **0..2 : 1**:
  - Satu dokumen `BTR_ReturnOrder` dapat dirujuk oleh **maksimal 2** dokumen `BTR_ReturJual` aktif (1 untuk item kondisi BAGUS dan 1 untuk item kondisi RUSAK).
  - Retur Jual yang dibuat secara manual (tanpa melalui order gudang) memiliki nilai default `ReturnOrderId = ''`.

## Migration Considerations

1. **Idempotent Migration Script (`Alter_BTR_ReturJual_Add_ReturnOrderId.sql`):**
   ```sql
   IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BTR_ReturJual' AND type = 'U')
   BEGIN
       -- 1. Tambah kolom ReturnOrderId jika belum ada
       IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('BTR_ReturJual') AND name = 'ReturnOrderId')
       BEGIN
           ALTER TABLE BTR_ReturJual 
           ADD ReturnOrderId VARCHAR(26) NOT NULL 
               CONSTRAINT DF_BTR_ReturJual_ReturnOrderId DEFAULT('');
       END;

       -- 2. Buat index IX_BTR_ReturJual_ReturnOrderId jika belum ada
       IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('BTR_ReturJual') AND name = 'IX_BTR_ReturJual_ReturnOrderId')
       BEGIN
           CREATE NONCLUSTERED INDEX IX_BTR_ReturJual_ReturnOrderId
               ON BTR_ReturJual(ReturnOrderId, VoidDate)
               WHERE ReturnOrderId <> ''
               WITH (FILLFACTOR = 75);
       END;
   END;
   GO
   ```

2. **Rollout & Backfill:**
   - Skrip migrasi dieksekusi sekali pada database operasional distribusi (`btr.sql`).
   - Tidak memerlukan *data backfill* karena seluruh dokumen `BTR_ReturJual` historis memiliki nilai default `ReturnOrderId = ''`.
   - Script baseline `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturJual.sql` diperbarui agar instalasi lingkungan baru langsung menyertakan kolom dan index ini.

---

# 9. Cross-Cutting Concerns

| Concern | Technical Treatment |
| :--- | :--- |
| **Integritas Data & Transaksionalitas** | Simpan dokumen `BTR_ReturJual`, pembentukan mutasi stok (`_genStokReturJualWorker`), dan pembaruan status `BTR_ReturnOrder` (`_returnOrderStatusWorker`) dibungkus dalam satu scope transaksi (`using (var trans = TransHelper.NewScope())`). Kegagalan pada salah satu tahapan akan membatalkan seluruh operasi secara utuh.<br>Perlakuan yang sama berlaku pada saat pembatalan (`DeleteReturJual`): pencatatan `VoidDate`, rollback mutasi kartu stok, dan evaluasi ulang status `BTR_ReturnOrder` dieksekusi dalam satu transaksi atomik. |
| **State Machine Deterministik** | Status `BTR_ReturnOrder` hanya boleh bernilai `'Synced'`, `'Partial'`, atau `'Imported'`. Evaluasi transisi dilakukan berbasis data fisik yang ada di database (apakah seluruh `JenisRetur` yang terdaftar pada order telah diterbitkan Retur Jual-nya dan belum di-void). Tidak ada ketergantungan pada memori aplikasi atau session. |
| **Pencegahan Redundansi & Concurrency** | Saat user memasukkan `ReturnOrderId` pada `ReturJualForm`, sistem memverifikasi apakah seluruh jenis retur pada order tersebut sudah terbit. Jika sudah terbit semua (status `Imported`), sistem menolak pemuatan dengan pesan yang jelas. Jika order heterogen dan satu jenis retur sudah pernah dibuat Retur Jual-nya, sistem secara deterministik hanya mengizinkan jenis retur yang tersisa. |
| **UX & Ergonomi Input Terpandu** | Pada order retur heterogen yang kedua jenis returnya belum pernah diproses, sistem menyajikan dialog prompt ringkas (*modal prompt*) kepada user untuk menentukan pilihan (BAGUS atau RUSAK). Pilihan ini langsung mengunci `JenisReturCombo` dan memfilter baris item yang sesuai ke grid, mencegah kesalahan input barang silang jenis retur. |
| **Kinerja & Responsivitas Monitoring** | Query pada `ListReturnOrderForm` dibatasi rentang tanggal maksimal 31 hari untuk menjaga waktu respon database. Filtered index pada `BTR_ReturJual(ReturnOrderId, VoidDate)` memastikan join/lookup informasi status Retur Jual berlangsung secara efisien dalam hitungan milidetik. |

---

# 10. Implementation Constraints

1. **Target Framework & Bahasa:** Seluruh implementasi pada `j05-btr-distrib` harus kompatibel dengan `.NET Framework 4.7.1` dan C# 7.3/8.0.
2. **Kueri Data (Dapper):** Seluruh akses database harus menggunakan parameterized query T-SQL via Dapper / `SqlConnection`; tidak menggunakan Entity Framework atau query builders pihak ketiga.
3. **Integritas Arsitektur Header Retur Jual:** Struktur atribut `JenisRetur` pada header `BTR_ReturJual` dan logika pada `GenStokReturJualWorker` tidak boleh diubah menjadi granularitas detail/baris demi mencegah risiko regresi pada modul persediaan.
4. **Isolasi Logika Status:** Evaluasi status transisi `ReturnOrder` harus terpusat pada `ReturnOrderStatusWorker` dan tidak boleh diduplikasi secara tersebar pada event handler UI form.
5. **UI Consistency:** Kontrol UI WinForms pada `ListReturnOrderForm.cs` dan dialog pendukung harus mengikuti standar desain visual BTR (font Segoe UI / Consolas, flat buttons, warna header standar).
6. **Deprecasi Bersih:** Komponen lama `GenerateReturnOrderForm.cs` dan use case `GenerateSalesReturnFromReturnOrderCommand.cs` harus dideprecate / dihapus dari registrasi DI agar tidak menyisakan *dead code*.

---

# 11. Acceptance Conditions

1. Skrip migrasi DDL `Alter_BTR_ReturJual_Add_ReturnOrderId.sql` dan baseline script `BTR_ReturJual.sql` terverifikasi menyertakan kolom `ReturnOrderId VARCHAR(26)` dan filtered index `IX_BTR_ReturJual_ReturnOrderId`.
2. Properti `ReturnOrderId` tersedia pada model domain `ReturJualModel`, dan `ReturJualDal` berhasil melakukan operasi `Insert`, `Update`, `GetData`, serta `ListByReturnOrderId`.
3. Worker `ReturnOrderStatusWorker` terimplementasi dan lulus pengujian logika:
   - Order homogen: setelah 1 Retur Jual disimpan -> status berubah menjadi `'Imported'`.
   - Order heterogen: setelah Retur Jual pertama disimpan -> status berubah menjadi `'Partial'`; setelah Retur Jual kedua disimpan -> status berubah menjadi `'Imported'`.
   - Void Retur Jual pada order dengan status `'Imported'`: jika masih ada 1 Retur Jual aktif lain -> status turun menjadi `'Partial'`; jika seluruh Retur Jual void -> status kembali ke `'Synced'`.
4. Pada `ReturJualForm.cs`:
   - Kontrol `ReturnOrderIdText`, `ReturnOrderButton`, dan fungsi `LoadReturnOrder(returnOrderId)` berfungsi menarik header Customer, Salesman, dan Driver dari order retur.
   - Kontrol `WarehouseIdText` tetap meminta pemilihan gudang logikal secara eksplisit oleh pengguna.
   - Pemilihan `JenisRetur` otomatis terset pada order homogen, dan menampilkan dialog prompt pilihan pada order heterogen yang belum diproses.
   - Grid detail item hanya memuat item barang yang sesuai dengan `JenisRetur` yang dipilih.
   - User dapat mengedit harga/diskon serta menghapus baris item yang dibatalkan.
5. Transaksi simpan (`SaveButton_Click`) dan pembatalan (`DeleteReturJual`) pada `ReturJualForm.cs` berhasil memperbarui status `BTR_ReturnOrder` secara atomik dalam satu scope transaksi.
6. Form monitoring baru `ListReturnOrderForm.cs` tampil dengan benar melalui menu `RT3` pada `MainForm`:
   - Menampilkan daftar dokumen Order Retur berdasarkan rentang periode tanggal (maks 31 hari) dan kata kunci pencarian.
   - Menampilkan status penyelesaian (`Synced`, `Partial`, `Imported`) dengan pewarnaan baris/cell yang membedakan.
   - Menampilkan nomor Retur Jual yang telah diterbitkan untuk kondisi BAGUS dan RUSAK.
   - Aksi *double-click* pada baris order berstatus `Synced` atau `Partial` langsung membuka `ReturJualForm` dan mengeksekusi `LoadReturnOrder(returnOrderId)`.
   - Aksi *double-click* pada baris order berstatus `Imported` membuka dokumen Retur Jual yang telah terbit.
   - Fitur ekspor data ke Excel berfungsi dengan format rapi.
7. Komponen `GenerateReturnOrderForm.cs` dihapus/dideprecate dari aplikasi tanpa menimbulkan error kompilasi pada solusi `j05-btr-distrib`.
8. Seluruh automated unit dan integration tests yang relevan lulus kompilasi dan eksekusi.