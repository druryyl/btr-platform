---
Title: Implementation Plan - Realisasi Order Retur Jual ke Retur Jual via ReturJualForm dan List Monitoring Return Order
Code: IS003-RETURN-ORDER-REALISASI-RETUR-JUAL
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-11-09
Status: COMPLETED
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

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Menambahkan skrip migrasi idempotent `src/j05-btr-distrib/btr.sql/Scripts/Alter_BTR_ReturJual_Add_ReturnOrderId.sql` untuk penambahan kolom `ReturnOrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_ReturJual_ReturnOrderId DEFAULT('')` dan filtered index `IX_BTR_ReturJual_ReturnOrderId` pada `(ReturnOrderId, VoidDate) WHERE ReturnOrderId <> '' WITH (FILLFACTOR = 75)`.
- Memperbarui baseline DDL `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturJual.sql` dengan kolom `ReturnOrderId` dan index `IX_BTR_ReturJual_ReturnOrderId`.
- Menambahkan file skrip migrasi ke `src/j05-btr-distrib/btr.sql/btr.sql.sqlproj` dan memverifikasi build dacpac berhasil (0 warnings, 0 errors).
- Changed files:
  - `src/j05-btr-distrib/btr.sql/Scripts/Alter_BTR_ReturJual_Add_ReturnOrderId.sql`
  - `src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_ReturJual.sql`
  - `src/j05-btr-distrib/btr.sql/btr.sql.sqlproj`

---

### P1-S02

Title: Domain Model & Status Constant Extensions

Implementation Status: IMPLEMENTED  
Review Status: GO

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
- Selesai diimplementasikan.
- Properti `ReturnOrderId` (`string.Empty` default) ditambahkan ke `ReturJualModel.cs`.
- File konstanta `ReturnOrderStatus.cs` dibuat dengan konstanta `Synced`, `Partial`, dan `Imported`.
- `btr.domain.csproj` diperbarui untuk menyertakan `ReturnOrderStatus.cs`.
- Proyek `btr.domain` berhasil dikompilasi dengan 0 warning dan 0 error.

Changed Files:
- `src/j05-btr-distrib/btr.domain/InventoryContext/ReturJualAgg/ReturJualModel.cs`
- `src/j05-btr-distrib/btr.domain/InventoryContext/ReturnOrderAgg/ReturnOrderStatus.cs`
- `src/j05-btr-distrib/btr.domain/btr.domain.csproj`

---

## P2 - Persistence, State Machine & Application Query

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P2-S03

Title: ReturJual DAL & Fluent Builder Extensions

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Menambahkan method `IEnumerable<ReturJualModel> ListByReturnOrderId(string returnOrderId);` pada interface `IReturJualDal`.
- Memperbarui `ReturJualDal`:
  - Menyertakan kolom dan parameter `ReturnOrderId` pada query parameterized Dapper di `Insert` dan `Update`.
  - Memetakan kolom `ReturnOrderId` pada query `GetData` dan `ListData`.
  - Mengimplementasikan method `ListByReturnOrderId(string returnOrderId)` yang mengembalikan daftar `ReturJualModel` yang cocok dengan `ReturnOrderId` secara terurut tanggal descending.
- Memperbarui `IReturJualBuilder` dan `ReturJualBuilder`:
  - Menambahkan method fluent `.ReturnOrderId(string returnOrderId)`.
  - Menginisialisasi `ReturnOrderId = string.Empty` pada `.Create()`.
  - Pemuatan `Load()` otomatis memetakan `ReturnOrderId` karena menggunakan `_returJualDal.GetData(returJualKey)`.
- Solusi `j05-btr-distrib.sln` berhasil dikompilasi dengan 0 error menggunakan MSBuild.

Changed Files:
- `src/j05-btr-distrib/btr.application/InventoryContext/ReturJualAgg/Contracts/IReturJualDal.cs`
- `src/j05-btr-distrib/btr.application/InventoryContext/ReturJualAgg/Workers/ReturJualBuilder.cs`
- `src/j05-btr-distrib/btr.infrastructure/InventoryContext/ReturJualAgg/ReturJualDal.cs`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

### P2-S04

