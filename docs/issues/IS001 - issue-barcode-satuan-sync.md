# ISSUE

## Metadata

ID: ISSUE-BARCODE-SATUAN-001
Type: BUG
Status: Open
Title: BTRG_Barang Satuan Data Not Properly Synchronized from BTRADE_Brg

## Source

Reported By: User
Reported Date: 2026-11-07

## Description

In the Barcode Registration popup (Pendaftaran Barcode Baru / Registrasi Barcode Produk) in `BtrReturGudang.Wpf`, the Satuan (unit of measure) combo box displays units that do not correspond to the product's registered units.

Each product/item already has its units defined in table `BTR_BrgSatuan`, which belongs to `j05-btr-distrib` (not `BtrReturGudang`). The expected data flow is:

1. `BTR_BrgSatuan` (j05-btr-distrib) → sends unit data to cloud table `BTRADE_Brg` (fields `SatBesar` and `SatKecil`)
2. `BTRADE_Brg` → should be synchronized to `BTRG_Barang` (j09-btr-retur-gudang)

Currently, `BTRG_Barang` contains 3 unit fields instead of the expected 2 (`SatBesar` and `SatKecil`), and the field names are not standardized. The combo box is populated from these inconsistent fields, resulting in incorrect unit options being shown to the user.

## Desired Outcome

- `BTRG_Barang` table should contain exactly 2 unit fields: `SatBesar` and `SatKecil`
- The Satuan combo box in the Barcode Registration popup should display only the correct units from the product's registered units (`BTR_BrgSatuan`), synchronized via `BTRADE_Brg`
- Field names in `BTRG_Barang` should be standardized to match `BTRADE_Brg` (`SatBesar` and `SatKecil`)

## Current Situation

- `BTRG_Barang` table currently has 3 unit-related fields (field names not standardized)
- The Satuan combo box in the Barcode Registration popup displays units that do not match the product's registered units from `BTR_BrgSatuan`
- The synchronization path from `BTRADE_Brg` to `BTRG_Barang` is either incomplete or mapping incorrectly

## Evidence

- Observed in: `BtrReturGudang.Wpf` – Barcode Registration popup, Satuan combo box
- Source table: `BTR_BrgSatuan` (j05-btr-distrib)
- Cloud table: `BTRADE_Brg` (fields `SatBesar`, `SatKecil`)
- Target table: `BTRG_Barang` (j09-btr-retur-gudang) – currently has 3 unit fields instead of 2

## Notes

- This is a data synchronization and schema consistency issue between j05-btr-distrib and j09-btr-retur-gudang
- The fix should ensure the correct 2-field structure (`SatBesar`, `SatKecil`) and proper data flow from the source system