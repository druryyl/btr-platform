---
Title: Warehouse Desktop Return Order & Barcode Management (.NET 4.7.1) — Implementation Plan
Code: BTR-RETUR-DESKTOP-001
Artifact: IMPLEMENTATION-PLAN
Version: 1.0
LastUpdated: 2026-10-11
Status: COMPLETED
Execution Approval: APPROVED
---

# 1. Objective

Implementation plan to realize the Warehouse Desktop Return Order Capture and Barcode Management capability (`BTR-RETUR-DESKTOP-001`), delivering a standalone .NET Framework 4.7.1 Windows Forms desktop client at `src/j09-btr-retur-gudang` that operates with a connected USB barcode scanner, local SQL Server (`btrgd`), and the existing Cloud API relay (`j06-pkl-btrade-api`).

Planning Mode: FEATURE-PLANNING

Referenced artifacts:

- FEATURE: `docs/features/return-order-desktop/feature.md` (`BTR-RETUR-DESKTOP-001`)
- ARCHITECTURE: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.1)
- FEASIBILITY-ASSESSMENT: `docs/work/j09-btr-retur-gudang/BTR-RETUR-GUDANG-DESKTOP-FEASIBILITY-ASSESSMENT.md` (Status: `READY-FOR-PLANNING`)

Architecture Applicability: ARCHITECTURE-REQUIRED

---

# 2. Planning Scope

The scope encompasses the complete technical realization of `BTR-RETUR-GUDANG-DESKTOP-ARCHITECTURE.md` (v1.1) across 4 execution phases:

1. **Foundation & Persistence:** Scaffold standalone layered solution `src/j09-btr-retur-gudang` (.NET Framework 4.7.1), SQL DDL script for warehouse tables in `btrgd`, core domain entities and value objects, and application contract interfaces / DTOs.
2. **Infrastructure & Integration:** Implement Dapper DALs for `btrgd`, RestSharp HTTP client for `j06-pkl-btrade-api` with `X-Session-*` headers, dual-trigger background synchronization engine, native audio beeper helper, workstation registry helper, and global USB scanner wedge filter (`IMessageFilter`).
3. **Presentation Layer (Approved Screen Inventory SCR-01 to SCR-07):**
   - Application shell `MainForm` (SCR-01) with docked sidebar navigation and status strip.
   - Core intake view `ReturnOrderEntryView` (SCR-02) with hands-free scanner focus banner, Quantity Strip (`Qty Bagus`/`Qty Rusak`), and double-buffered items grid.
   - Fallback manual item search modal `ManualItemSearchDialog` (SCR-03) on `F2`.
   - On-the-spot unknown barcode registration modal `QuickBarcodeRegistrationDialog` (SCR-04).
   - Order history and read-only inspector view `ReturnOrderListView` (SCR-05).
   - Barcode catalog management view `BarcodeRegistryView` (SCR-06).
   - Workstation settings, DB tester, shift operator switcher, and audio toggle view `SettingsView` (SCR-07).
4. **End-to-End Verification & Packaging:** Wire full application workflow, verify offline persistence, synchronization upload transitions (`Draft` -> `Synced`), post-sync document locking, and configure portable directory distribution output.

Excluded from scope:
- Physical receipt printing.
- Financial accounting, pricing, and invoice valuation (remains in Main Office BTR Desktop `ReturJual`).
- Modifications to existing endpoints in `src/j06-pkl-btrade-api`.
- Modifications to Packing Order codebase in `src/j07-btr-gudang`.

---

# 3. Dependencies

### External Dependencies
- Local SQL Server instance `(local)\btrgd` reachable on the warehouse PC.
- Running instance or network route to Cloud API relay `j06-pkl-btrade-api`.
- .NET Framework 4.7.1 Developer Pack / Build Tools on the workstation.
- USB HID barcode scanner emitting standard keystroke bursts terminating with CR (`\r`) or LF (`\n`).

### Slice Dependency Rules
- `Depends On` declares strict implementation prerequisites.
- Dependency satisfaction requires the referenced slice implementation status to be `IMPLEMENTED`.
- Dependency satisfaction does not require review status `GO`.

---

# 4. Progress Summary

| Phase | Description | Slices | Implementation Status | Review Status | Progress |
| :--- | :--- | :---: | :---: | :---: | :---: |
| **P1** | Foundation & Data Model | 3 | IMPLEMENTED | GO | 3/3 |
| **P2** | Infrastructure & Integration | 3 | IMPLEMENTED | GO | 3/3 |
| **P3** | Presentation Layer (Screen Inventory) | 4 | IMPLEMENTED | GO | 4/4 |
| **P4** | Verification & Packaging | 1 | IMPLEMENTED | GO | 1/1 |
| **Total** | | **11** | **IMPLEMENTED** | **GO** | **11/11** |

---

# 5. Phases

## P1 - Foundation & Data Model

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P1-S01: Solution Scaffolding & Database DDL
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Scaffold standalone solution `src/j09-btr-retur-gudang/BtrReturGudang.sln` targeting .NET Framework 4.7.1 with 5 project layers, and create SQL DDL script creating all warehouse tables, primary keys, and indexes in `btrgd`.
- **Depends On:** None
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Solution file `src/j09-btr-retur-gudang/BtrReturGudang.sln` created targeting .NET 4.7.1.
  - 5 project files created with proper inter-project references: `BtrReturGudang.Domain`, `BtrReturGudang.AppTier`, `BtrReturGudang.Infrastructure`, `BtrReturGudang.Winform`, `BtrReturGudang.SqlDb`.
  - SQL script `src/j09-btr-retur-gudang/BtrReturGudang.SqlDb/Scripts/001_CreateWarehouseTables.sql` written with DDL for:
    - `BTRG_ReturnOrder` (ULID PK, StatusSync default 'Draft', indexed on StatusSync and ReturnOrderDate)
    - `BTRG_ReturnOrderItem` (Composite PK `ReturnOrderId, NoUrut`, `JenisRetur` VARCHAR(5))
    - `BTRG_Barcode` (PK Barcode, `Pengali`, `IsActive`)
    - `BTRG_BarcodeRegistration` (ULID PK, `StatusSync` default 'Pending')
    - `BTRG_Barang`, `BTRG_Customer`, `BTRG_Driver`, `BTRG_SalesPerson`, `BTRG_SyncLog`
  - Zero database foreign keys declared (adhering to IR-RO-11).
  - Solution compiles cleanly via `msbuild` / `dotnet build`.