Title: ReturnOrderStatusWorker & State Machine Implementation

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Interface `IReturnOrderStatusWorker` dan implementasi `ReturnOrderStatusWorker` dibuat di `btr.application/InventoryContext/ReturnOrderAgg/Workers/ReturnOrderStatusWorker.cs`.
- Mengimplementasikan `INunaService<string, string>` dengan method `Execute(string req)` dan `UpdateStatus(string returnOrderId, SqlConnection conn = null, SqlTransaction trans = null)`. Mendukung eksekusi dalam ambient transaction scope (`TransHelper.NewScope()`) serta fallback ADO.NET query langsung jika koneksi disediakan.
- Logika state machine deterministik dua arah:
  - Membaca distinct `JenisRetur` dari `BTR_ReturnOrderItem`.
  - Membaca `BTR_ReturJual` aktif (`!x.IsVoid` / `VoidDate = '3000-01-01'`).
  - Menentukan status: `Synced` (0 retur jual aktif tersisa), `Imported` (seluruh `JenisRetur` order retur terpenuhi), atau `Partial` (sebagian terpenuhi).
- Sinkronisasi data `SalesPersonId`, `SalesPersonName`, `DriverId`, `DriverName` dari Retur Jual ke ReturnOrder jika pada ReturnOrder masih kosong.
- Mendaftarkan `IReturnOrderStatusWorker` ke `Program.cs` (`services.AddScoped<IReturnOrderStatusWorker, ReturnOrderStatusWorker>()`) dan menyertakannya dalam `btr.application.csproj`.
- Membuat unit test suite `ReturnOrderStatusWorkerTest.cs` (11 test case) di `btr.test` yang memverifikasi seluruh transisi status maju, revert saat void, dan sinkronisasi sales/driver. Seluruh test berhasil (11 passed).
- Solusi `j05-btr-distrib.sln` berhasil dikompilasi dengan 0 error.

Changed Files:
- `src/j05-btr-distrib/btr.application/InventoryContext/ReturnOrderAgg/Workers/ReturnOrderStatusWorker.cs`
- `src/j05-btr-distrib/btr.application/btr.application.csproj`
- `src/j05-btr-distrib/btr.distrib/Program.cs`
- `src/j05-btr-distrib/btr.test/InventoryContext/ReturnOrderStatusWorkerTest.cs`
- `src/j05-btr-distrib/btr.test/btr.test.csproj`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

### P2-S05

Title: Return Order Monitoring Query & DTO Implementation

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- DTO `ListReturnOrderDto` dibuat pada `btr.application/InventoryContext/ReturnOrderAgg/ListReturnOrderDto.cs` memuat seluruh properti monitoring yang disyaratkan.
- `IReturnOrderDal` dan `ReturnOrderDal` diperbarui untuk mendukung `IListData<ReturnOrderModel, Periode>` melalui parameterized query date range.
- `ListReturnOrderMonitoringQuery` dan `ListReturnOrderMonitoringHandler` dibuat pada `btr.application/InventoryContext/ReturnOrderAgg/ListReturnOrderMonitoringQuery.cs`:
  - Validasi parameter tanggal input (`yyyy-MM-dd`, tanggal awal <= tanggal akhir, rentang <= 31 hari).
  - Penarikan order retur pada periode tanggal.
  - Resolusi nama gudang via `IWarehouseDal`.
  - Penghitungan jumlah item per order retur via `IReturnOrderItemDal`.
  - Pemetaan Retur Jual aktif terkait (`!x.IsVoid`) untuk nomor retur BAGUS (`ReturJualBagusCode`) dan RUSAK (`ReturJualRusakCode`).
- Menambahkan file baru ke `btr.application.csproj`.
- Handler otomatis tercakup dalam MediatR assembly scanning di `Program.cs`.
- Menambahkan comprehensive unit tests pada `btr.test/InventoryContext/ListReturnOrderMonitoringQueryTest.cs` (menguji guard, date range, pemetaan BAGUS & RUSAK, dan eksklusi void). Seluruh pengujian (19/19) lulus dan solusi berhasil dikompilasi dengan 0 error.

Changed Files:
- `src/j05-btr-distrib/btr.application/InventoryContext/ReturnOrderAgg/ListReturnOrderDto.cs`
- `src/j05-btr-distrib/btr.application/InventoryContext/ReturnOrderAgg/ListReturnOrderMonitoringQuery.cs`
- `src/j05-btr-distrib/btr.application/InventoryContext/ReturnOrderAgg/Contracts/IReturnOrderDal.cs`
- `src/j05-btr-distrib/btr.application/btr.application.csproj`
- `src/j05-btr-distrib/btr.infrastructure/InventoryContext/ReturnOrderAgg/ReturnOrderDal.cs`
- `src/j05-btr-distrib/btr.test/InventoryContext/ListReturnOrderMonitoringQueryTest.cs`
- `src/j05-btr-distrib/btr.test/InventoryContext/ReturnOrderStatusWorkerTest.cs`
- `src/j05-btr-distrib/btr.test/btr.test.csproj`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

