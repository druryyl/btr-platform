---
Title: Implementation Plan - Realisasi Order Retur Jual ke Retur Jual via ReturJualForm dan List Monitoring Return Order
Code: IS003-RETURN-ORDER-REALISASI-RETUR-JUAL
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-11-09
Status: NOT-STARTED
Execution Approval: APPROVED
---

# 1. Objective

Menerapkan realisasi teknis untuk mengalihkan mekanisme realisasi Order Retur Jual (`BTR_ReturnOrder`) menjadi dokumen fisik/akuntansi Retur Jual (`BTR_ReturJual`) di sistem distribusi pusat (`j05-btr-distrib`). Perubahan ini merealisasikan change request `BTR-RETURN-ORDER-REALISASI-001` dengan mengadopsi tiga prinsip arsitektur: **Pragmatis**, **Explicit**, dan **Minimalist**:

1. Menggantikan proses batch otomatis yang kaku pada `GenerateReturnOrderForm.cs` dengan alur interaktif dan terpandu langsung pada `ReturJualForm.cs` (mengadopsi pola hubungan `OrderJualId` pada `FakturForm.cs`).
2. Menyimpan referensi dokumen asal secara eksplisit melalui penambahan kolom `ReturnOrderId VARCHAR(26)` pada tabel `BTR_ReturJual` yang diindeks dengan *filtered index*.
3. Menyediakan form monitoring representatif `ListReturnOrderForm.cs` (mengadopsi arsitektur `ListOrderForm.cs`) untuk memantau status penyelesaian order retur (`Synced`, `Partial`, `Imported`) serta visualisasi nomor Retur Jual terkait, dilengkapi navigasi *double-click* langsung ke form transaksi.
4. Menjaga stabilitas modul persediaan dan generator mutasi stok (`GenStokReturJualWorker`) dengan mempertahankan atribut `JenisRetur` pada level header dokumen `BTR_ReturJual`.
5. Menerapkan manajemen *lifecycle state machine* dua arah yang deterministik pada `BTR_ReturnOrder.Status` via `ReturnOrderStatusWorker` (transisi maju saat penyimpanan Retur Jual, dan *revert* status saat pembatalan/void Retur Jual) yang dibungkus dalam transaksi atomik (`TransHelper.NewScope()`).

Planning Mode: FEATURE-PLANNING

Referenced artifacts:
- ISSUE: `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ISSUE.md` (`BTR-RETURN-ORDER-REALISASI-001`)
- FEASIBILITY-ASSESSMENT: `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-FEASIBILITY.md` (Status: `READY-FOR-PLANNING`)
- ARCHITECTURE: `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ARCHITECTURE.md` (`BTR-RETURN-ORDER-REALISASI-001`)
- Baseline ARCHITECTURE: `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`
- Baseline DOMAIN: `docs/work/return-order/RETURN-ORDER-DOMAIN.md`

Architecture Applicability: ARCHITECTURE-REQUIRED  
Target capability architecture is defined and authoritative in `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ARCHITECTURE.md`.

---

# 2. Planning Scope

Lingkup pekerjaan implementasi mencakup modifikasi skema database DDL, model domain, data access layer (DAL), business worker untuk state machine, kueri monitoring, pengembangan UI WinForms (`ReturJualForm` & `ListReturnOrderForm`), integrasi menu `MainForm`, pembersihan komponen legacy, serta pengujian otomatis pada repositori `src/j05-btr-distrib`:

1. **Database Schema & Migration (`btr.sql`):**
   - Pembuatan skrip migrasi idempotent `Alter_BTR_ReturJual_Add_ReturnOrderId.sql` pada `btr.sql/Scripts/` untuk menambahkan kolom `ReturnOrderId VARCHAR(26) NOT NULL DEFAULT('')` dan filtered index `IX_BTR_ReturJual_ReturnOrderId ON BTR_ReturJual(ReturnOrderId, VoidDate) WHERE ReturnOrderId <> '' WITH (FILLFACTOR = 75)`.
   - Pembaruan skrip baseline DDL pada `btr.sql/Tables/InventoryContext/BTR_ReturJual.sql`.