- **Implementation Notes:**
  - Created Visual Studio solution `src/j09-btr-retur-gudang/BtrReturGudang.sln` with SDK-style project references targeting `net471` using `Microsoft.NETFramework.ReferenceAssemblies`.
  - Configured project layers: `BtrReturGudang.Domain` (no upstream references), `BtrReturGudang.AppTier` (references `Domain`), `BtrReturGudang.Infrastructure` (references `AppTier` and `Domain`), `BtrReturGudang.Winform` (WinExe with `<UseWindowsForms>true</UseWindowsForms>`, references `AppTier`, `Infrastructure`, and `Domain`), and `BtrReturGudang.SqlDb` (content copying for scripts).
  - Created `001_CreateWarehouseTables.sql` creating all 9 warehouse tables with ULID PKs, appropriate default constraints, and performance indexes while declaring zero SQL foreign keys (per IR-RO-11).
  - Solution compiles with 0 warnings and 0 errors via `dotnet build`.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.sln`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/BtrReturGudang.AppTier.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Placeholder.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Placeholder.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Program.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.SqlDb/BtrReturGudang.SqlDb.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.SqlDb/Scripts/001_CreateWarehouseTables.sql`

---

### P1-S02: Domain Model & Value Objects
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement core domain entities, value objects, status enums, barcode normalization logic, and ULID generation in `BtrReturGudang.Domain`.
- **Depends On:** None
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Aggregate roots and entities implemented: `ReturnOrder`, `ReturnOrderItem`, `Barcode`, `Barang`, `Customer`, `Driver`, `SalesPerson`.
  - Enums declared: `JenisRetur` (`BAGUS`, `RUSAK`), `StatusSync` (`Draft`, `Synced`), `BarcodeRegistrationStatus` (`Pending`, `Uploaded`).
  - Value object `BarcodeNormalization` implemented with VO-01 rules: CR/LF/TAB stripped, whitespace trimmed, leading zeros preserved, lookup key uppercase.
  - ULID generator helper implemented producing standard 26-character identifiers (`Ulid.NewUlid().ToString()`).
  - Unit tests or domain validation methods confirming immutability checks on `Synced` documents.
- **Implementation Notes:**
  - Scaffolded `BtrReturGudang.Domain` targeting .NET Framework 4.7.1 (`net471`).
  - Implemented all aggregate roots and entities: `ReturnOrder`, `ReturnOrderItem`, `Barcode`, `Barang`, `Customer`, `Driver`, `SalesPerson`.
  - Implemented enums: `JenisRetur` (`BAGUS`, `RUSAK`), `StatusSync` (`Draft`, `Synced`), and `BarcodeRegistrationStatus` (`Pending`, `Uploaded`).
  - Implemented `BarcodeNormalization` value object strictly satisfying VO-01 (CR/LF/TAB stripped, whitespace trimmed, leading zeros preserved, uppercase lookup key).
  - Implemented `UlidGenerator` producing 26-character Crockford Base32 canonical monotonic ULIDs with validation support.
  - Implemented domain invariants and immutability checks (`DocumentLockedException`) in `ReturnOrder` protecting synced documents from mutation.
  - Implemented test suite in `BtrReturGudang.Domain.Tests` with 26 unit tests verifying immutability, normalization, and ULID generation; all tests passed.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/BtrReturGudang.Domain.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Enums/JenisRetur.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Enums/StatusSync.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Enums/BarcodeRegistrationStatus.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Exceptions/DomainExceptions.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/ValueObjects/BarcodeNormalization.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Common/UlidGenerator.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/ReturnOrder.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/ReturnOrderItem.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Barcode.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/BarcodeRegistration.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Barang.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Customer.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Driver.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/SalesPerson.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/BtrReturGudang.Domain.Tests.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/ReturnOrderImmutabilityTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/BarcodeNormalizationTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Domain.Tests/UlidGeneratorTests.cs`

---

### P1-S03: Application Contracts & DTOs
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Define repository interfaces, DTOs, and application orchestration service signatures in `BtrReturGudang.AppTier`.
- **Depends On:** P1-S02
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - Repository interfaces declared: `IReturnOrderRepo`, `IBarcodeRepo`, `IReferenceRepo`, `ISyncLogRepo`.
  - DTOs defined: `ReturnOrderDto`, `ReturnOrderItemDto`, `BarcodeRegistrationDto`, `MasterSyncDto`.
  - Service contracts declared:
    - `IReturnOrderService`: Create, add item, update line, commit draft, commit & upload.
    - `IBarcodeService`: Lookup barcode, quick register unmapped barcode.
    - `ISyncService`: Trigger document upload, query pending queue, pull reference deltas.
  - Project references `BtrReturGudang.Domain` and compiles with 0 errors.
- **Implementation Notes:**
  - Declared repository interfaces under `BtrReturGudang.AppTier.Repositories`: `IReturnOrderRepo`, `IBarcodeRepo`, `IReferenceRepo`, `ISyncLogRepo`.
  - Declared DTOs under `BtrReturGudang.AppTier.DTOs`: `ReturnOrderDto`, `ReturnOrderItemDto`, `BarcodeRegistrationDto`, `MasterSyncDto`, `SyncResultDto`, `SyncLogEntry`, `BarcodeLookupResult`, and master reference DTOs (`BarangDto`, `BarcodeDto`, `CustomerDto`, `DriverDto`, `SalesPersonDto`).
  - Declared service contracts under `BtrReturGudang.AppTier.Services`: `IReturnOrderService`, `IBarcodeService`, `ISyncService`, and cloud API contract `IBtradeApiClient` under `BtrReturGudang.AppTier.Contracts`.
  - Implemented application orchestration services:
    - `ReturnOrderService`: Orchestrates document creation, item addition, line modification, draft commitment, and atomic commit + cloud upload.
    - `BarcodeService`: Orchestrates barcode normalization, local lookup, item code fallback, and on-the-spot registration queuing with immediate local cache insertion.
  - Implemented test suite in `BtrReturGudang.AppTier.Tests` with 11 unit tests covering DTO mapping, calculations, and service workflows; all tests passed cleanly with 0 errors.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/ReturnOrderDto.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/ReturnOrderItemDto.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/BarcodeRegistrationDto.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/MasterSyncDto.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/ReferenceDtos.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/SyncResultDto.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/SyncLogEntry.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/BarcodeLookupResult.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Repositories/IReturnOrderRepo.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Repositories/IBarcodeRepo.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Repositories/IReferenceRepo.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Repositories/ISyncLogRepo.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Contracts/IBtradeApiClient.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Services/IReturnOrderService.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Services/IBarcodeService.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Services/ISyncService.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Services/ReturnOrderService.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Services/BarcodeService.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier.Tests/BtrReturGudang.AppTier.Tests.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier.Tests/ReturnOrderDtoTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier.Tests/BarcodeRegistrationDtoTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier.Tests/MasterSyncDtoTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier.Tests/ReturnOrderServiceTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier.Tests/BarcodeServiceTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.sln`