## P3 - WinForms Presentation Layer & Shell Integration

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P3-S06

Title: Interactive Guided Entry & Atomic Transactions in ReturJualForm

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Memperbarui `ReturJualForm.Designer.cs`:
  - Menambahkan kontrol `ReturnOrderLabel`, `ReturnOrderIdText`, dan `ReturnOrderButton` pada `panel3` dengan penataan visual rapi dan font konsisten (`Consolas`, 9pt).
- Membuat browser `ReturnOrderBrowser.cs` dan `ReturnOrderBrowserView` pada `btr.distrib/Browsers/` dan mendaftarkannya pada `btr.distrib.csproj` untuk mendukung penelusuran Order Retur secara interaktif via `ReturnOrderButton` dan tombol pintas `F1`.
- Mengimplementasikan `LoadReturnOrder(string returnOrderId)` pada `ReturJualForm.cs`:
  - Validasi keberadaan dokumen order retur dan penolakan jika status sudah `Imported`.
  - Pemuatan data referensi header: pengisian `ReturnOrderIdText`, penelusuran `CustomerIdText` beserta nama/alamat customer, pengisian `SalesIdText` dan `DriverIdText` jika tersedia.
  - Pengosongan `WarehouseIdText` dan `WarehouseNameText` untuk mewajibkan pemilihan gudang logikal secara eksplisit oleh pengguna, diikuti dialog pemberitahuan dan fokus otomatis ke kontrol gudang.
  - Penanganan pemilahan jenis retur:
    - Homogen: langsung mengunci `JenisReturCombo` ke jenis retur item yang ada.
    - Heterogen: mengecek dokumen Retur Jual aktif terkait via `_returJualDal.ListByReturnOrderId`. Jika salah satu jenis retur sudah pernah terbit, otomatis memilih jenis retur yang tersisa; jika keduanya belum terbit, memunculkan modal dialog interaktif `SelectJenisReturDialog` dengan pilihan tombol `[BAGUS]` dan `[RUSAK]`.
  - Memuat dan memvalidasi baris item ke dalam `_listItem` menggunakan `_createReturJualItemWorker.Execute` dan menghitung ulang total retur (`CalcTotal()`).
- Memperbarui `BuildReturJual()` untuk menyertakan `.ReturnOrderId(ReturnOrderIdText.Text)`.
- Mengamankan atomisitas transaksi:
  - Pada `SaveButton_Click`: penyimpanan retur jual (`_writer.Save`), pembuatan mutasi persediaan (`_genStokReturJualWorker.Execute`), dan pembaruan status order retur (`_returnOrderStatusWorker.UpdateStatus`) dibungkus dalam `using (var trans = TransHelper.NewScope())`.
  - Pada `DeleteReturJual`: penandaan `VoidDate`, pembatalan mutasi stok (`_removeRollbackStokWorker.Execute`), dan evaluasi pembaruan/revert status order retur (`_returnOrderStatusWorker.UpdateStatus`) dibungkus dalam `using (var trans = TransHelper.NewScope())`.
- Mengikat event `NewButton.Click` untuk memicu `ClearDisplay()` yang membersihkan `ReturnOrderIdText` beserta kontrol transaksi lainnya.
- Solusi berhasil dikompilasi menggunakan MSBuild dengan 0 error dan seluruh pengujian ReturnOrder (19/19) lulus.

Changed Files:
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.Designer.cs`
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturJualAgg/ReturJualForm.cs`
- `src/j05-btr-distrib/btr.distrib/Browsers/ReturnOrderBrowser.cs`
- `src/j05-btr-distrib/btr.distrib/btr.distrib.csproj`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

### P3-S07

