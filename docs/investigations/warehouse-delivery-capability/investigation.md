# Investigation: Existing Warehouse & Delivery Capability Assessment

**Date:** 2026-07-23  
**Type:** Capability baseline investigation (as-is)  
**Objective:** Establish an evidence-backed baseline of existing Warehouse, Delivery, and Faktur fulfillment capabilities before designing Logistics Management.  
**Constraint:** Investigation only — no redesign, no new business concepts introduced unless already present in code.

**Related prior work:**
- `docs/investigations/faktur-distribution-sync/investigation.md` — PackingOrder sync reliability (Desktop → Sync → Cloud → Gudang)

**Foundation context used:**
- `docs/foundation/PRODUCT.md` — Warehouse Fulfillment is a core capability; Warehouse Staff pick/pack
- `docs/foundation/DOMAIN.md` — Warehouse (logical), Depo (physical), Picking, Packing
- `docs/foundation/LANDSCAPE.md` — BTR Gudang owns warehouse operations; Inventory area
- `docs/foundation/WORKFLOW.md` — Sales → Faktur → Warehouse Fulfillment → Customer → Faktur Kembali
- `docs/features/faktur/feature.md` — inventory deducted on create; one Warehouse per Faktur

---

## Status Legend

| Label | Meaning |
| ----- | ------- |
| **Implemented** | Present in code, wired to UI/menu, used in operational path |
| **Partially Implemented** | Exists but incomplete, hidden, dual-path, or not end-to-end |
| **Legacy / Unused** | Code or schema present but not active in current menus/flows |
| **Missing** | Searched by name and behavior; no evidence found |

---

# 1. Executive Summary

BTR’s warehouse fulfillment is **document-driven print/prepare**, not a task-managed WMS or delivery execution system.

**What exists today:**

1. **Faktur** is created in BTR Desktop with `WarehouseId`, `DriverId`, and `TglRencanaKirim`.
2. **Inventory is deducted immediately** on Faktur save from that Warehouse (not at pick/pack/delivery).
3. A **PackingOrder** is created from the Faktur and synced (best-effort) to **BTR Gudang** for Excel printing:
   - Per-Supplier ≈ operational picking list
   - Per-Faktur ≈ operational packing list
4. A **parallel office Packing** feature (`Packing2Form`) groups fakturs by Driver + DeliveryDate and prints Excel packing lists from the branch DB.
5. **Faktur Kembali** is marked manually in Faktur Control when the signed invoice returns.
6. **Driver** is a master used for assignment and reporting — not for trip/schedule/execution.

**What does not exist:**

- No first-class Delivery Operations module
- No Surat Jalan, Shipment, Route (delivery), Manifest, Truck Schedule, Loading Group, or Loading verification
- No warehouse pick/pack task confirmation or warehouse task board
- No delivery status, proof of delivery, failed-delivery tracking, or delivery history
- `DeliveryAgg` exists only as an unused domain stub (`DeliveryId`, `DeliveryDate`) with no DAL/UI/table

**Architectural implication for Logistics Management:**  
The strongest reusable foundation is the **Faktur → PackingOrder → Gudang print** pipeline plus **Driver / Warehouse / Depo master data** and **planned delivery fields**. Delivery execution and loading coordination are currently **outside the system**.

---

# 2. Existing Warehouse Capability

## 2.1 Systems Involved

| System | Path | Role |
| ------ | ---- | ---- |
| BTR Desktop | `src/j05-btr-distrib` | Source of truth: Warehouse/Driver master, Faktur, Packing, PackingOrder |
| BTR Sync | `src/j07-btrade-sync` | Uploads PackingOrder when `UploadPackingOrder=1` |
| Cloud API | `src/j06-pkl-btrade-api` | Hub for PackingOrder upload/download by Depo |
| BTR Gudang | `src/j07-btr-gudang` | Warehouse app: download + Excel print |
| BTR Portal | `btr.portal.web` | Analytics (warehouse performance KPIs) — not ops fulfillment |
| BTrade3 | `src/BTrade3` | No warehouse fulfillment code found |

## 2.2 Capability Matrix