---

## P2 - Infrastructure & Integration

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P2-S04: Local Persistence DAL with Dapper
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement Dapper data access repositories in `BtrReturGudang.Infrastructure` persisting records in local SQL Server `(local)\btrgd` using parameterized queries and short-lived connections.
- **Depends On:** P1-S01, P1-S03
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `ConnStringHelper` implemented supporting connection string resolution (`Server=(local);Database=btrgd;User Id=btrGudangLogin;Password=btrGudang123!;Max Pool Size=20;`).
  - `ReturnOrderDal` implemented implementing `IReturnOrderRepo` (Insert/Update header, Insert item lines within transaction scope, GetById, GetFilteredList, MarkSynced).
  - `BarcodeDal` implemented implementing `IBarcodeRepo` (LookupByBarcode, InsertBarcode, QueueRegistration, GetPendingRegistrations).
  - `MasterDataDal` implemented implementing `IReferenceRepo` (BulkUpsertBarang, BulkUpsertCustomer, SearchBarangByNameOrCode).
  - Queries execute via Dapper with explicit transaction management and short-lived connection scopes.
- **Implementation Notes:**
  - Added Dapper 2.1.66 package reference to `BtrReturGudang.Infrastructure.csproj`.
  - Implemented `ConnStringHelper` in `BtrReturGudang.Infrastructure.Helpers` supporting multi-tier connection string resolution: in-memory override -> Windows Registry (`HKCU`/`HKLM\Software\BTR\Warehouse\DbConnectionString`) -> `App.config` (`ConnectionStrings["btrgd"]` / `AppSettings["BtrgdConnString"]`) -> standard fallback `Server=(local);Database=btrgd;User Id=btrGudangLogin;Password=btrGudang123!;Max Pool Size=20;`. Added synchronous and asynchronous connection factory methods (`CreateConnection`, `OpenConnection`, `OpenConnectionAsync`).
  - Implemented `ReturnOrderDal` in `BtrReturGudang.Infrastructure.Persistence` implementing `IReturnOrderRepo`:
    - `GetById` / `GetByIdAsync`: multi-result query hydrating header and item collections into domain `ReturnOrder`.
    - `GetFilteredList` / `GetFilteredListAsync`: date and status filtered query with batched item retrieval (`IN @OrderIds`) avoiding N+1 queries.
    - `GetPendingUploadList` / `GetPendingUploadListAsync`: draft query with complete item collections for upload engine.
    - `Save` / `SaveAsync`: atomic transactional upsert of header and replacement of item lines with explicit status locking check (`DocumentLockedException`).
    - `MarkSynced` / `MarkSyncedAsync`: transitions status to `Synced` and records `SyncedDate`.
    - `DeleteDraft` / `DeleteDraftAsync`: transactional deletion of items and header, guarding synced documents against deletion.
  - Implemented `BarcodeDal` in `BtrReturGudang.Infrastructure.Persistence` implementing `IBarcodeRepo`:
    - `LookupByBarcode` / `LookupByBarcodeAsync`: normalized barcode query against active cache in `BTRG_Barcode`.
    - `InsertBarcode` / `InsertBarcodeAsync`: single barcode upsert.
    - `BulkUpsertBarcodes` / `BulkUpsertBarcodesAsync`: transactional batched upsert.
    - `GetAllBarcodes` / `SearchBarcodes`: catalog queries by code/item name.
    - `QueueRegistration` / `QueueRegistrationAsync`: queues on-the-spot registration requests into `BTRG_BarcodeRegistration`.
    - `GetPendingRegistrations` / `GetPendingRegistrationsAsync`: retrieves un-synced registrations.
    - `MarkRegistrationUploaded` / `MarkRegistrationUploadedAsync`: updates registration status to `Uploaded`.
  - Implemented `MasterDataDal` in `BtrReturGudang.Infrastructure.Persistence` implementing `IReferenceRepo`:
    - Barang: `BulkUpsertBarang`, `SearchBarangByNameOrCode`, `GetBarangById`.
    - Customer: `BulkUpsertCustomer`, `SearchCustomer`, `GetCustomerById`.
    - Driver: `BulkUpsertDriver`, `GetAllDrivers`.
    - SalesPerson: `BulkUpsertSalesPerson`, `GetAllSalesPersons`.
  - Implemented `SyncLogDal` in `BtrReturGudang.Infrastructure.Persistence` implementing `ISyncLogRepo`:
    - `LogSync` / `LogSyncAsync`: inserts execution log entries into `BTRG_SyncLog`.
    - `GetRecentLogs` / `GetRecentLogsAsync`: fetches top N execution logs.
    - `GetLastSuccessfulSyncTimestamp` / `GetLastSuccessfulSyncTimestampAsync`: scalar query for last successful sync run.
  - Created test suite `BtrReturGudang.Infrastructure.Tests` with 25 unit tests covering `ConnStringHelper` resolution/overrides and DAL interface validation rules; 100% tests passed.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Helpers/ConnStringHelper.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Persistence/ReturnOrderDal.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Persistence/BarcodeDal.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Persistence/MasterDataDal.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Persistence/SyncLogDal.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure.Tests/BtrReturGudang.Infrastructure.Tests.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure.Tests/ConnStringHelperTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure.Tests/DalValidationTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.sln`

---

### P2-S05: Cloud API Relay Client & Synchronization Engine
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement RestSharp API client targeting `j06-pkl-btrade-api` with session headers, and background synchronization engine managing dual-trigger uploads and master reference synchronization.
- **Depends On:** P1-S03, P2-S04
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `BtradeApiClient` implemented using RestSharp:
    - Injects `X-Session-Location` (`WarehouseCode`) and `X-Session-Actor` (`OperatorName`) on all requests.
    - `UploadReturnOrderAsync(ReturnOrderDto dto)` -> `POST api/return-order`.
    - `UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto)` -> `POST api/barcode-registration`.
    - `DownloadMasterDataAsync(string serverId)` -> `GET api/barcodes/sync`, `GET api/Customer/{serverId}`, `GET api/Brg/{serverId}`.
  - `SyncService` implemented implementing `ISyncService` with re-entrancy lock (`Interlocked.CompareExchange`):
    - Triggers upload of specific Return Order upon save.
    - Scans for un-synced Return Orders and pending Barcode Registrations.
    - Updates local database status from `Draft` to `Synced` and writes to `BTRG_SyncLog`.
- **Implementation Notes:**
  - Added RestSharp 112.1.0 package reference to `BtrReturGudang.Infrastructure.csproj` targeting `net471`.
  - Implemented `BtradeApiClient` in `BtrReturGudang.Infrastructure.Http` implementing `IBtradeApiClient`:
    - Attaches `X-Session-Location` and `X-Session-Actor` session headers on all outbound requests, dynamically resolving active workstation warehouse code and shift operator name from `IRegistryHelper` or dynamic providers.
    - `UploadReturnOrderAsync`: serializes order payload matching Cloud API `ReturnOrderSubmitRequest` (`ReturnOrderId`, `ReturnOrderDate`, `WarehouseCode`, `CustomerId`, `CustomerName`, `SalesPersonId`, `SalesPersonName`, `DriverId`, `DriverName`, `Note`, `ListItem`) and executes `POST api/return-order`.
    - `UploadBarcodeRegistrationAsync`: serializes barcode linking request matching Cloud API `BarcodeRegistrationSubmitRequest` (`ClientRequestId`, `BarcodeValue`, `BrgId`, `Satuan`) and executes `POST api/barcode-registration`.
    - `DownloadMasterDataAsync`: queries `GET api/barcodes/sync`, `GET api/Customer/{serverId}`, `GET api/Brg/{serverId}`, `GET api/Driver/{serverId}`, and `GET api/SalesPerson/{serverId}` parsing JSend envelope structures into `MasterSyncDto`.
  - Implemented `SyncService` in `BtrReturGudang.Infrastructure.Services` implementing `ISyncService`:
    - Thread-safe re-entrancy lock using `Interlocked.CompareExchange(ref _syncLock, 1, 0)` preventing overlapping sync passes while exposing live `IsSyncInProgress`.
    - `TriggerDocumentUploadAsync`: fetches order by ID, uploads via `BtradeApiClient`, updates status to `Synced` with timestamp via `IReturnOrderRepo.MarkSyncedAsync`, and records audit entry in `BTRG_SyncLog`.
    - `QueryPendingQueueAsync` & `QueryPendingRegistrationQueueAsync`: retrieves un-synced orders and pending registrations for queue inspection.
    - `PullReferenceDeltasAsync`: downloads master reference deltas and performs transactional bulk upserts into `BTRG_Barang`, `BTRG_Barcode`, `BTRG_Customer`, `BTRG_Driver`, and `BTRG_SalesPerson`.
    - `RunFullSyncPassAsync`: orchestrates push of pending barcode registrations, push of pending return orders, and pull of master reference deltas, guarded by re-entrancy locking.
  - Implemented test suites `BtradeApiClientTests` and `SyncServiceTests` in `BtrReturGudang.Infrastructure.Tests` covering header injection, operator change reflection, payload generation, JSend parsing, single-document upload transitions, master delta upserts, and re-entrancy concurrency protection. All 86 unit tests across the solution pass cleanly with 0 errors.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Http/BtradeApiClient.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Services/SyncService.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure.Tests/BtradeApiClientTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure.Tests/SyncServiceTests.cs`