2. **Domain Layer (`btr.domain`):**
   - Penambahan properti `ReturnOrderId` pada model entitas `ReturJualModel` (`btr.domain/InventoryContext/ReturJualAgg/ReturJualModel.cs`).
   - Standardisasi konstanta nilai status resmi `ReturnOrderStatus` (`Synced`, `Partial`, `Imported`) pada agregat `ReturnOrder` (`btr.domain/InventoryContext/ReturnOrderAgg/ReturnOrderStatus.cs`).

3. **Data Access Layer & Application Builders (`btr.application` & `btr.infrastructure`):**
   - Penambahan method query `IEnumerable<ReturJualModel> ListByReturnOrderId(string returnOrderId);` pada interface `IReturJualDal`.
   - Pembaruan implementasi `ReturJualDal` untuk memetakan kolom `ReturnOrderId` pada `Insert`, `Update`, `GetData`, serta implementasi method `ListByReturnOrderId`.
   - Ekstensi `IReturJualBuilder` dan `ReturJualBuilder` untuk mendukung penetapan `ReturnOrderId(string returnOrderId)`.

4. **Business Logic & State Machine (`btr.application`):**
   - Pembuatan worker baru `IReturnOrderStatusWorker` / `ReturnOrderStatusWorker` di `btr.application/InventoryContext/ReturnOrderAgg/Workers/` untuk mengevaluasi status dokumen `BTR_ReturnOrder` berdasarkan perbandingan distinct `JenisRetur` pada detail order retur terhadap dokumen aktif `BTR_ReturJual` (`VoidDate = '3000-01-01'`).
   - Penanganan sinkronisasi data Salesman dan Driver ke header `BTR_ReturnOrder` jika sebelumnya kosong.
   - Registrasi service pada ServiceCollection.

5. **Monitoring Query & DTO (`btr.application`):**
   - Pembuatan DTO `ListReturnOrderDto` untuk visualisasi monitoring.
   - Pembuatan query `ListReturnOrderMonitoringQuery` dan handlernya `ListReturnOrderMonitoringHandler` untuk menarik daftar order retur dalam rentang periode (maksimal 31 hari) beserta nomor dokumen Retur Jual terkait (`ReturJualBagusCode`, `ReturJualRusakCode`).

6. **Interactive Presentation UI (`btr.distrib`):**
   - `ReturJualForm.cs` & `ReturJualForm.Designer.cs`:
     - Penambahan kontrol `ReturnOrderIdText`, `ReturnOrderButton`, dan event handler `ReturnOrderIdText_Validated`.
     - Implementasi method `public void LoadReturnOrder(string returnOrderId)` dengan validasi status, pengisian default customer/sales/driver, keharusan memilih gudang logikal (`WarehouseIdText`), pemilahan otomatis untuk order homogen, dan modal prompt interaktif untuk order heterogen yang belum diproses.
     - Pengikatan `ReturnOrderId` pada `BuildReturJual()`.
     - Pembungkusan transaksi atomik `TransHelper.NewScope()` pada `SaveButton_Click` dan pembatalan `DeleteReturJual` bersama eksekusi `ReturnOrderStatusWorker.UpdateStatus(returJual.ReturnOrderId)`.
   - `ListReturnOrderForm.cs` & `ListReturnOrderForm.Designer.cs`:
     - Pembuatan form baru monitoring Order Retur mengadopsi pola arsitektur `ListOrderForm.cs`.
     - Filter rentang periode tanggal (maks 31 hari) dan pencarian kata kunci multi-field.
     - DataGridView dengan pewarnaan status visual: `Synced` (standar), `Partial` (kuning muda), `Imported` (hijau muda).
     - Kolom referensi Retur Jual BAGUS dan RUSAK.
     - Interaksi *double-click* dan context menu: membuka `ReturJualForm` dan memicu `LoadReturnOrder` (untuk status `Synced`/`Partial`) atau menampilkan Retur Jual terbit (untuk status `Imported`).
     - Ekspor ke Excel via ClosedXML.
   - `MainForm.cs`:
     - Pengalihan klik menu ribbon `RT3` dari `GenerateReturnOrderForm` ke `ListReturnOrderForm`.
     - Pembersihan / deprecasi form usang `GenerateReturnOrderForm.cs` beserta DTO pendukung dan command `GenerateSalesReturnFromReturnOrderCommand`.