| Capability | Status | Evidence |
| ---------- | ------ | -------- |
| Warehouse master CRUD | **Implemented** | `WarehouseForm` (menu IM2-Gudang); `BTR_Warehouse` |
| Stock balance by warehouse | **Implemented** | `BTR_StokBalanceWarehouse`; stock reports |
| Assign Warehouse on Faktur | **Implemented** | `FakturForm` + `BTR_Faktur.WarehouseId` |
| Depo master data | **Implemented** (DAL); dedicated Depo UI **Missing** | `BTR_Depo`; `IDepoDal`; Depo set via Supplier |
| Picking as domain entity / confirmation | **Missing** | No Picking aggregate/table/screen |
| Operational picking document | **Implemented** (print proxy) | Gudang PK1 `PER-SUPPLIER` Excel |
| Packing as domain entity (office) | **Implemented** | `BTR_Packing*` + `Packing2Form` |
| Packing as warehouse print | **Implemented** | Gudang PK1 `PER-FAKTUR` Excel |
| PackingOrder sync to Gudang | **Partially Implemented** | Opt-in sync flag; H-3 window; best-effort (see prior investigation) |
| Warehouse task / status workflow | **Missing** | No task board, pick confirm, pack confirm |
| Loading / shipment verification | **Missing** | No tables/screens |
| Warehouse ops dashboard | **Missing** (Portal is analytics only) | `LocationDashboardView.vue` KPIs |

## 2.3 Domain Model (Warehouse-related)

| Concept | Model | Notes |
| ------- | ----- | ----- |
| Warehouse | `WarehouseModel` | Logical inventory grouping; `IsSpecial`, `IsAktif` |
| Depo | `DepoType` / `IDepoKey` | Physical storage; linked via `Supplier.DepoId` |
| Packing | `PackingModel` + Faktur/Brg children | Office grouping by Driver + DeliveryDate |
| PackingOrder | `PackingOrderModel` + Item + Depo | Sync/print payload derived from Faktur |
| Driver | `DriverModel` | Master; assigned on Faktur / Packing / PackingOrder |
| Picking | — | **Missing** as entity; product language maps to PER-SUPPLIER print |

## 2.4 Warehouse Workflow (as implemented)

```text
Faktur saved (Warehouse assigned)
      ↓
Inventory deducted from WarehouseId
      ↓
PackingOrder created (items tagged with Depo via Supplier)
      ↓
[Sync] Cloud → Gudang (per Depo)
      ↓
Warehouse prints PER-SUPPLIER (pick) / PER-FAKTUR (pack)
      ↓
Manual physical pick / pack / load  ← outside system
```

**Warehouse vs Depo (critical distinction):**

| Concept | Role in stock | Role in warehouse print |
| ------- | ------------- | ----------------------- |
| **Warehouse** | Stock ledger bucket on Faktur | Not used as Gudang download filter |
| **Depo** | Not used for stock balance | Gudang download filter (`warehouseCode` API param = DepoId) |

One PackingOrder can contain items for **multiple Depo**, supporting the operational pattern where goods for one Faktur (or one driver’s load) may be picked from more than one physical location.

Evidence: `PackingOrderModel.CreateFromFaktur`; `FakturItemDal.ListBrgDepo` (Brg → Kategori → Supplier → Depo); Cloud `GET .../{warehouseCode}/...` documented as DepoId in sync investigation.

## 2.5 UI — Warehouse

### BTR Desktop — Active

| Screen | Menu | Path |
| ------ | ---- | ---- |
| WarehouseForm | IM2-Gudang | `btr.distrib/InventoryContext/WarehouseAgg/WarehouseForm.cs` |
| DriverForm | IM4-Driver | `btr.distrib/InventoryContext/DriverAgg/DriverForm.cs` |
| Packing2Form | IT3-Packing | `btr.distrib/InventoryContext/PackingAgg/Packing2Form.cs` |
| FakturForm | Sales | assigns Warehouse / Driver / TglRencanaKirim; SavePackingOrder |
| DriverFakturInfoForm | SF8 | fakturs per driver |
| StokBalance info forms | Inventory reports | stock by warehouse |

### BTR Gudang — Active

| Screen | Menu | Path |
| ------ | ---- | ---- |
| DL1DownloaderForm | DL1 | download PackingOrder |
| DL2DownloadPackingOrderInfoForm | DL2 | download info |
| DL3PendingDownloaderForm | DL3 | pending download |
| PK1PrintPackingOrderForm | PK1 | Excel print |

### Legacy

| Screen | Status | Evidence |
| ------ | ------ | -------- |
| PackingForm | **Legacy / Unused** | Not opened from MainForm (menu opens Packing2Form); calls `.Warehouse()` which no longer exists on PackingBuilder |

---

# 3. Existing Delivery Capability

## 3.1 Verdict

