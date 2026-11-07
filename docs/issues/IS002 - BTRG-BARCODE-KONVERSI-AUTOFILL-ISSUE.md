# ISSUE

## Metadata

ID: BTRG-BARCODE-KONVERSI-001
Type: CHANGE-REQUEST
Status: Open
Title: Auto-populate Conversion Multiplier in Barcode Registration from Synchronized BTRADE_Brg Data

## Source

Reported By: User
Reported Date: 2026-11-07

## Description

In the 'Registrasi Barcode Produk' form (pop-up window) of `BtrReturGudang.Wpf`, there is a field for inputting 'Pengali (konversi ke satuan kecil)'. Currently, users must input this multiplier manually.

The cloud database table `BTRADE_Brg` (used by `j06-pkl-btrade-api` for data synchronization) already possesses the item conversion value (`Konversi`). A change is requested so that this conversion factor is stored in the local warehouse database (`BTRG_Barang`) and automatically displayed in the 'Registrasi Barcode Produk' UI when an item is selected, removing the need for manual input by warehouse operators.

## Desired Outcome

- The product conversion multiplier is stored locally in the warehouse database (`BTRG_Barang`) through data synchronization with cloud master data (`BTRADE_Brg`).
- When registering or linking a barcode in the 'Registrasi Barcode Produk' window in `BtrReturGudang.Wpf`, the conversion multiplier field ('Pengali') automatically reflects the pre-configured conversion value of the selected product without requiring manual data entry.

## Current Situation

- The 'Pengali (Konversi ke Pcs)' input field in `BarcodeRegistrationDialog` / `QuickBarcodeRegistrationDialog` requires manual entry or defaults to 1.
- Local table `BTRG_Barang` in `BtrReturGudang` does not currently store the conversion value from cloud `BTRADE_Brg`.
- Cloud table `BTRADE_Brg` in `j06-pkl-btrade-api` contains the `Konversi` field alongside `SatBesar` and `SatKecil`, but this conversion value is not populated to the local database or displayed automatically in the barcode registration UI.

## Evidence

- Form UI: [BarcodeRegistrationDialog.xaml](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/BarcodeRegistrationDialog.xaml) & [QuickBarcodeRegistrationDialog.xaml](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Wpf/Views/QuickBarcodeRegistrationDialog.xaml) — contains `TxtPengali` text box.
- Cloud table schema: [BTRADE_Brg.sql](file:///d:/Project.Private/btr-platform/src/j06-pkl-btrade-api/btrade.sqldb/SalesContext/BTRADE_Brg.sql) — defines `Konversi INT NOT NULL`.
- Local table definition & entity: [001_CreateWarehouseTables.sql](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.SqlDb/Scripts/001_CreateWarehouseTables.sql) and [Barang.cs](file:///d:/Project.Private/btr-platform/src/j09-btr-retur-gudang/BtrReturGudang.Domain/Entities/Barang.cs) — `BTRG_Barang` currently lacks conversion persistence.
- Related issue: [IS001 - issue-barcode-satuan-sync.md](file:///d:/Project.Private/btr-platform/docs/issues/IS001%20-%20issue-barcode-satuan-sync.md).

## Notes

- This change request directly relates to improving warehouse operator accuracy and eliminating manual entry errors during barcode registration.
- Detailed technical design and schema migration decisions are deferred to the Analyst and Architect roles.