7. **Automated Verification (`btr.test`):**
   - Unit testing untuk `ReturnOrderStatusWorker` mencakup transisi maju (homogen & heterogen), transisi mundur/revert saat void, serta idempotensi status.
   - Unit/integration testing untuk kueri monitoring `ListReturnOrderMonitoringQuery`.

### Out of Scope:
- Modifikasi pada aplikasi gudang cabang (`BtrReturGudang.Wpf`) maupun sinkronisasi `j07-btrade-sync`.
- Modifikasi pada Cloud API (`j06-pkl-btrade-api`).
- Perubahan formula mutasi stok persediaan pada `GenStokReturJualWorker` dan `RemoveRollbackStokWorker`.
- Modifikasi penempatan atribut `JenisRetur` (tetap berada di header `BTR_ReturJual`).
- Perubahan pada modul akuntansi/jurnal posting (`RT2-Posting Retur`).

---

# 3. Dependencies

Semua perubahan berada dalam repository distribusi kantor pusat: `src/j05-btr-distrib`.

### Dependency Rules:
- `Depends On` mendeklarasikan prasyarat teknis implementasi antar-slice.
- Nilai ketergantungan hanya merujuk pada `Slice ID` yang valid di dalam rencana ini.
- Prasyarat terpenuhi ketika slice yang dirujuk memiliki status implementasi `IMPLEMENTED`.
- Kepuasan ketergantungan tidak mensyaratkan status review `GO` (review beroperasi secara independen).
- Slices yang tidak saling bergantung dapat diimplementasikan secara paralel.

---

# 4. Progress Summary

| Phase | Implementation Status | Review Status | Progress |
| :--- | :--- | :--- | :--- |
| P1 - Database Schema & Domain Foundation | NOT-STARTED | NOT-REVIEWED | 0/2 |
| P2 - Persistence, State Machine & Application Query | NOT-STARTED | NOT-REVIEWED | 0/3 |
| P3 - WinForms Presentation Layer & Shell Integration | NOT-STARTED | NOT-REVIEWED | 0/3 |
| P4 - Automated Testing & Verification | NOT-STARTED | NOT-REVIEWED | 0/1 |

---

# 5. Phases

## P1 - Database Schema & Domain Foundation

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P1-S01

Title: Database Schema Migration & Baseline DDL Updates

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Menyediakan skrip migrasi T-SQL idempotent untuk menambahkan kolom `ReturnOrderId VARCHAR(26)` dan filtered index `IX_BTR_ReturJual_ReturnOrderId` pada tabel `BTR_ReturJual`, serta memperbarui baseline DDL tabel.

Depends On: None

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Skrip migrasi idempotent `src/j05-btr-distrib/btr.sql/Scripts/Alter_BTR_ReturJual_Add_ReturnOrderId.sql` tersedia dan dapat dieksekusi berulang kali tanpa error.
2. Kolom `ReturnOrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_ReturJual_ReturnOrderId DEFAULT('')` ditambahkan ke `BTR_ReturJual`.
3. Filtered index `IX_BTR_ReturJual_ReturnOrderId` dibuat pada `BTR_ReturJual(ReturnOrderId, VoidDate) WHERE ReturnOrderId <> '' WITH (FILLFACTOR = 75)`.
4. File baseline DDL `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturJual.sql` diperbarui mencakup kolom dan definisi index baru tersebut.