There is **no Delivery Operations module**. Delivery-related behavior is limited to:

- planned fields on Faktur (`DriverId`, `TglRencanaKirim`)
- Driver assignment on Packing / PackingOrder / ReturJual
- office packing lists grouped by Driver + DeliveryDate
- Faktur Control status **Kirim** (domain exists; **UI column hidden**)
- void reason **Customer Reject** (cancels invoice; not a delivery failure state)

## 3.2 Capability Matrix

| Capability | Status | Evidence |
| ---------- | ------ | -------- |
| Driver master | **Implemented** | `BTR_Driver`, `DriverForm` |
| Driver assignment on Faktur | **Implemented** | `BTR_Faktur.DriverId`; FakturForm |
| Planned delivery date | **Implemented** | `TglRencanaKirim`; default = FakturDate + 1 day |
| Packing as driver daily load list (proxy) | **Partially Implemented** | `BTR_Packing` groups by Driver + DeliveryDate; search uses Faktur date range, not TglRencanaKirim |
| Driver Faktur report | **Implemented** | SF8 `DriverFakturInfoForm` |
| Driver on Retur Jual | **Implemented** | `BTR_ReturJual.DriverId` |
| DeliveryAgg | **Legacy / Unused** | Domain stub only — no DAL, UI, SQL table |
| Status Kirim | **Partially Implemented** | `StatusFakturEnum.Kirim` + builder; `FakturGrid.Columns.GetCol("Kirim").Visible = false` |
| Surat Jalan | **Missing** | Zero matches in fulfillment code |
| Delivery confirmation / POD / history | **Missing** | No screens/tables |
| Failed delivery tracking | **Missing** | Only void reason 3 “Customer Reject” |
| Shipment / Batch / Manifest / Truck Schedule / Loading Group | **Missing** | Zero matches by name or behavior |
| Delivery route planning | **Missing** | `SalesRute` / VisitPlan are sales visit concepts, not delivery |

## 3.3 Driver Participation

Driver **is not merely unused master data**. It participates as an **assignment attribute** in:

| Workflow | Participation |
| -------- | ------------- |
| Faktur create | Planned driver on invoice |
| PackingOrder | Copied from Faktur (`DriverReff`) |
| Packing (office) | Header key with DeliveryDate |
| Faktur print | `DriverName` on printout |
| Driver Faktur Info | Report by driver + date |
| Retur Jual | Driver recorded on return |

Driver does **not** participate in: scheduling, capacity, truck assignment, multi-warehouse loading plan, trip check-in/out, GPS, or delivery confirmation.

## 3.4 Concepts Searched by Behavior (Not Found)

Searched terminology and operational behavior for: Shipment, Delivery Batch, Route (delivery), Delivery Group, Loading Group, Delivery Manifest, Truck Schedule, Loading Sheet, Surat Jalan, Pengiriman entity, Kurir.

**Closest existing proxies:**

| Business intent | Closest existing artifact |
| --------------- | ------------------------- |
| Planned delivery | `TglRencanaKirim` |
| Driver daily load list | `BTR_Packing` (Driver + DeliveryDate) |
| Warehouse work order | `PackingOrder` |
| “Sent” status | `StatusFakturEnum.Kirim` (hidden UI) |

---

# 4. Faktur Fulfillment Workflow

## 4.1 Complete As-Is Lifecycle (Post Create Faktur)

```text
[Office — BTR Desktop FakturForm]
Create / Save Faktur
  ├─ Persist BTR_Faktur (+ WarehouseId, DriverId, TglRencanaKirim)
  ├─ GenStokFakturWorker → FIFO stock out from WarehouseId
  ├─ Event → BTR_FakturControlStatus = Posted
  ├─ Event → BTR_Doc (print queue) — Partially / likely legacy UI
  ├─ SavePiutang
  ├─ SavePackingOrder → BTR_PackingOrder (+ Item + Depo)
  └─ Print Faktur (RDLC)

[Optional Sync — BTR Sync]
  └─ Upload PackingOrder → Cloud BTRADE_*  (if UploadPackingOrder=1)

[Warehouse — BTR Gudang]
  ├─ DL1/DL3 download by DepoId → BTRG_*
  └─ PK1 print Excel (PER-SUPPLIER / PER-FAKTUR / Faktur List)

[Office — Parallel path]
  └─ Packing2Form → BTR_Packing* → Excel packing lists by Driver + DeliveryDate

[Physical operations — outside system]
  ├─ Manual picking / packing
  ├─ Loading (driver may load from both warehouses)
  ├─ Depart ~07:00–08:00 / deliver / return ~14:00–15:00
  └─ (times from business context; not encoded in software)

[Office — Faktur Control]
  ├─ KembaliFaktur checkbox when signed Faktur returns
  ├─ Kirim — domain ready, UI hidden
  └─ Lunas / Pajak (finance; beyond warehouse scope)

[Exception paths]
  ├─ Void via un-Post + VoidReason (incl. Customer Reject)
  └─ ReturJual (may record Driver)
```