---

### P2-S06: Hardware & OS Helpers
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement native audio cues (`AudioHelper`), workstation registry settings (`RegistryHelper`), and global keyboard-wedge scanner listener (`ScannerWedgeListener`).
- **Depends On:** P1-S03
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `AudioHelper` implemented:
    - `PlaySuccess()` invokes `Console.Beep(2000, 80)` via `Task.Run`.
    - `PlayWarning()` invokes `Console.Beep(400, 200)` via `Task.Run`.
    - Toggled via `EnableSoundFeedback` boolean property.
  - `RegistryHelper` implemented reading and persisting workstation configuration (`WarehouseCode`, `ServerTargetId`, `LastOperatorName`, `EnableSoundFeedback`) under `HKCU\Software\BTRApp\ReturGudang`.
  - `ScannerWedgeListener` implemented implementing `IMessageFilter`:
    - Intercepts `WM_KEYDOWN` (0x0100) / `WM_CHAR` (0x0102).
    - Buffers keystrokes arriving within <= 50ms intervals.
    - On `VK_RETURN (0x0D)`, normalizes buffered characters, fires `BarcodeScanned` event, and suppresses keys from active child control (`return true`).
    - Flushes buffer if interval exceeds 50ms (human typing pass-through).
- **Implementation Notes:**
  - Declared `IAudioHelper` and `IRegistryHelper` contracts under `BtrReturGudang.AppTier.Contracts`.
  - Implemented `AudioHelper` in `BtrReturGudang.Infrastructure.Helpers` providing non-blocking asynchronous audio cues (`Console.Beep(2000, 80)` for success, `Console.Beep(400, 200)` for warning) via `Task.Run` with `EnableSoundFeedback` toggle.
  - Implemented `RegistryHelper` in `BtrReturGudang.Infrastructure.Helpers` reading and persisting workstation configuration (`WarehouseCode`, `ServerTargetId`, `LastOperatorName`, `EnableSoundFeedback`) under `HKCU\Software\BTRApp\ReturGudang` with bidirectional audio toggle synchronization.
  - Implemented `ScannerWedgeListener` in `BtrReturGudang.Winform.Hardware` implementing `System.Windows.Forms.IMessageFilter`:
    - Buffers rapid keystrokes arriving within <= 50ms inter-character interval.
    - Suppresses `VK_RETURN (0x0D)` on complete burst, normalizes barcode per VO-01 rules (`BarcodeNormalization`), fires `BarcodeScanned` event, and suppresses child control dispatch (`return true`).
    - Flushes buffer on slow human typing (> 50ms interval) or navigation/function keys, allowing normal pass-through (`return false`).
  - Added test suite in `BtrReturGudang.Winform.Tests` containing 12 unit tests verifying audio toggling, registry persistence, and message filter burst interception; all 12 tests passed cleanly (total solution tests: 49 passing, 0 errors).
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Contracts/IAudioHelper.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/Contracts/IRegistryHelper.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Helpers/AudioHelper.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/Helpers/RegistryHelper.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Infrastructure/BtrReturGudang.Infrastructure.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Hardware/BarcodeScannedEventArgs.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Hardware/ScannerWedgeListener.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/BtrReturGudang.Winform.Tests.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/AudioHelperTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/RegistryHelperTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ScannerWedgeListenerTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.sln`

---

## P3 - Presentation Layer (Screen Inventory)

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P3-S07: Shell Window & Navigation Infrastructure (SCR-01)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement application shell `MainForm` (SCR-01) with docked sidebar navigation, workstation status header, operator switcher, global scanner filter registration, and 5-minute background sync timer.
- **Depends On:** P2-S05, P2-S06
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `Program.cs` configured with `Microsoft.Extensions.DependencyInjection` wiring services, repositories, and forms.
  - `MainForm` (SCR-01) created:
    - Docked Sidebar Navigation with module buttons: Intake Retur (`F1`), Daftar Retur (`F3`), Barcode Registry (`F4`), Pengaturan (`F5`).
    - Dynamic view hosting container (`pnlContentHost`, `Dock = DockStyle.Fill`).
    - Top header displaying Depo code (`WarehouseCode`), active Operator, and live online/offline indicator dot.
    - Bottom status strip displaying scanner hook connection status, buffer latency, and last sync timestamp.
    - `System.Windows.Forms.Timer` running every 5 minutes executing background sync pass without blocking UI.
    - Sidebar "Sync Sekarang" button triggering manual sync with animated indicator.
    - Registers `ScannerWedgeListener` into `Application.AddMessageFilter`.
- **Implementation Notes:**
  - Added `Microsoft.Extensions.DependencyInjection` (version 10.0.12) to `BtrReturGudang.Winform.csproj`.
  - Configured DI container in `Program.cs` via `ConfigureServices`, registering DAL repositories (`ReturnOrderDal`, `BarcodeDal`, `MasterDataDal`, `SyncLogDal`), helpers (`AudioHelper`, `RegistryHelper`), scanner filter (`ScannerWedgeListener`), services (`BtradeApiClient`, `SyncService`, `ReturnOrderService`, `BarcodeService`), and forms/views.
  - Implemented application shell `MainForm` (SCR-01) in `BtrReturGudang.Winform.Views`:
    - Docked sidebar navigation (`pnlSidebar`, Width = 220, Slate-800) with module buttons: Intake Retur (`F1`), Daftar Retur (`F3`), Barcode Registry (`F4`), Pengaturan (`F5`), with active button visual highlighting.
    - Dynamic view container `pnlContentHost` (`Dock = DockStyle.Fill`) with double-buffering.
    - Top header (`pnlHeader`, Height = 60, Slate-900) displaying Depo code badge, active Operator badge with "Ganti" switcher button, and live Online/Offline indicator dot.
    - Implemented `OperatorSwitcherDialog` modal allowing operator to change active shift operator with immediate registry persistence and header updates.
    - Bottom status strip (`StatusStrip`) displaying scanner wedge status, buffer latency (`<= 50ms`), last sync timestamp, and operational readiness info.
    - 5-minute background sync timer (`System.Windows.Forms.Timer`, interval = 300,000 ms) running non-blocking synchronization pass.
    - Sidebar "Sync Sekarang" button triggering manual sync pass with animated spinner (`_syncAnimationTimer`) and completion state reflection.
    - Registered `ScannerWedgeListener` into `Application.AddMessageFilter` with safe removal on form closure; dispatches scans to active views implementing `IScannerConsumer`.
    - Global keyboard shortcuts (`F1`, `F3`, `F4`, `F5`) handled via `ProcessCmdKey` switching active views in `pnlContentHost`.
    - Provided stub `UserControl` views (`ReturnOrderEntryView`, `ReturnOrderListView`, `BarcodeRegistryView`, `SettingsView`) ensuring seamless navigation without crashes.
  - Implemented unit and integration test suite `MainFormTests` in `BtrReturGudang.Winform.Tests` covering DI resolution, form startup, multi-view navigation, online indicator toggling, and scan event consumption; all 17 tests passed cleanly (total solution tests: 91 passed, 0 failed, 0 errors, 0 warnings).
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Program.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/IScannerConsumer.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/MainForm.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderListView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/BarcodeRegistryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/SettingsView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/OperatorSwitcherDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/MainFormTests.cs`