Notes:
Sesuai Technical Decision TD-001 pada dokumen arsitektur. Kolom `VoidDate` disertakan dalam key index untuk mempercepat filter Retur Jual aktif (`VoidDate = '3000-01-01'`).

---

### P1-S02

Title: Domain Model & Status Constant Extensions

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Menambahkan properti `ReturnOrderId` pada model domain `ReturJualModel` dan menstandarisasi konstanta nilai status resmi `ReturnOrderStatus` pada agregat `ReturnOrder`.

Depends On: None

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Properti `public string ReturnOrderId { get; set; }` ditambahkan pada `btr.domain/InventoryContext/ReturJualAgg/ReturJualModel.cs` dengan default string kosong (`string.Empty`).
2. Kelas / struct konstanta status `ReturnOrderStatus` dibuat pada `btr.domain/InventoryContext/ReturnOrderAgg/ReturnOrderStatus.cs`:
   - `public const string Synced = "Synced";`
   - `public const string Partial = "Partial";`
   - `public const string Imported = "Imported";`
3. Proyek `btr.domain` berhasil dikompilasi tanpa error.

Notes:
Dapat dikerjakan secara paralel dengan `P1-S01`.

---

## P2 - Persistence, State Machine & Application Query

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P2-S03

Title: ReturJual DAL & Fluent Builder Extensions

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Memperbarui DAL `ReturJualDal` dan fluent builder `ReturJualBuilder` untuk mendukung persistensi dan pembacaan field `ReturnOrderId`, serta menyediakan query lookup Retur Jual berdasarkan `ReturnOrderId`.

Depends On: P1-S01, P1-S02

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Interface `IReturJualDal` (`btr.application/InventoryContext/ReturJualAgg/Contracts/IReturJualDal.cs`) diperkaya dengan method:
   - `IEnumerable<ReturJualModel> ListByReturnOrderId(string returnOrderId);`
2. Kelas `ReturJualDal` (`btr.infrastructure/InventoryContext/ReturJualAgg/ReturJualDal.cs`) diperbarui:
   - Parameter dan kolom `ReturnOrderId` disertakan pada query `Insert` dan `Update`.
   - Query `GetData` memetakan kolom `ReturnOrderId` ke `ReturJualModel`.
   - Implementasi `ListByReturnOrderId` mengeksekusi T-SQL query mengambil seluruh data Retur Jual berdasarkan `ReturnOrderId`.
3. Interface `IReturJualBuilder` dan kelas `ReturJualBuilder` (`btr.application/InventoryContext/ReturJualAgg/Workers/ReturJualBuilder.cs`) diperbarui dengan method:
   - `IReturJualBuilder ReturnOrderId(string returnOrderId);`
   - Pemetaan `ReturnOrderId` saat memuat (`Load`) entitas yang ada.
4. Solusi `j05-btr-distrib` berhasil dikompilasi.

Notes:
Memastikan query DAL menggunakan parameterized queries via Dapper untuk menjaga keamanan dan performa kueri.

---

### P2-S04

Title: ReturnOrderStatusWorker & State Machine Implementation

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Mengimplementasikan worker terisolasi `ReturnOrderStatusWorker` untuk mengevaluasi kondisi item order retur terhadap dokumen Retur Jual aktif dan memperbarui status `BTR_ReturnOrder.Status` secara deterministik dua arah (maju dan revert).

Depends On: P2-S03

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Interface `IReturnOrderStatusWorker` dan kelas `ReturnOrderStatusWorker` dibuat pada `btr.application/InventoryContext/ReturnOrderAgg/Workers/`:
   - Method `string UpdateStatus(string returnOrderId, SqlConnection conn = null, SqlTransaction trans = null)` (atau mendukung ambient transaction scope).