Title: ListReturnOrderForm Monitoring & Navigation

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Form `ListReturnOrderForm.cs`, `ListReturnOrderForm.Designer.cs`, dan `ListReturnOrderForm.resx` dibuat pada `btr.distrib/InventoryContext/ReturnOrderAgg/` mengadopsi struktur arsitektural `ListOrderForm.cs`.
- Kontrol UI filter dan grid:
  - `PeriodeStartDatePicker` & `PeriodeEndDatePicker` dengan validasi rentang tanggal (guard <= 31 hari).
  - `SearchTextBox` (dengan alias `SearchText`) dan event handler Enter key serta tombol `SearchButton` dan `RefreshButton`.
  - `OrderGrid` terikat pada `SortableBindingList<ListReturnOrderDto>` melalui `BindingSource` dengan styling visual kolom Segoe UI, baris bernomor (`RowPostPaint`), dan `FullRowSelect`.
- Pewarnaan baris status otomatis pada event `CellFormatting`:
  - `Synced`: White.
  - `Partial`: LightYellow.
  - `Imported`: PaleGreen.
- Navigasi interaktif & Context Menu:
  - `DoubleClick` dan menu context "Realisasi Retur Jual" / "Buka Retur Jual Terkait":
    - Status `Synced` atau `Partial`: membuka `ReturJualForm` melalui shell MDI dan mengeksekusi `LoadReturnOrder(returnOrderId)`.
    - Status `Imported`: membaca dokumen Retur Jual aktif terkait via `_returJualDal.ListByReturnOrderId`. Jika terdapat 1 dokumen langsung membuka `ShowReturJual`, jika 2 dokumen (BAGUS & RUSAK) menyajikan dialog prompt pemilihan bagi pengguna.
- Ekspor Excel terintegrasi:
  - Tombol `ExcelButton` mengekspor seluruh data yang tampil pada grid ke format file `.xlsx` menggunakan library ClosedXML dengan header bergaya BTR standar.
- Form didaftarkan ke proyek `btr.distrib.csproj` (otomatis terdaftar ke DI container sebagai Transient Form via assembly scanning di `Program.cs`).
- Menambahkan test suite `ListReturnOrderFormTest.cs` (3 test case) pada `btr.test` memverifikasi instansiasi kontrol, pemuatan data query, dan resolusi DI. Seluruh test berhasil (22/22 passed).
- Proyek `btr.distrib` dan `btr.test` berhasil dikompilasi dengan 0 error menggunakan MSBuild.

Changed Files:
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/ListReturnOrderForm.cs`
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/ListReturnOrderForm.Designer.cs`
- `src/j05-btr-distrib/btr.distrib/InventoryContext/ReturnOrderAgg/ListReturnOrderForm.resx`
- `src/j05-btr-distrib/btr.distrib/btr.distrib.csproj`
- `src/j05-btr-distrib/btr.test/InventoryContext/ListReturnOrderFormTest.cs`
- `src/j05-btr-distrib/btr.test/btr.test.csproj`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

### P3-S08

Title: Shell Menu Integration & Legacy Component Cleanup

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Memperbarui `MainForm.cs`: event handler `RT3GenerateReturnOrder_Click` diarahkan untuk membuka `ListReturnOrderForm` (`BringMdiChildToFrontIfLoaded<ListReturnOrderForm>()` / `ThisServicesProvider.GetRequiredService<ListReturnOrderForm>()`).
- Memperbarui `MainForm.Designer.cs`: Text tombol menu `RT3GenerateReturnOrderMenu` diubah menjadi `"RT3-Monitoring Return Order"`.
- Memperbarui `BTR_Menu.dataseed.sql`: Deskripsi menu RT3 disinkronkan menjadi `'RT3-Monitoring Return Order'`.
- Membersihkan dan menghapus form lama dan DTO pendukungnya dari disk serta proyek `btr.distrib.csproj`:
  - `GenerateReturnOrderForm.cs`, `GenerateReturnOrderForm.Designer.cs`, `GenerateReturnOrderForm.resx`
  - `GenerateReturnOrderFormItemDto.cs`, `GenerateReturnOrderFormOrderDto.cs`, `GenerateReturnOrderFormResultDto.cs`
- Menghapus command lama `GenerateSalesReturnFromReturnOrderCommand.cs` dari disk dan referensi kompilasinya dari `btr.application.csproj`.
- Melakukan Rebuild bersih seluruh solusi `j05-btr-distrib.sln` dengan MSBuild (0 error).
- Seluruh 22 automated tests pada `btr.test` lulus dengan sukses (22/22 passed).

