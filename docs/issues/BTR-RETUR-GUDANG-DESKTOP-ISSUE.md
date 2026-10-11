# ISSUE

## Metadata

ID: `BTR-RETUR-GUDANG-DESKTOP-001`
Type: `CHANGE-REQUEST`
Status: `New`
Title: **Warehouse Operational App — Replace Rejected Android Client (BGud) with Dedicated Windows Desktop Application (.NET 4.7.1)**

## Source

Reported By: Client / Stakeholder (relayed via repository owner)  
Reported Date: 2026-10-11

## Description

The Android mobile warehouse application (`src/BGud`) has been rejected by the client for operational use. Warehouse receiving operations must instead be conducted from a dedicated Windows desktop application running on the onsite warehouse PC.

Due to physical workstation constraints and hardware/OS limitations at the warehouse, the desktop application must target the .NET Framework 4.7.1 runtime environment, consistent with the existing warehouse application (`src/j07-btr-gudang`).

Physical goods intake at the warehouse receiving station will be performed using a dedicated USB barcode scanner device connected to the workstation PC.

## Desired Outcome

- Warehouse officers can record physical goods returned by customers (Return Orders) and manage product barcode registrations directly from a dedicated Windows desktop application running on the warehouse PC.
- Maintain full functional parity with the operational workflows previously targeted for BGud:
  - Create, view, edit (pre-sync), delete (pre-sync), and synchronize Return Orders.
  - Fast item receiving supporting return classification (`BAGUS` / `RUSAK`) and packaging units.
  - Search product barcode catalog and register unrecognized barcodes on-the-spot.
- Seamless barcode capture using the dedicated USB barcode scanner device during physical receiving.
- Application executes reliably within the onsite PC runtime constraints (.NET Framework 4.7.1).
- Operational data captures locally and synchronizes upstream with the BTR platform without disrupting existing Main Office and Cloud relay pipelines.

## Current Situation

- An Android application was developed in `src/BGud` utilizing Jetpack Compose, Room (SQLite), and Retrofit, but client operational requirements mandate a desktop workstation solution instead.
- The onsite warehouse PC currently hosts `src/j07-btr-gudang` (`BtrGudang.Winform`) targeting .NET Framework 4.7.1 and connecting to a local SQL Server instance (`(local)\btrgd`).
- Upstream backend staging and Cloud API endpoints for Return Order submission (`POST api/return-order`) and Barcode management (`GET api/barcodes/sync`, `POST api/barcode-registration`) are already realized in `src/j06-pkl-btrade-api`.
- Main Office authoritative entities and synchronization mechanisms are defined in `src/j05-btr-distrib` and `src/j07-btrade-sync`.

## Evidence

- Rejected mobile application source: `src/BGud`
- Existing warehouse desktop reference (.NET 4.7.1 baseline): `src/j07-btr-gudang`
- Return Order domain specification: `docs/work/return-order/RETURN-ORDER-DOMAIN.md`
- Return Order architecture blueprint: `docs/work/return-order/RETURN-ORDER-ARCHITECTURE.md`
- Upstream Cloud API contracts: `src/j06-pkl-btrade-api`
- Stakeholder alignment session records (2026-10-11)

## Notes

The following operational points were clarified during initial stakeholder intake and must be considered during downstream analysis:

1. **Functional Scope:** Full functional parity with BGud (both Return Order receiving and Barcode Registry/Lookup are required).
2. **Device Input:** Dedicated USB barcode scanner operating in keyboard wedge mode, requiring fluid scan capture ergonomics and audible/visual cues.
3. **Data Persistence & Connectivity:** Onsite PC operates with local database persistence (`(local)\btrgd`), syncing upstream via Cloud API endpoints.
4. **Target Structure:** Standalone solution to be created at `src/j09-btr-retur-gudang` with portable execution distribution.