## 4.2 Step Detail

| # | Step | DB / Side effect | Classification | Evidence |
| - | ---- | ---------------- | -------------- | -------- |
| 1 | Save Faktur | `BTR_Faktur` / items; Order may get `StatusSync="TERBIT FAKTUR"` | **Implemented** | `SaveFakturWorker.cs` |
| 2 | Deduct inventory | Stok FIFO out via `WarehouseId` | **Implemented** | `GenStokFakturWorker.cs`; `docs/features/faktur/feature.md` |
| 3 | Mark Posted | `BTR_FakturControlStatus` | **Implemented** | CreatePost on SavedFaktur event |
| 4 | Doc print queue | `BTR_Doc` | **Partially Implemented** | Event handler exists; PrintManager not in MainForm menu |
| 5 | Piutang | Receivable tables | **Implemented** | `FakturForm.SavePiutang` |
| 6 | PackingOrder | `BTR_PackingOrder*` | **Implemented** | `FakturForm.SavePackingOrder` ~line 1045 |
| 7 | Print Faktur | RDLC | **Implemented** | `PrintFakturRdlc` |
| 8 | Sync upload | `BTRADE_PackingOrder*` | **Partially Implemented** | Flag + date window + best-effort |
| 9 | Gudang download | `BTRG_PackingOrder*` | **Implemented** | DL1 (form must be open for auto-poll) |
| 10 | Print picking/packing | Excel + PrintLog | **Implemented** | `PK1PrintPackingOrderForm.cs` |
| 11 | Loading / depart / POD | — | **Missing** | — |
| 12 | Faktur Kembali | Control status | **Implemented** (manual) | `FakturControlForm` |
| 13 | Office Packing lists | `BTR_Packing*` | **Implemented** (parallel) | `Packing2Form` |

## 4.3 Inventory Timing (Important Constraint)

- **When:** Immediately on Faktur save (also regen on edit when applicable).
- **Which warehouse:** Exactly `Faktur.WarehouseId` (one Faktur → one Warehouse).
- **Not gated on:** pick, pack, load, or delivery confirmation.
- **Void:** Un-Post rolls stock back.

Evidence: `GenStokFakturWorker` passes `faktur.WarehouseId` into `RemoveFifoStokRequest`; feature.md rule “Faktur creation immediately reduces inventory.”

## 4.4 Status Fields Related to Fulfillment

**On `BTR_Faktur`:** no pick/pack/deliver status columns. Planning fields only:

- `WarehouseId`, `DriverId`, `TglRencanaKirim`

**On `BTR_FakturControlStatus` (`StatusFakturEnum`):**

| Status | Meaning | UI |
| ------ | ------- | --- |
| Posted | Active after create | Visible; uncheck = void path |
| Kirim | Intended “sent” | **Hidden** (`Visible = false`) |
| KembaliFaktur | Signed invoice returned | Visible checkbox |
| Lunas / Pajak | Payment / tax | Finance |

**PackingOrder timestamps:** sync/print markers (`UploadTimestamp`, Depo download timestamps, PrintLogId) — not delivery status.

## 4.5 Relationship Map

```text
Customer ──< Faktur >── Warehouse (logical stock; required; 1:1)
              │
              ├── Driver (planned assignment)
              ├── TglRencanaKirim (planned delivery date)
              │
              └── PackingOrder ──< Item >── Depo
                                    │
                                    └── Depo from Item → Kategori → Supplier.DepoId

Warehouse ≉ Depo
  Warehouse = inventory ownership / stock ledger
  Depo      = physical pick location for Gudang print routing

Customer ↔ Warehouse: none direct (via Faktur only)
Driver ↔ Warehouse: none direct
Driver ↔ Customer: none (via Faktur / PackingOrder list)
```

---

# 5. Existing Database Model

## 5.1 Branch DB (`BTR_*`) — Active Fulfillment Tables

