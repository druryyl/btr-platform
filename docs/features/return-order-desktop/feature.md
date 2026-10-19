---
Title: Warehouse Desktop Return Order Capture and Barcode Management
Code: BTR-RETUR-DESKTOP-001
Artifact: FEATURE
Version: 1.1
LastUpdated: 2026-10-19
---

# 1. Purpose

Provide a dedicated Windows desktop client for Warehouse Officers to record physical goods returned by Customers (Return Orders) and manage product barcode registrations at the warehouse receiving station, replacing the rejected Android mobile client (`src/BGud`).

# 2. Business Outcome

- Warehouse Officers record returned physical goods immediately upon arrival at the warehouse workstation using a connected USB barcode scanner.
- Goods returns are classified into sellable (`BAGUS`) and damaged (`RUSAK`) quantities with appropriate physical packaging units.
- Unrecognized barcodes can be mapped and registered on-the-spot without interrupting return intake.
- Return Orders and Barcode Registrations persist reliably in the local warehouse database even during network interruptions.
- Captured operational records synchronize upstream to the BTR platform, serving as authoritative operational source documents for Main Office Sales Return generation.

# 3. Participating Domains

| Domain | Responsibility within this feature |
| ------ | ---------------------------------- |
| Warehouse Return Order | Owns the operational receiving document (header and item lines), classification (`BAGUS`/`RUSAK`), lifecycle (`Draft` -> `Synced`), and submission. |
| Barcode Registry | Owns product barcode mapping, validation, duplicate prevention, and on-the-spot barcode registration requests. |
| Master Data (Item / Customer / Driver / Salesman) | Supplies authoritative reference data for product lookup, unit conversions, customer identification, and transport actors. |
| Warehouse Administration | Provides workstation assignment, warehouse location context (`WarehouseCode`), and active operator session identification. |

# 4. Trigger

A delivery vehicle, driver, or customer arrives at the warehouse dock with physical goods being returned, and the Warehouse Officer opens the Return Order entry interface on the warehouse desktop station.

# 5. Preconditions

- The desktop workstation is configured with a valid Warehouse / Depo assignment and an active Shift Operator in `[F5] Pengaturan` → *Identitas Workstation & Shift Operator*. Return Order capture (`Save Draft` / `Save & Upload`) is refused with a clear message when either is missing; no placeholder warehouse or operator is substituted.
- An active Warehouse Officer session is selected/active on the workstation.
- Master reference data (Items, Barcodes, Customers, Drivers, SalesPersons) is available in the local workstation database.
- A USB barcode scanner is connected to the workstation PC.

# 6. Operational Flow

```text
[Warehouse Officer]                                           [Desktop Application]
        │                                                              │
        │── 1. Select Customer (and optional Driver / Salesman) ─────►│
        │                                                              │
        │── 2. Scan physical item barcode via USB Scanner ───────────►│
        │                                                              ├─ Validate barcode
        │                                                              │
        ├──[If Barcode Found]──────────────────────────────────────────┤
        │                                                              ├─ Display product name & unit
        │   ◄─ Focus on Quantity Strip (Bagus / Rusak) ────────────────┤
        │── 3. Enter Qty Bagus and/or Qty Rusak, press Enter ─────────►│
        │                                                              ├─ Commit line item to grid
        │   ◄─ Play Success Beep; return focus to scanner ─────────────┤
        │                                                              │
        ├──[If Barcode Unknown]────────────────────────────────────────┤
        │   ◄─ Play Warning Beep; prompt Quick Barcode Registration ───┤
        │── 3a. Search Item master, select Unit, confirm link ────────►│
        │                                                              ├─ Queue barcode registration
        │                                                              ├─ Immediately add item to return
        │   ◄─ Return focus to scanner for next item ──────────────────┤
        │                                                              │
        ├──[If Barcode Missing / Damaged]──────────────────────────────┤
        │── 3b. Press F2 (Manual Item Search) ────────────────────────►│
        │                                                              ├─ Open search grid
        │── 3c. Filter item by code/name, select Unit & Quantities ───►│
        │                                                              ├─ Commit line item to grid
        │                                                              │
        │── 4. Complete intake and choose "Simpan & Upload" ──────────►│
        │                                                              ├─ Save Return Order as Draft
        │                                                              ├─ Trigger background upload
        │                                                              │  (transitions to Synced upon success)
        │   ◄─ Display confirmation & clear form for next intake ──────┤
```

# 7. Domain Orchestration