---

### P3-S08: Core Return Order Intake & Quantity Strip (SCR-02)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement `ReturnOrderEntryView` (SCR-02) featuring customer autocomplete, scanner intake focus banner, ergonomic Quantity Strip (`Qty Bagus`/`Qty Rusak`), double-buffered items DataGridView, and action toolbar.
- **Depends On:** P2-S04, P2-S06, P3-S07
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `ReturnOrderEntryView` UserControl created inheriting `Dock = DockStyle.Fill`.
  - Header area: Customer search box with autocomplete against `BTRG_Customer`, optional Driver & Salesman dropdowns, notes textbox.
  - Active Scanner Banner: Displays scanned barcode, resolved product name, item code, packaging unit multiplier.
  - Ergonomic Quantity Strip:
    - Packaging unit dropdown (`cboSatuan`).
    - `numQtyBagus` (emerald accent) and `numQtyRusak` (rose accent) NumericUpDown controls.
    - Auto-focuses `numQtyBagus` immediately upon successful barcode scan.
    - Pressing `Enter` in quantity fields adds row(s) to grid, triggers `AudioHelper.PlaySuccess()`, and resets focus for the next scan.
  - Double-buffered `DataGridView` displaying line items with visual badges for `BAGUS` vs `RUSAK`, line numbering, and delete action.
  - Grid summary footer computing total line items, sum `Qty Bagus`, and sum `Qty Rusak`.
  - Action toolbar: `F9` Simpan Draft, `F10` Simpan & Upload, `Esc` Reset Form.