| Table | Purpose | Relationships | Usage | Status |
| ----- | ------- | ------------- | ----- | ------ |
| `BTR_Warehouse` | Warehouse master | Referenced by Faktur, Stok | Master + stock | **Active** |
| `BTR_Depo` | Physical depot master | Via Supplier.DepoId → PackingOrderItem | Print routing | **Active** |
| `BTR_Driver` | Driver master | Faktur, Packing, PackingOrder, ReturJual | Assignment | **Active** |
| `BTR_Faktur` | Invoice (+ Warehouse, Driver, TglRencanaKirim) | Customer, Warehouse, Driver | Core | **Active** |
| `BTR_FakturControlStatus` | Posted/Kirim/Kembali/Lunas/Pajak | Faktur | Lifecycle flags | **Active** (Kirim unused in UI) |
| `BTR_Packing` | Office packing header | Driver; WarehouseId column exists | Packing2 | **Active** |
| `BTR_PackingFaktur` | Packing ↔ Faktur | Packing, Faktur | Packing2 | **Active** |
| `BTR_PackingBrg` | Packing lines | Packing | Packing2 | **Active** |
| `BTR_PackingOrder` | Sync payload header | Faktur, Customer, Driver | Gudang pipeline | **Active** |
| `BTR_PackingOrderItem` | Lines + DepoId | PackingOrder, Depo | Gudang pipeline | **Active** |
| `BTR_PackingOrderDepo` | Per-Depo upload/download tracking | PackingOrder, Depo | Sync | **Active** |
| `BTR_StokBalanceWarehouse` | Qty per Brg×Warehouse | Warehouse, Brg | Inventory | **Active** |
| `BTR_ReturJual` | Return (+ DriverId) | Customer, Driver | Returns | **Active** |

Schema note: `BTR_Packing.WarehouseId` exists in SQL and on `PackingModel`, but `PackingDal` does **not** persist/read it; `Packing2Form` does not set it. Classification: **Partially Implemented** column.

## 5.2 Cloud DB (`BTRADE_*`)

| Table | Purpose | Status |
| ----- | ------- | ------ |
| `BTRADE_PackingOrder` | Cloud hub header | **Active** |
| `BTRADE_PackingOrderItem` | Cloud lines | **Active** |
| `BTRADE_PackingOrderDepo` | DownloadTimestamp / UpdateTimestamp per Depo | **Active** |

## 5.3 Gudang Local DB (`BTRG_*`)

| Table | Purpose | Status |
| ----- | ------- | ------ |
| `BTRG_PackingOrder` | Local packing order (+ PrintLogId) | **Active** |
| `BTRG_PackingOrderItem` | Local lines | **Active** |
| `BTRG_PrintLog` | DocType = PER-SUPPLIER / PER-FAKTUR | **Active** |
| `BTRG_PrintLogPackingOrder` | Print audit link | **Active** |

## 5.4 Missing / Unused Delivery Schema

| Expected concept | Finding |
| ---------------- | ------- |
| `BTR_Delivery` | **Missing** — no table |
| `DeliveryAgg` | **Legacy / Unused** domain stub only |
| Shipment / Manifest / Loading tables | **Missing** |
| Surat Jalan tables | **Missing** |

## 5.5 Dual Packing Models (Constraint)

Two separate aggregates coexist and are **not the same thing**:

| Aggregate | Tables | Purpose |
| --------- | ------ | ------- |
| PackingOrder | `BTR_PackingOrder*` → Cloud → Gudang | Warehouse sync + print |
| Packing | `BTR_Packing*` | Office Excel packing lists by Driver |

They are not automatically synchronized with each other. Faktur.Driver / TglRencanaKirim are also not auto-synced into Packing when Packing2 is built (user re-selects Driver + DeliveryDate).

---

# 6. Existing UI / Reports

## 6.1 Operational UI

| UI | App | Role |
| -- | --- | ---- |
| FakturForm | Desktop | Create Faktur; assign Warehouse/Driver/TglRencanaKirim; create PackingOrder; print Faktur |
| FakturControlForm | Desktop | Posted / Kembali / Lunas / Pajak; Kirim hidden |
| Packing2Form (IT3) | Desktop | Group fakturs; Excel packing lists |
| WarehouseForm (IM2) | Desktop | Warehouse master |
| DriverForm (IM4) | Desktop | Driver master |
| DriverFakturInfoForm (SF8) | Desktop | Faktur-per-driver report |
| SyncForm + Konfigurasi | Sync | Enable/upload PackingOrder |
| DL1 / DL2 / DL3 | Gudang | Download PackingOrder |
| PK1 Print | Gudang | Print picking/packing/faktur list Excel |