1. **Warehouse Administration -> Master Data:** The workstation session provides the active `WarehouseCode` and operator identity, scoping reference queries and transaction ownership.
2. **Barcode Registry -> Master Data:** Scanned barcodes are resolved against the local Barcode and Item catalog. If unmapped, on-the-spot registration associates the new barcode with an authoritative Item (`BrgId`) and packaging unit (`Satuan`).
3. **Warehouse Return Order -> Master Data:** Each line item captures the verified `BrgId`, physical packaging unit (`SatId`), and specified quantities (`QtyBagus`, `QtyRusak`), maintaining referential consistency with the Customer and optional transport actors.
4. **Warehouse Return Order -> Cloud Relay:** Completed Return Orders and queued Barcode Registrations are relayed upstream via the synchronization service without blocking local warehouse operations.

# 8. Constraints

- **Single Authority Preservation:** Physical Return Orders remain operational intake records; financial valuation, pricing, and accounting posting remain exclusively with Main Office Sales Return (`ReturJual`) processing (DOMAIN §1, §15).
- **Post-Sync Immutability:** Once a Return Order is synchronized upstream (`Synced`), it becomes read-only on the warehouse workstation to prevent split-brain divergence (P-10, ADR-RO-006).
- **Runtime Constraints:** The client must operate within the .NET Framework 4.7.1 runtime environment on the onsite warehouse PC.
- **Display Resolution Constraint (1280x720):** Workstation monitors operate at 1280x720 physical resolution with Windows OS taskbar (~40-48px height), yielding an effective desktop working area of ~1280x672 px. All views (SCR-01 through SCR-07) and modal dialogs must fit comfortably within this viewport without vertical clipping, control truncation, or hiding action buttons. In particular, the intake screen (SCR-02) must preserve sufficient vertical space to display at least 8-10 rows in the items DataGridView simultaneously.

# 9. Exceptions

| Exception Condition | System Reaction | Actor Action |
| ------------------- | --------------- | ------------ |
| Barcode not found in catalog | Emits warning tone; displays modal to register the barcode to a known Item. | Operator searches Item master, selects packaging unit, confirms link, or cancels. |
| Barcode is torn, smudged, or missing | Scanner fails to read label. | Operator presses `F2` to trigger manual item search by item code or name. |
| Customer not found in local cache | System flags missing customer record. | Operator triggers reference data sync or verifies customer code with office. |
| Network offline during "Simpan & Upload" | Document saves locally as `Draft` (`Pending Upload`); visual offline badge shown. | Operator continues operations; document auto-uploads when connection is restored. |
| Attempt to edit already synchronized document | System blocks modification and displays document in read-only mode. | Operator views details or creates a new Return Order if additional goods arrive. |

# 10. Acceptance Criteria

1. Warehouse Officer can create a complete Return Order containing customer, optional driver/salesman, and multiple line items with `Qty Bagus` and `Qty Rusak`.
2. USB barcode scanner input is captured seamlessly on the return screen without requiring mouse clicks into a text box.
3. System emits distinct audio tones for scan success vs scan error/unrecognized barcode.
4. Pressing `F2` allows instant manual item lookup filtering by item code or name.
5. Unrecognized barcodes can be linked to an item on-the-spot and immediately added to the active Return Order while queuing registration.
6. Return Orders and Barcode Registrations save to the local SQL Server database (`btrgd`) and persist across application restarts.
7. Return Orders upload upstream to the Cloud API relay (`POST api/return-order`) and transition from `Draft` to `Synced`.
8. Synchronized Return Orders become read-only on the warehouse workstation.
9. Application runs reliably under .NET Framework 4.7.1 on the onsite PC.
10. Customer master synchronization pulls from the cloud API endpoint (`api/Customer/{ServerTargetId}`) with default server target `JOG` and base URL `http://dev.smart-ics.com:8089/belajar-api`, persisting `customerId`, `customerCode`, `customerName`, `alamat`, and `wilayah` in `BTRG_Customer`. Return Orders freeze `customerCode` and `wilayah` at document creation time.
11. The application shell and all embedded views cleanly fit within a 1280x720 desktop display without window overflow, bottom action toolbar clipping, or scrollbar obscuration. The main shell starts maximized on 720p screens or adapts its initial dimensions within the OS working area.
12. In the Return Order intake view (SCR-02), the document header, scanner intake strip, and action toolbar are streamlined so that the items DataGridView displays at least 8-10 rows concurrently without vertical scrolling on a 1280x720 display.
13. In the Return Order list and inspector view (SCR-05), the split view layout proportions prevent metadata truncation and ensure the detail items grid displays at least 5-6 rows comfortably.
14. All modal dialogs (SCR-03 Manual Item Search, SCR-04 Quick Barcode Registration, SCR-08 Manual Customer Search, BarcodeRegistrationDialog, OperatorSwitcherDialog) fit comfortably within the 720p screen height (< 520px height) so their header, inputs, and action buttons remain fully visible and operable without clipping.
15. Customer selection utilizes on-demand keyboard-driven modal search (SCR-08) with real-time keydown filtering over large datasets (> 2,000 customers), eliminating startup ComboBox memory bloat and UI thread freezes while preserving manual walk-in customer capture (`CUST-MANUAL`).