2. Logika evaluasi status:
   - Mengambil distinct `JenisRetur` dari daftar item order retur terkait via `IReturnOrderItemDal`.
   - Mengambil seluruh Retur Jual aktif (`VoidDate = '3000-01-01'`) yang memiliki `ReturnOrderId` tersebut via `IReturJualDal.ListByReturnOrderId`.
   - Jika semua `JenisRetur` pada order retur telah memiliki minimal satu Retur Jual aktif -> set `Status = ReturnOrderStatus.Imported`.
   - Jika baru sebagian `JenisRetur` yang memiliki Retur Jual aktif -> set `Status = ReturnOrderStatus.Partial`.
   - Jika tidak ada Retur Jual aktif tersisa (misal seluruh retur jual di-void) -> set `Status = ReturnOrderStatus.Synced`.
3. Jika pada `BTR_ReturnOrder` data `SalesPersonId` atau `DriverId` masih kosong, worker menyinkronkan data sales/driver dari Retur Jual ke order retur.
4. Worker didaftarkan ke DI container (`Program.cs`) atau tercakup dalam Scrutor assembly scanning.

Notes:
Sesuai Technical Decision TD-002 dan TD-006. Logika status harus sepenuhnya murni di application layer tanpa ketergantungan UI.

---

### P2-S05

Title: Return Order Monitoring Query & DTO Implementation

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Menyediakan DTO dan MediatR query handler untuk kebutuhan monitoring daftar Order Retur beserta informasi penyelesaian dokumen Retur Jual terkait (BAGUS & RUSAK) dalam rentang periode tanggal.

Depends On: P2-S03

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. DTO `ListReturnOrderDto` dibuat pada `btr.application/InventoryContext/ReturnOrderAgg/` memuat properti:
   - `ReturnOrderId`, `ReturnOrderNo`, `ReturnOrderDate`
   - `CustomerId`, `CustomerName`, `WarehouseCode`, `WarehouseName`
   - `SalesPersonId`, `SalesPersonName`, `DriverId`, `DriverName`
   - `Status`, `TotalItem`
   - `ReturJualBagusCode`, `ReturJualRusakCode`
   - `Note`
2. Query `ListReturnOrderMonitoringQuery` dan handlernya `ListReturnOrderMonitoringHandler` dibuat pada `btr.application/InventoryContext/ReturnOrderAgg/`:
   - Validasi parameter input rentang tanggal `Tgl1` dan `Tgl2` (format `yyyy-MM-dd`, selisih maksimal 31 hari).
   - Menarik data `BTR_ReturnOrder` pada rentang periode.
   - Menggabungkan informasi dokumen Retur Jual aktif terkait (`BTR_ReturJual`) untuk memetakan nomor retur kondisi BAGUS dan RUSAK.
3. Handler terdaftar pada MediatR assembly scanning.

Notes:
Dapat dikerjakan secara paralel dengan `P2-S04`.

---

## P3 - WinForms Presentation Layer & Shell Integration

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P3-S06

Title: Interactive Guided Entry & Atomic Transactions in ReturJualForm

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Menambahkan kontrol referensi Order Retur pada `ReturJualForm`, mengimplementasikan method `LoadReturnOrder(string returnOrderId)` dengan validasi jenis retur terpandu, serta memastikan atomisitas transaksi penyimpanan dan pembatalan (void) bersama pembaruan status Order Retur.

Depends On: P2-S04

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Desain form `ReturJualForm.Designer.cs` diperbarui:
   - Menambahkan kontrol TextBox `ReturnOrderIdText`, tombol browse `ReturnOrderButton`, dan label deskriptif.