## 6.2 Reports / Analytics

| Report | Status | Notes |
| ------ | ------ | ----- |
| Driver Faktur Info (SF8) | **Implemented** | Ops visibility by driver |
| Stock by warehouse | **Implemented** | Inventory, not fulfillment status |
| Portal Location / Warehouse Performance | **Implemented** (analytics) | Not operational fulfillment |
| Warehouse task / loading / in-transit dashboards | **Missing** | — |

## 6.3 Legacy UI

| UI | Status |
| -- | ------ |
| PackingForm | **Legacy / Unused** from menu |
| PrintManagerForm | **Partially / likely legacy** — Doc queue written; form not wired in MainForm |

---

# 7. Existing Printed Documents

| Document | Purpose | Generated by | Business timing | Implementation |
| -------- | ------- | ------------ | --------------- | -------------- |
| **Faktur** (RDLC) | Customer sales invoice; includes DriverName | `FakturForm` / Control reprint | On create / reprint | **Implemented** |
| **Faktur** (GDI `FakturPrintDoc`) | Alternate invoice print | Doc/print path | If PrintManager used | **Partially Implemented** |
| **Packing Order Per-Supplier** (Excel) | Operational picking across selected fakturs | Gudang PK1 `PER-SUPPLIER` | After Gudang download | **Implemented** |
| **Packing Order Per-Faktur** (Excel) | Operational packing per invoice | Gudang PK1 `PER-FAKTUR` | After Gudang download | **Implemented** |
| **Packing Order Faktur List** (Excel) | Checklist of selected fakturs | Gudang PK1 Print Faktur List | After selection | **Implemented** |
| **Packing List Per Faktur / Per Supplier** (Excel) | Office packing lists | `Packing2Form` | When packing batch prepared | **Implemented** |
| **Surat Jalan** | Delivery note | — | — | **Missing** |
| **Delivery Manifest / Loading Sheet** | Loading verification | — | — | **Missing** |
| Formal **Picking List** entity | Named pick document object | — | — | **Missing** (print proxy only) |

Evidence for Gudang DocTypes: `PK1PrintPackingOrderForm.cs` (`PER-SUPPLIER` ~193, `PER-FAKTUR` ~359); prior sync investigation maps these to picking/packing.

---

# 8. Business Process Diagram (As-Is)

```text
Sales / Admin
      │
      ▼
Create Faktur (Warehouse + Driver + TglRencanaKirim)
      │
      ├──────────────────────────────┐
      ▼                              ▼
Inventory − Warehouse          PackingOrder created
Posted status set              (items tagged by Depo)
Print Faktur (office)
      │                              │
      │                              ▼
      │                     Sync → Cloud → Gudang (per Depo)
      │                              │
      │                              ▼
      │                     Print Picking (Per-Supplier)
      │                     Print Packing (Per-Faktur)
      │                              │
      │         ┌────────────────────┘
      │         ▼
      │   Manual pick / pack
      │         │
      │         ▼
      │   Manual loading
      │   (driver may load BOTH warehouses)
      │         │
      │         ▼
      │   Truck depart → deliver → return
      │   (outside system)
      │         │
      ▼         ▼
[Optional] Office Packing2 Excel lists by Driver
      │
      ▼
Faktur Kembali (office checkbox)
      │
      ▼
Finance (Lunas / Pajak) — beyond warehouse scope
```

**Operational timing from business context (not in software):**  
Loading ~16:00 for next-day delivery; trucks leave ~07:00–08:00; deliveries finish ~14:00–15:00; then loading for following day begins.

---

# 9. Reusable Components

These existing assets are concrete starting points for Logistics Management (reuse assessment only — not a design):

| Component | Why reusable | Location |
| --------- | ------------ | -------- |
| Warehouse master | Logical stock ownership already on every Faktur | `WarehouseAgg`, `BTR_Warehouse` |
| Depo master + Supplier→Depo | Physical print routing; multi-Depo per PackingOrder | `BTR_Depo`, `ListBrgDepo`, PackingOrderDepo |
| Driver master + assignment fields | Already used in Faktur / Packing / PackingOrder / reports | `DriverAgg`, `BTR_Driver` |
| `TglRencanaKirim` | Existing planned delivery date on Faktur | `BTR_Faktur` |
| PackingOrder pipeline | End-to-end warehouse document distribution | Desktop + Sync + API + Gudang |
| Gudang print + PrintLog | Existing picking/packing document generation & audit | `PK1PrintPackingOrderForm`, `BTRG_PrintLog*` |
| Packing (`BTR_Packing`) | Closest existing “driver daily group” of fakturs | `Packing2Form` |
| Faktur Control statuses | Posted / Kirim (dormant) / Kembali lifecycle hooks | `StatusFakturEnum`, `FakturControlBuilder` |
| Driver Faktur Info (SF8) | Existing ops visibility by driver | `DriverFakturRpt` |
| Void reason Customer Reject | Existing delivery-reject cancel path | `docs/features/faktur/feature.md` |