- **Implementation Notes:**
  - Implemented `DoubleBufferedDataGridView` in `BtrReturGudang.Winform.Controls` enabling hardware double-buffering to eliminate rendering flicker during fast data entry.
  - Implemented `ReturnOrderEntryView` (SCR-02) in `BtrReturGudang.Winform.Views`:
    - Header area: customer combobox with autocomplete against `BTRG_Customer` cache, optional Driver & Salesman comboboxes populated from `IReferenceRepo`, and notes textbox.
    - Active Scanner Banner: displays barcode, resolved product name, item code, and packaging unit multiplier with status reflection.
    - Ergonomic Quantity Strip: packaging unit selector (`cboSatuan`), `numQtyBagus` (emerald accent) and `numQtyRusak` (rose accent), auto-focusing `numQtyBagus` on successful scan.
    - Commits line(s) to grid on `Enter`, triggers non-blocking success audio cue via `IAudioHelper.PlaySuccess()`, and resets focus for subsequent scans.
    - Double-buffered items DataGridView with visual badges for `BAGUS` (emerald) vs `RUSAK` (rose), line numbering (`NoUrut`), and delete action.
    - Grid summary footer dynamically computing total items count, sum `Qty Bagus`, and sum `Qty Rusak`.
    - Action toolbar handling `F9` Simpan Draft, `F10` Simpan & Upload, and `Esc` Reset Form, integrated with `ReturnOrderService`, `BarcodeService`, `IReferenceRepo`, and `IAudioHelper`.
  - Added test suite `ReturnOrderEntryViewTests` in `BtrReturGudang.Winform.Tests` covering initialization, recognized and unmapped barcode scans, quantity strip commits (Bagus, Rusak, and dual-row), delete/re-sequencing, reset, and draft loading; all 27 WinForms tests and 101 solution tests pass with 0 errors.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Controls/DoubleBufferedDataGridView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ReturnOrderEntryViewTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.AppTier/DTOs/ReturnOrderItemDto.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/BarcodeRegistrationDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderListView.cs`

---

### P3-S09: Modal Dialogs for Item Search & Barcode Registration (SCR-03 & SCR-04)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement keyboard-driven manual item search modal `ManualItemSearchDialog` (SCR-03 / `F2`) and on-the-spot unknown barcode registration modal `QuickBarcodeRegistrationDialog` (SCR-04).
- **Depends On:** P2-S04, P2-S06, P3-S08
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `ManualItemSearchDialog` (SCR-03) created:
    - Triggered by pressing `F2` from `ReturnOrderEntryView` or clicking search button.
    - Auto-focused search textbox filtering `BTRG_Barang` by code or name.
    - Fast `DataGridView` navigable via Up/Down arrow keys.
    - `Enter` selects active item, passes it back to `ReturnOrderEntryView.SetSelectedItem()`, and closes modal.
    - `Esc` closes modal without selection.
  - `QuickBarcodeRegistrationDialog` (SCR-04) created:
    - Triggered automatically when `ScannerWedgeListener` emits a barcode not found in `BTRG_Barcode`.
    - Triggers `AudioHelper.PlayWarning()` (400Hz buzz).
    - Displays scanned barcode in prominent read-only display.
    - Product picker to link to existing `BrgId` in `BTRG_Barang`.
    - Unit selector (`Dus`, `Pak`, `Pcs`) and multiplier input (`Pengali`).
    - On confirmation: Saves mapping into local `BTRG_Barcode`, queues record into `BTRG_BarcodeRegistration`, immediately adds the item to the active intake in `ReturnOrderEntryView`, and closes dialog.
- **Implementation Notes:**
  - Implemented `ManualItemSearchDialog` (SCR-03) in `BtrReturGudang.Winform.Views`:
    - Auto-focused search textbox `_txtSearch` filtering master product records from `BTRG_Barang` via `IReferenceRepo.SearchBarangByNameOrCodeAsync`.
    - Double-buffered items `DataGridView` navigable via Up/Down arrow keys directly from search box without requiring explicit mouse clicks.
    - Unit selector dropdown `_cboSatuan` populated dynamically with item packaging units (`Satuan1`, `Satuan2`, `Satuan3`).
    - `Enter` commits item selection and unit, sets `DialogResult = DialogResult.OK`, triggers success sound cue, and passes item back to `ReturnOrderEntryView.SetSelectedItem()`.
    - `Esc` or Cancel closes dialog with `DialogResult = DialogResult.Cancel` without modifying intake state.
  - Implemented `QuickBarcodeRegistrationDialog` (SCR-04) in `BtrReturGudang.Winform.Views`:
    - Automatically triggered when unknown barcode is scanned in `ReturnOrderEntryView`.
    - Emits non-blocking 400Hz audio cue (`IAudioHelper.PlayWarning()`) on show.
    - Prominently displays scanned barcode in stylized read-only display box (`Consolas 12pt Bold` with Rose-50 accent).
    - Product picker filtering master `BTRG_Barang` records by name or code.
    - Packaging unit dropdown (`Dus`, `Pak`, `Pcs`, `Ball`, `Lusin`) and multiplier input `_numPengali` (`NumericUpDown`).
    - On confirmation: persists mapping into local `BTRG_Barcode`, queues registration request into `BTRG_BarcodeRegistration` via `IBarcodeService.QuickRegisterUnmappedBarcodeAsync`, plays success beep, and immediately applies `RegisteredResult` to the active intake banner and quantity strip in `ReturnOrderEntryView`.
  - Wired both dialogs into `ReturnOrderEntryView`:
    - Added `SetSelectedItem(Barang item, string selectedSatuan = null)` to activate product intake and focus quantity strip.
    - Bound `TriggerManualItemSearch()` to launch `ManualItemSearchDialog` on `F2` key or search button click.
    - Updated `OnBarcodeScanned` to launch `QuickBarcodeRegistrationDialog` automatically on unmapped barcode miss and immediate intake population upon registration.
    - Added testing launchers `ManualSearchDialogLauncher` and `QuickRegistrationDialogLauncher` for deterministic test automation.
    - Registered dialogs in `Program.ConfigureServices` DI container.
  - Implemented unit and integration test suite `ModalDialogTests` in `BtrReturGudang.Winform.Tests` covering dialog lifecycle, keyboard navigation, filtering, registration commits, unit selection, and `ReturnOrderEntryView` integration; all 11 modal tests and all 98 solution tests pass cleanly with 0 errors and 0 warnings.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Program.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ManualItemSearchDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/QuickBarcodeRegistrationDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ModalDialogTests.cs`