2. Method `public void LoadReturnOrder(string returnOrderId)` diimplementasikan pada `ReturJualForm.cs`:
   - Memvalidasi keberadaan order retur; jika status sudah `Imported`, tampilkan pesan penolakan dan hentikan proses.
   - Mengisi otomatis `ReturnOrderIdText`, `CustomerIdText` (dan nama), `SalesIdText` (jika terisi), `DriverIdText` (jika terisi).
   - Memastikan `WarehouseIdText` tetap kosong dan meminta user memilih gudang logikal secara eksplisit.
   - Menangani alur jenis retur:
     - Homogen: otomatis set `JenisReturCombo` sesuai jenis item dan muat seluruh item ke grid `_listItem`.
     - Heterogen: periksa apakah salah satu jenis retur sudah terbit di Retur Jual aktif; jika ya, otomatis pilih jenis retur yang tersisa; jika keduanya belum terbit, tampilkan modal konfirmasi pilihan interaktif bagi user (BAGUS atau RUSAK).
   - Mengisi baris item barang ke grid, memvalidasi baris, dan menghitung total retur.
3. Penyesuaian `BuildReturJual()`:
   - Memanggil `_builder.ReturnOrderId(ReturnOrderIdText.Text)`.
4. Atomisitas Transaksi Simpan & Void:
   - Pada `SaveButton_Click`: pemanggilan `_writer.Save()`, `_genStokReturJualWorker.Execute()`, dan `_returnOrderStatusWorker.UpdateStatus(returJual.ReturnOrderId)` dieksekusi di dalam `using (var trans = TransHelper.NewScope())`.
   - Pada `DeleteReturJual()`: pembaruan `VoidDate`, `_removeRollbackStokWorker.Execute()`, dan `_returnOrderStatusWorker.UpdateStatus(returJual.ReturnOrderId)` dieksekusi di dalam `using (var trans = TransHelper.NewScope())`.
5. Solusi berhasil dikompilasi dan form dapat dibuka tanpa runtime exception.

Notes:
Sesuai Technical Decision TD-003 dan TD-005.

---

### P3-S07

Title: ListReturnOrderForm Monitoring & Navigation

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Membangun form monitoring daftar Order Retur `ListReturnOrderForm.cs` (mengadopsi arsitektur `ListOrderForm.cs`) yang menampilkan status penyelesaian order retur, nomor Retur Jual terkait, ekspor Excel, serta navigasi *double-click* ke `ReturJualForm`.

Depends On: P2-S05, P3-S06

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Form `ListReturnOrderForm.cs` & `ListReturnOrderForm.Designer.cs` dibuat di `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/`:
   - Kontrol filter rentang tanggal (`PeriodeStartDatePicker`, `PeriodeEndDatePicker`) dengan batas maksimal 31 hari.
   - Kontrol pencarian `SearchText` dan tombol `SearchButton` / `RefreshButton`.
   - DataGridView `OrderGrid` yang diikat dengan `SortableBindingList<ListReturnOrderDto>`.
2. Format dan styling tampilan grid:
   - Kolom: No Order, Tgl Order, Customer, Salesman, Driver, Status (`Synced`, `Partial`, `Imported`), No Retur Bagus, No Retur Rusak, Catatan.
   - Pewarnaan baris otomatis pada event `CellFormatting`:
     - `Synced`: default / cornsilk / putih.
     - `Partial`: LightYellow / Khaki muda.
     - `Imported`: PaleGreen.
3. Interaksi Navigasi:
   - Event `DoubleClick` pada baris order atau menu context:
     - Jika status `Synced` atau `Partial`: membuka `ReturJualForm` melalui shell MDI dan memanggil method `returJualForm.LoadReturnOrder(returnOrderId)`.
     - Jika status `Imported`: membuka `ReturJualForm` dalam mode baca dokumen yang telah terbit.
4. Fitur Ekspor Excel:
   - Tombol `ExcelButton` mengekspor daftar baris order retur yang tampil di grid ke file `.xlsx` menggunakan library ClosedXML.
5. Form didaftarkan pada DI container dan dapat dibuka tanpa error.

Notes:
Sesuai Technical Decision TD-004.

---

### P3-S08

