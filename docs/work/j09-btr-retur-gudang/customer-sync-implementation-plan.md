# Implementation Plan — Local Customer Sync & Persistence (BTRG_Customer)

## 1. Overview
In `src/j09-btr-retur-gudang/BtrReturGudang.sln`, the local customer cache (`BTRG_Customer`) is not populated because the application defaults to `http://localhost:5000` with an empty `ServerTargetId`. The staging cloud endpoint `http://dev.smart-ics.com:8089/belajar-api/api/Customer/JOG` exposes the complete customer dataset.

This plan details the technical changes to:
1. Pull customer data from `http://dev.smart-ics.com:8089/belajar-api/api/Customer/{ServerTargetId}` with default `ServerTargetId = JOG`.
2. Persist `customerId`, `customerCode`, `customerName`, `alamat`, and `wilayah` in `BTRG_Customer`.
3. Freeze `customerCode` and `wilayah` in `BTRG_ReturnOrder` at order intake time.
4. Support dynamic configuration of `ApiBaseUrl` in `RegistryHelper` and `SettingsView`.
5. Enhance customer search and autocomplete display in `ReturnOrderEntryView`.

---

## 2. Technical Slices

### Slice 1: Database Migration Scripts [COMPLETED]
- **Target:** `BtrReturGudang.SqlDb`
- **Actions:**
  - [x] Create `Scripts/002_UpdateCustomerAndReturnOrderColumns.sql` to alter existing tables gracefully:
    - In `BTRG_Customer`: add/rename columns to `CustomerId VARCHAR(20) NOT NULL PRIMARY KEY`, `CustomerCode VARCHAR(20) NULL`, `CustomerName VARCHAR(100) NOT NULL`, `Alamat VARCHAR(250) NULL`, `Wilayah VARCHAR(100) NULL`, and indexes.
    - In `BTRG_ReturnOrder`: add `CustomerCode VARCHAR(20) NULL`, `Wilayah VARCHAR(100) NULL`.
  - [x] Update `Scripts/001_CreateWarehouseTables.sql` to include these columns in base DDL for fresh setups.

### Slice 2: Domain Entities [COMPLETED]
- **Target:** `BtrReturGudang.Domain`
- **Actions:**
  - [x] Update `Customer.cs`:
    - Properties: `CustomerId`, `CustomerCode`, `CustomerName`, `Alamat`, `Wilayah`.
    - Backwards-compatible aliases: `CustId => CustomerId`, `CustName => CustomerName`, `CustCode => CustomerCode`.
    - Updated constructors.
  - [x] Update `ReturnOrder.cs`:
    - Add `CustomerCode` and `Wilayah` properties.
    - Update factory/constructors to support `CustomerCode` and `Wilayah`.

### Slice 3: AppTier Contracts & DTOs [COMPLETED]
- **Target:** `BtrReturGudang.AppTier`
- **Actions:**
  - [x] Update `CustomerDto` in `DTOs/ReferenceDtos.cs`:
    - Include `CustomerId`, `CustomerCode`, `CustomerName`, `Alamat`, `Wilayah`.
    - Bi-directional domain mapping `FromDomain` / `ToDomain`.
    - Aliases `CustId`, `CustName`, `CustCode`.
  - [x] Update `ReturnOrderDto.cs`:
    - Add `CustomerCode`, `Wilayah`, and `CustCode` fields.
    - Update mapping `FromDomain` / `ToDomain`.
  - [x] Update `IRegistryHelper.cs`:
    - Add `string ApiBaseUrl { get; set; }`.

### Slice 4: Infrastructure Persistence & HTTP Client [COMPLETED]
- **Target:** `BtrReturGudang.Infrastructure`
- **Actions:**
  - [x] Update `RegistryHelper.cs`:
    - Add `KeyApiBaseUrl = "ApiBaseUrl"`.
    - Default `ApiBaseUrl` to `"http://dev.smart-ics.com:8089/belajar-api"`.
    - Default `ServerTargetId` to `"JOG"` when unconfigured.
  - [x] Update `BtradeApiClient.cs`:
    - Accept dynamic `Func<string> baseUrlProvider` or resolve `_registryHelper.ApiBaseUrl`.
    - Update `ParseCustomers` to parse `customerId`, `customerCode`, `customerName`, `alamat`, and `wilayah`.
  - [x] Update `MasterDataDal.cs`:
    - Update `BulkUpsertCustomer` / `BulkUpsertCustomerAsync` with SQL merge on `CustomerId` updating `CustomerCode`, `CustomerName`, `Alamat`, and `Wilayah`.
    - Update `SearchCustomer` / `SearchCustomerAsync` to search across `CustomerId`, `CustomerCode`, `CustomerName`, and `Wilayah`.
    - Update `GetCustomerById` / `GetCustomerByIdAsync` targeting `CustomerId`.
  - [x] Update `ReturnOrderDal.cs`:
    - Save and load `CustomerCode` and `Wilayah` in `BTRG_ReturnOrder`.

### Slice 5: WinForms Presentation Layer [COMPLETED]
- **Target:** `BtrReturGudang.Winform`
- **Actions:**
  - [x] Update `SettingsView.cs`:
    - Load `_txtApiUrl.Text` from `_registryHelper.ApiBaseUrl`.
    - Default `_txtServerTargetId.Text` to `"JOG"` if empty.
    - Persist `_txtApiUrl.Text` into `_registryHelper.ApiBaseUrl` in `OnSaveAllClick`.
  - [x] Update `ReturnOrderEntryView.cs`:
    - Update `CustomerItem` class to store `CustomerId`, `CustomerCode`, `CustomerName`, and `Wilayah`.
    - Format `ToString()` as `$"[{CustomerCode}] {CustomerName} - {Wilayah}"` (or `$"[{CustomerCode}] {CustomerName}"` if Wilayah empty).
    - Pass `CustomerCode` and `Wilayah` when creating the `ReturnOrder`.

### Slice 6: Verification & Testing [COMPLETED]
- **Target:** All test projects
- **Actions:**
  - [x] Update unit tests in `BtrReturGudang.AppTier.Tests`, `BtrReturGudang.Infrastructure.Tests`, and `BtrReturGudang.Winform.Tests`.
  - [x] Run full test suite (`dotnet test`): 108 passed, 0 failed.
  - [x] Verify live endpoint `http://dev.smart-ics.com:8089/belajar-api/api/Customer/JOG`: 2,280 customer records successfully verified with `customerId`, `customerCode`, `customerName`, `alamat`, and `wilayah`.