---

### P3-S10: History Inspector, Barcode Registry & Settings Views (SCR-05, SCR-06, SCR-07)
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Implement `ReturnOrderListView` (SCR-05) with status filtering and post-sync read-only document locking; `BarcodeRegistryView` (SCR-06) for catalog lookup; and `SettingsView` (SCR-07) for station configuration and hardware testing.
- **Depends On:** P2-S04, P2-S05, P2-S06, P3-S07
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - `ReturnOrderListView` (SCR-05) created:
    - Date range picker and Status Sync filter (`All`, `Draft`, `Synced`).
    - Master grid displaying Return Orders with colored badges (`Draft` in amber, `Synced` in emerald).
    - Slide-over / split inspector panel showing header metadata and line item details.
    - Post-Sync Immutability: If selected order has `StatusSync = 'Synced'`, Edit and Delete buttons are disabled, and a prominent "Read-Only / Dokumen Terkunci" alert banner is displayed.
    - Draft orders allow re-opening or re-uploading via "Upload Ulang".
  - `BarcodeRegistryView` (SCR-06) created:
    - Search input filtering local barcode catalog by barcode, code, or name.
    - Grid displaying Barcode, BrgId, BrgName, Satuan, Pengali, Status (`Aktif Pusat` vs `Pending Upload`).
    - "Tambah Barcode Baru" button opening registration form.
  - `SettingsView` (SCR-07) created:
    - Workstation identity panel: `WarehouseCode`, PC Name, active `OperatorName` with "Ganti Operator" switcher.
    - Local DB connection string input with "Tes Koneksi Local DB" button.
    - Cloud API relay URL configuration with ping/test button.
    - Audio Feedback toggle (`EnableSoundFeedback`) with test buttons for 2000Hz (Success) and 400Hz (Warning) beeps.