Title: Shell Menu Integration & Legacy Component Cleanup

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Menghubungkan menu ribbon `RT3` pada `MainForm.cs` ke form baru `ListReturnOrderForm`, memperbarui judul menu, serta membersihkan form usang `GenerateReturnOrderForm.cs` dan command usang agar tidak menyisakan *dead code*.

Depends On: P3-S07

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Pada `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.cs`:
   - Handler menu `RT3GenerateReturnOrder_Click` diperbarui untuk memanggil `BringMdiChildToFrontIfLoaded<ListReturnOrderForm>()` atau me-resolve `ListReturnOrderForm` dari DI.
   - Label/Text menu RT3 diubah dari *"Generate Return Order"* menjadi *"Monitoring Return Order"*.
2. Pembersihan komponen lama:
   - Form lama `GenerateReturnOrderForm.cs`, `GenerateReturnOrderForm.Designer.cs`, `GenerateReturnOrderForm.resx`, beserta DTO khususnya (`GenerateReturnOrderFormItemDto.cs`, `GenerateReturnOrderFormOrderDto.cs`, `GenerateReturnOrderFormResultDto.cs`) dihapus dari proyek `btr.distrib`.
   - Command lama `GenerateSalesReturnFromReturnOrderCommand.cs` di `btr.application/InventoryContext/ReturnOrderAgg/` dideprecate atau dihapus, dan referensi pemanggilannya dibersihkan.
3. Seluruh proyek `j05-btr-distrib` dikompilasi bersih (Clean & Build) tanpa peringatan referensi hilang atau broken links.

Notes:
Sesuai Technical Decision TD-007.

---

## P4 - Automated Testing & Verification

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P4-S09

Title: Automated Unit & Integration Testing

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

Objective:
Menyediakan automated unit tests dan integration tests untuk menguji kebenaran state machine transisi status Order Retur (`ReturnOrderStatusWorker`), query monitoring, serta memastikan seluruh acceptance conditions terpenuhi tanpa regresi.

Depends On: P2-S04, P2-S05, P3-S06, P3-S08

Repository: `src/j05-btr-distrib`

Completion Criteria:
1. Test fixture `ReturnOrderStatusWorkerTest.cs` dibuat di `src/j05-btr-distrib/btr.test/InventoryContext/`:
   - Menguji transisi order retur homogen: 1 Retur Jual disimpan -> status berubah menjadi `Imported`.
   - Menguji transisi order retur heterogen: Retur Jual BAGUS disimpan -> status menjadi `Partial`; Retur Jual RUSAK disimpan -> status menjadi `Imported`.
   - Menguji pembatalan (void) Retur Jual pada order retur `Imported`: 1 Retur Jual di-void (tersisa 1 aktif) -> status turun menjadi `Partial`; semua Retur Jual di-void -> status kembali menjadi `Synced`.
   - Menguji sinkronisasi SalesPersonId dan DriverId ke order retur.
2. Test fixture `ListReturnOrderMonitoringQueryTest.cs` dibuat di `src/j05-btr-distrib/btr.test/InventoryContext/`:
   - Menguji pemetaan hasil query yang menggabungkan order retur dengan nomor Retur Jual BAGUS dan RUSAK.
   - Menguji validasi rentang tanggal query (maksimal 31 hari).
3. Seluruh unit tests pada `btr.test` lulus dieksekusi (`100% pass`).

Notes:
Memastikan integritas fungsional dan mencegah regresi pada rilis berikutnya.

---

# 6. Change Log

- 2026-11-09: Dokumen IMPLEMENTATION-PLAN awal dibentuk oleh `ica-architect` berdasarkan ARCHITECTURE `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ARCHITECTURE.md` dan FEASIBILITY-ASSESSMENT `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-FEASIBILITY.md` (`READY-FOR-PLANNING`). Struktur rencana diresmikan dan diberi `Execution Approval: APPROVED`.