Changed Files:
- `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.cs`
- `src/j05-btr-distrib/btr.distrib/SharedForm/MainForm.Designer.cs`
- `src/j05-btr-distrib/btr.distrib/btr.distrib.csproj`
- `src/j05-btr-distrib/btr.application/btr.application.csproj`
- `src/j05-btr-distrib/btr.sql/DataSeeds/BTR_Menu.dataseed.sql`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

## P4 - Automated Testing & Verification

Implementation Status: NOT-STARTED  
Review Status: NOT-REVIEWED

### P4-S09

Title: Automated Unit & Integration Testing

Implementation Status: IMPLEMENTED  
Review Status: GO

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

Implementation Notes:
- Menyiapkan dan memperkaya unit test suite `ReturnOrderStatusWorkerTest.cs` (14 unit tests) mencakup:
  - Guard input ID null/kosong/whitespace dan kondisi order retur tidak ditemukan.
  - Transisi homogen: 1 Retur Jual disimpan -> status `Imported`.
  - Transisi heterogen: Retur Jual BAGUS disimpan -> status `Partial`; Retur Jual RUSAK disimpan -> status `Imported`.
  - Transisi revert saat void: 1 Retur Jual di-void (tersisa 1 aktif) -> status turun ke `Partial`; seluruh Retur Jual di-void -> status kembali ke `Synced`.
  - Sinkronisasi `SalesPersonId`, `SalesPersonName`, `DriverId`, `DriverName` dari Retur Jual ke order retur jika di order retur masih kosong, serta preservasi nilai awal jika sudah terisi (tidak di-overwrite).
  - Eksekusi via `INunaService<string, string>.Execute` dan toleransi *case insensitivity* jenis retur.
- Menyiapkan dan memperkaya unit test suite `ListReturnOrderMonitoringQueryTest.cs` (10 unit tests) mencakup:
  - Guard null request, format tanggal tidak valid, tanggal awal > tanggal akhir, dan batas rentang tanggal maksimal 31 hari.
  - Verifikasi batas tepat 31 hari (lulus) dan >31 hari (ditolak dengan pesan presisi).
  - Pemetaan nomor Retur Jual BAGUS dan RUSAK aktif serta eksklusi dokumen Retur Jual void.
  - Resolusi nama gudang otomatis dari `IWarehouseDal` dan preservasi nilai jika `WarehouseName` sudah terisi di header model.
- Menyiapkan integration test suite baru `ReturnOrderWorkflowIntegrationTest.cs` (2 test cases) yang memverifikasi siklus hidup utuh:
  - Siklus Order Retur Heterogen: Inisiasi order (`Synced`) -> realisasi BAGUS (`Partial`, monitoring memetakan nomor retur BAGUS) -> realisasi RUSAK (`Imported`, monitoring memetakan BAGUS dan RUSAK) -> void dokumen RUSAK (`Partial`, monitoring mengosongkan kode RUSAK) -> void dokumen BAGUS (`Synced`, monitoring mengosongkan seluruh nomor retur).
  - Siklus Order Retur Homogen: Inisiasi order (`Synced`) -> realisasi Retur Jual (`Imported`) -> void dokumen (`Synced`).
- Mendaftarkan file test baru ke `btr.test.csproj` dan mengompilasi solusi dengan 0 warning CS error.
- Seluruh 29 unit & integration tests pada `btr.test/InventoryContext` dieksekusi dengan hasil 100% lulus (29 passed, 0 failed, 0 skipped).

Changed Files:
- `src/j05-btr-distrib/btr.test/InventoryContext/ReturnOrderStatusWorkerTest.cs`
- `src/j05-btr-distrib/btr.test/InventoryContext/ListReturnOrderMonitoringQueryTest.cs`
- `src/j05-btr-distrib/btr.test/InventoryContext/ReturnOrderWorkflowIntegrationTest.cs`
- `src/j05-btr-distrib/btr.test/btr.test.csproj`
- `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-IMPLEMENTATION-PLAN.md`

---

# 6. Change Log

- 2026-11-09: Dokumen IMPLEMENTATION-PLAN awal dibentuk oleh `ica-architect` berdasarkan ARCHITECTURE `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-ARCHITECTURE.md` dan FEASIBILITY-ASSESSMENT `docs/issues/IS003-RETURN-ORDER-REALISASI-RETUR-JUAL-FEASIBILITY.md` (`READY-FOR-PLANNING`). Struktur rencana diresmikan dan diberi `Execution Approval: APPROVED`.