- **Implementation Notes:**
  - Implemented `ReturnOrderListView` (SCR-05) in `BtrReturGudang.Winform.Views`:
    - Date range pickers (`dtpStartDate`, `dtpEndDate`) and sync status dropdown filter (`All`, `Draft`, `Synced`).
    - Double-buffered master grid displaying Return Orders with visual status badges: `Draft` in amber (`#FEF3C7`/`#B45309`), `Synced` in emerald (`#D1FAE5`/`#047857`).
    - Slide-over inspector panel detailing document metadata (ULID, date, customer, driver, sales, depo, operator, sync timestamp) and double-buffered line items grid with good/damaged classification badges.
    - Post-Sync Immutability enforcement: when a `Synced` document is selected, displays prominent red read-only alert banner (`pnlLockedBanner`), disabling edit and delete action buttons. For `Draft` documents, enables editing and re-uploading via "Upload Ulang".
  - Implemented `BarcodeRegistryView` (SCR-06) in `BtrReturGudang.Winform.Views`:
    - Keyword search input filtering local barcode catalog by barcode value, item code, or item name.
    - Status filtering supporting all barcodes, `Aktif Pusat`, or `Pending Upload`.
    - Double-buffered DataGridView displaying Barcode, BrgId, BrgName, Satuan, Pengali, and colored status badges.
    - Integrated with `BarcodeRegistrationDialog` modal opened via "Tambah Barcode Baru" button for on-the-spot and manual packaging barcode registrations.
  - Implemented `SettingsView` (SCR-07) in `BtrReturGudang.Winform.Views`:
    - Workstation identity panel displaying and configuring `WarehouseCode`, `ServerTargetId`, machine name, and active operator with `OperatorSwitcherDialog` trigger.
    - Local database connection tester with live test connection button, default reset, and status badge.
    - Cloud API relay configuration with ping route verification.
    - Sound feedback toggle (`EnableSoundFeedback`) synchronized with `IRegistryHelper` and `IAudioHelper`, with interactive test buttons for 2000Hz (Success) and 400Hz (Warning) tones.
  - Added test suites in `BtrReturGudang.Winform.Tests` (`ReturnOrderListViewTests`, `BarcodeRegistryViewTests`, `SettingsViewTests`) covering immutability locking, catalog search/filter, and workstation configuration persistence; all 39 tests in `BtrReturGudang.Winform.Tests` and 113 tests solution-wide pass cleanly.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderListView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/BarcodeRegistryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/BarcodeRegistrationDialog.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/SettingsView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/MainFormTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/ReturnOrderListViewTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/BarcodeRegistryViewTests.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/SettingsViewTests.cs`

---

## P4 - Verification & Packaging

Implementation Status: IMPLEMENTED  
Review Status: GO  

### P4-S11: End-to-End Workflow Integration & Portable Packaging
- **Implementation Status:** IMPLEMENTED
- **Review Status:** GO
- **Objective:** Perform end-to-end integration verification across all 7 screens, validate full intake flow with USB scanner simulation, verify offline persistence and cloud sync transitions, and configure portable folder distribution output.
- **Depends On:** P3-S08, P3-S09, P3-S10
- **Repository:** `d:\Project.Private\btr-platform`
- **Completion Criteria:**
  - End-to-end integration verified:
    1. Scan valid barcode -> Quantity Strip auto-focus -> enter Qty Bagus/Rusak -> `Enter` commits line with 2000Hz beep.
    2. Scan invalid barcode -> 400Hz buzz -> `QuickBarcodeRegistrationDialog` maps item -> added to return order.
    3. Press `F2` -> `ManualItemSearchDialog` filters master product -> selected into quantity strip.
    4. Press `F10` ("Simpan & Upload") -> document saved to `(local)\btrgd` as `Draft`, HTTP upload to `j06-pkl-btrade-api` transitions document to `Synced`.
    5. In `ReturnOrderListView`, document displays `Synced` badge and editing is permanently locked.
    6. Disconnecting network still allows saving locally as `Draft`; 5-minute background timer pushes document once connectivity resumes.
  - Build configuration generates self-contained portable distribution folder (`bin\Release\net471` or xcopy-ready package) containing all required DLLs (Dapper, RestSharp, etc.) without installer wizards.
  - Solution builds with 0 errors and 0 warnings.
- **Implementation Notes:**
  - Created comprehensive End-to-End integration test suite `EndToEndWorkflowIntegrationTests` in `BtrReturGudang.Winform.Tests` covering all 6 key operational scenarios and all 7 UI screens (SCR-01 through SCR-07):
    1. Valid barcode scan populates active scanner banner, auto-focuses quantity strip, commits Bagus/Rusak lines on `Enter` with 2000Hz beep, and updates double-buffered grid without flicker.
    2. Unrecognized barcode scan emits 400Hz warning buzzer, prompts `QuickBarcodeRegistrationDialog` (SCR-04) for on-the-spot mapping, adds mapped item immediately to return order, and queues upload request.
    3. `F2` keyboard shortcut triggers `ManualItemSearchDialog` (SCR-03) filtering cached products and populates selected product into intake quantity strip.
    4. `F10` ("Simpan & Upload") persists return order to local storage as `Draft`, triggers HTTP upload to cloud staging relay, and transitions document status to `Synced` with recorded timestamp.
    5. `ReturnOrderListView` (SCR-05) displays `Synced` badge and enforces post-sync immutability, permanently disabling Edit and Delete actions and showing the read-only locked alert banner.
    6. Network disconnection preserves local persistence as `Draft`; simulating network restoration and 5-minute periodic background timer (`ISyncService.RunFullSyncPassAsync`) successfully pushes pending orders to cloud and transitions them to `Synced`.
    7. Multi-screen navigation in shell `MainForm` (SCR-01) verifies smooth transitions across all screens (SCR-01 through SCR-07) and keyboard shortcuts.
  - Configured portable build and distribution output in `BtrReturGudang.Winform.csproj` enabling `<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>` to copy all required dependencies (Dapper, RestSharp, Microsoft.Extensions.DependencyInjection, System.Text.Json, etc.) to `bin\Release\net471`.
  - Created automated portable distribution packager script `publish-portable.ps1` at `src/j09-btr-retur-gudang/publish-portable.ps1` generating xcopy-ready directory output at `dist/BtrReturGudang-Portable` and standalone ZIP archive `dist/BtrReturGudang-Portable.zip` with `Run-BtrReturGudang.bat` launcher.
  - Validated build cleanly via `dotnet build -c Release` with 0 warnings and 0 errors.
  - All 105 tests across all test suites (`AppTier.Tests`, `Infrastructure.Tests`, `Winform.Tests`) pass cleanly with 0 errors and 0 failures.
- **Changed Files:**
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/ReturnOrderEntryView.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/Views/MainForm.cs`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform/BtrReturGudang.Winform.csproj`
  - `src/j09-btr-retur-gudang/BtrReturGudang.Winform.Tests/EndToEndWorkflowIntegrationTests.cs`
  - `src/j09-btr-retur-gudang/publish-portable.ps1`

---

# 6. Change Log

- **2026-10-11 (v1.0):** Initial creation of IMPLEMENTATION-PLAN for `BTR-RETUR-DESKTOP-001` incorporating approved Screen Design Inventory (SCR-01 through SCR-07) across 4 execution phases and 11 continuous slices.