---

# 10. Architectural Constraints

Evidence-backed constraints any future Logistics design must respect or explicitly change:

1. **Inventory is already committed at Faktur create**  
   Logistics cannot assume stock reservation-at-pick or deduction-at-delivery without changing a core Faktur rule.

2. **One Faktur → one Warehouse (stock)**  
   Multi-warehouse stock ownership on a single invoice is not supported. Multi-location picking is modeled via **Depo on PackingOrder items**, not via multiple WarehouseIds on Faktur.

3. **Warehouse ≠ Depo**  
   Confusing these concepts will break stock vs print routing.

4. **Two packing aggregates coexist**  
   PackingOrder (Gudang sync) and Packing (office Excel) serve different purposes and are not unified.

5. **Faktur is not synced as an entity to Gudang**  
   Only PackingOrder (embedded Faktur fields) is distributed. Confirmed in sync investigation.

6. **Sync is best-effort / opt-in**  
   Reliability gaps are documented in `docs/investigations/faktur-distribution-sync/investigation.md`. Logistics cannot assume guaranteed warehouse receipt today.

7. **Delivery execution is outside the system**  
   Loading, departure, POD, and failed delivery are manual. Only paper return (`Kembali`) and optional void exist.

8. **Kirim status is dormant in UI**  
   Domain support exists but is not operationally used via Faktur Control grid.

9. **Sales route ≠ delivery route**  
   Existing route/visit concepts belong to sales field activity, not truck delivery planning.

10. **Mature behavior should be treated as intentional**  
    Immediate stock reduction, dual packing paths, and Depo-based print routing are established operational patterns.

---

# 11. Gap Analysis

Gaps vs a modern Logistics Management module (Warehouse Operations + Delivery Operations).  
No solution design — gaps only, with evidence.

| Gap | Classification | Evidence |
| --- | -------------- | -------- |
| No shipment / delivery trip concept | **Missing** | No tables/UI; `DeliveryAgg` stub unused |
| No loading group / loading verification | **Missing** | No loading screens/tables; ops described as manual |
| No truck / departure schedule | **Missing** | Operational times not encoded |
| No Surat Jalan | **Missing** | Zero fulfillment matches |
| No delivery status / POD / history | **Missing** | No status fields beyond Control (Kirim hidden) |
| No failed delivery workflow | **Missing** | Only void reason Customer Reject |
| No warehouse task management (pick/pack confirm) | **Missing** | Gudang = download + print + PrintLog only |
| No dedicated Picking entity | **Missing** | Product picking = PER-SUPPLIER Excel |
| No operational visibility (in-transit / loaded / delivered) | **Missing** | SF8 lists fakturs; no trip status |
| No multi-warehouse loading plan for one driver | **Missing as system concept** | Operationally done; Packing/PackingOrder do not model cross-warehouse load verification |
| PackingOrder sync reliability gaps | **Partially Implemented** | Prior sync investigation |
| Planned date vs Packing DeliveryDate not auto-aligned | **Partially Implemented** | Separate fields; Packing2 search by Faktur date |
| `BTR_Packing.WarehouseId` not persisted | **Partially Implemented** | Column/model exist; DAL omits |
| Status Kirim unused in UI | **Partially Implemented** | Domain + hidden column |
| No Logistics feature artifact | **Missing** | No `docs/features/warehouse*` or delivery feature folder |

---

# 12. Recommendations for Reuse (Starting Point Only)

These recommendations identify **what can be reused as a starting point**. They are not an architecture or implementation plan.

1. **Treat PackingOrder + Gudang print as the existing Warehouse Operations core**  
   Any Logistics Warehouse track should start from this pipeline rather than inventing a parallel print path.

2. **Treat Driver + `TglRencanaKirim` + Packing (Driver+DeliveryDate) as the existing Delivery Planning seeds**  
   They are assignment/planning attributes, not execution. Delivery Operations would extend beyond them.

3. **Preserve Warehouse vs Depo semantics**  
   Stock ownership (Warehouse) and physical pick routing (Depo) must remain distinct unless a deliberate domain change is approved.

4. **Do not assume DeliveryAgg is a foundation**  
   It is an unused stub. Prefer building from real active tables (`Faktur`, `Packing`, `PackingOrder`, `Driver`).

5. **Decide explicitly how Packing vs PackingOrder should relate**  
   Before adding Logistics concepts, clarify whether office Packing remains a parallel tool or becomes subordinate to a single operational grouping model.

6. **Account for immediate inventory deduction**  
   Logistics status models (picked/packed/delivered) must not silently redefine when stock leaves the ledger.

7. **Re-evaluate dormant Kirim status before inventing a new “sent” flag**  
   Domain already has Kirim; UI currently hides it. Reuse vs replace should be an explicit decision later.

8. **Use the sync investigation as a prerequisite reliability baseline**  
   Warehouse document distribution quality affects any Logistics module that depends on Gudang receiving work.

9. **Next approved step (outside this investigation)**  
   After stakeholder review of this baseline: Analyst feature scope for Logistics Management → Architect implementation plan.  
   **Do not implement from this document alone.**

---

# Appendix A — Key Evidence Index

```text
Foundation
  docs/foundation/PRODUCT.md
  docs/foundation/DOMAIN.md
  docs/foundation/LANDSCAPE.md
  docs/foundation/WORKFLOW.md
  docs/features/faktur/feature.md
  docs/investigations/faktur-distribution-sync/investigation.md

Desktop — masters & fulfillment
  src/j05-btr-distrib/btr.distrib/InventoryContext/WarehouseAgg/WarehouseForm.cs
  src/j05-btr-distrib/btr.distrib/InventoryContext/DriverAgg/DriverForm.cs
  src/j05-btr-distrib/btr.distrib/InventoryContext/PackingAgg/Packing2Form.cs
  src/j05-btr-distrib/btr.distrib/SalesContext/FakturAgg/FakturForm.cs
  src/j05-btr-distrib/btr.distrib/SalesContext/FakturControlAgg/FakturControlForm.cs
  src/j05-btr-distrib/btr.application/InventoryContext/StokAgg/GenStokUseCase/GenStokFakturWorker.cs
  src/j05-btr-distrib/btr.domain/InventoryContext/PackingOrderFeature/PackingOrderModel.cs
  src/j05-btr-distrib/btr.domain/InventoryContext/DeliveryAgg/DeliveryModel.cs

Schema
  src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_Warehouse.sql
  src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_Depo.sql
  src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_Driver.sql
  src/j05-btr-distrib/btr.sql/Tables/InventoryContext/BTR_Packing.sql
  src/j05-btr-distrib/btr.sql/PackingOrderModel/BTR_PackingOrder.sql
  src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_Faktur.sql

Gudang
  src/j07-btr-gudang/BtrGudang.Winform/Forms/PK1PrintPackingOrderForm.cs
  src/j07-btr-gudang/BtrGudang.Winform/Forms/DL1DownloaderForm.cs

Sync / API
  src/j07-btrade-sync (PackingOrderUploadSvc)
  src/j06-pkl-btrade-api (PackingOrderController)
```

---

# Appendix B — Classification Summary

| Area | Implemented | Partial | Legacy | Missing |
| ---- | ----------- | ------- | ------ | ------- |
| Warehouse master / stock by warehouse | ✓ | | | |
| Depo + multi-Depo PackingOrder | ✓ (DAL) | Depo-only UI | | Depo master UI |
| Driver master + assignment | ✓ | | | Driver scheduling/trips |
| PackingOrder → Gudang print | ✓ | Sync reliability | | Task confirmation |
| Office Packing2 Excel | ✓ | WarehouseId on Packing | PackingForm v1 | |
| Planned delivery date | ✓ | Not tied to execution | | |
| Faktur Kembali | ✓ | | | |
| Status Kirim | | ✓ (hidden UI) | | |
| DeliveryAgg | | | ✓ stub | Real delivery module |
| Surat Jalan / Manifest / Loading / POD | | | | ✓ |
| Shipment / Truck Schedule / Delivery Route | | | | ✓ |

---

**Document status:** Complete investigation baseline for Logistics Management design.  
**Next gate:** User review → Analyst feature specification → Architect plan (no implementation until approved).
