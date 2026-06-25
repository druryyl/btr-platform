# Historical Backfill Source Investigation

**Status:** Schema investigation complete — evidence-based analysis only  
**Date:** 2026-06-25  
**Scope:** Transactional tables listed for Entity Analytics historical replay  
**Source of truth:** `src/j05-btr-distrib/btr.sql/Tables/` DDL files; corroborated by DAL SQL where DDL is incomplete  
**Aliases used in task brief:** `BTR_Barang` = `BTR_Brg`; `BTR_Warehous` = `BTR_Warehouse`

---

## Executive Summary

This investigation examines whether the **specified transactional tables** contain sufficient historical information to support Entity Analytics monthly snapshot reconstruction via transaction replay.

### Findings by entity domain (from scoped tables only)

| Entity domain | Monthly history reconstructable from scoped tables? | Verdict |
|---------------|---------------------------------------------------|---------|
| **Supplier** | **Yes — partially to strongly** | Purchase spend, invoice counts, item-level purchase mix, and purchase returns are preserved in dated, append-style transaction headers and line items (`BTR_Invoice`, `BTR_InvoiceItem`, `BTR_ReturBeli`, `BTR_ReturBeliItem`). |
| **Item** | **Partial only** | Inbound purchase and purchase-return quantities/values per SKU per month are reconstructable. Sales-side movement, consumption, and inventory position are **not** reconstructable from the scoped tables alone. |
| **Customer** | **No — for core sales KPIs** | Scoped tables lack sales invoice headers and line items (`BTR_Faktur`, `BTR_FakturItem`). Only sales **returns** (`BTR_ReturJual*`) and master attributes are available. |
| **Salesman** | **No — for core sales KPIs** | Same gap as Customer for billed omzet. Field-activity history (`BTR_VisitPlan`, `BTR_CheckIn`) is partially available but does not substitute for sales performance history. |
| **Inventory position** | **No** | Current balance tables and stock movement ledger are **outside** the scoped table list. |
| **Receivable position** | **No** | Receivable tables and payment history are **outside** the scoped table list. `BTR_Customer.CreditBalance` is current-state only. |

### Cross-cutting schema characteristics

1. **Transaction headers** (`BTR_Invoice`, `BTR_ReturBeli`, `BTR_ReturJual`) preserve business dates, entity keys, amounts, audit timestamps, and soft-void markers (`VoidDate = '3000-01-01'` = active). These are suitable for period-filtered replay.
2. **Master tables** (`BTR_Brg`, `BTR_Customer`, `BTR_SalesPerson`, `BTR_Supplier`, `BTR_Kategori`, `BTR_Wilayah`, `BTR_Warehouse`) store **current state only**. They do not version historical attribute changes (region assignment, principal link, HPP, plafond, etc.).
3. **DDL vs runtime drift:** Application DAL references columns on `BTR_Invoice` (`IsStokPosted`, `Note`) that are **not present** in the checked-in `BTR_Invoice.sql` DDL. Posting status (PU-KPI-003) depends on `IsStokPosted`; this must be verified against the deployed database schema.
4. **The scoped table set is insufficient alone** for full Entity Analytics backfill across Customer, Salesman, Item inventory KPIs, and receivable KPIs. Additional transactional tables are required (at minimum `BTR_Faktur` / `BTR_FakturItem` for sales; `BTR_Piutang*` for receivables; `BTR_StokMutasi` / `BTR_StokBalanceWarehouse` for inventory).

---

## Table Analysis

### BTR_Brg (alias: BTR_Barang / ITEM_MASTER)

#### 1. Business Purpose

Represents the **item (SKU) master** — product catalog identity and current classification/cost attributes.

- **When created:** When a new product is registered in the catalog.
- **Who creates:** Purchasing / master-data users via Desktop or admin workflows.
- **Historical vs current:** **Current state only.** The row is updated in place when name, supplier link, category, HPP, or active flag changes.

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `BrgId` (VARCHAR(6)) |
| Natural key | `BrgCode` (VARCHAR(20)) — indexed `(BrgCode, BrgId)` but not declared UNIQUE in DDL |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `BrgName` | Display name of the item |
| `BrgCode` | Business code used in reports and Entity Analytics |
| `IsAktif` | Whether the SKU is currently active in the catalog |
| `SupplierId` | Current principal/supplier assignment for the item |
| `KategoriId` | Current product category |
| `JenisBrgId` | Product type classification |
| `Hpp` | Current cost of goods (HPP) |
| `HppTimestamp` | Last time `Hpp` was updated — **not a full cost history** |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Supplier` | `SupplierId` | Principal that owns/supplies this SKU today |
| `BTR_Kategori` | `KategoriId` | Category used for item peer grouping (radar) |
| `BTR_InvoiceItem` | `BrgId` | Purchase line items referencing this SKU |
| `BTR_ReturBeliItem` | `BrgId` | Purchase return line items |
| `BTR_ReturJualItem` | `BrgId` | Sales return line items |

No formal FK constraints in DDL; relationships are logical/application-enforced.

#### 5. Historical Characteristics

**Mutable master data — current state only.**

`HppTimestamp` records when the current HPP was last set, but prior HPP values are overwritten. `SupplierId` and `KategoriId` changes are not versioned in this table.

#### 6. Suitability for Historical Replay

**Strongly Recommended** — not as a transaction source, but as the **entity registry and dimension resolver** (BrgId → BrgCode, category, supplier) when replaying line-item transactions.

#### 7. Limitations

- Cannot determine which supplier or category an item belonged to in a prior month if reassigned since then.
- Cannot determine historical HPP for valuing past inventory; only current `Hpp` is stored.
- `IsAktif` reflects current catalog status, not whether the item was active in a past period.

#### 8. Historical Replay Capability

From this table alone:

- Resolve item identity (`BrgId`, `BrgCode`, `BrgName`) at replay time
- Attach **current** category and supplier dimensions to historical transactions
- Value inventory at **current** HPP only (not historically accurate cost)

Cannot reconstruct: item sales omzet, consumption, stock position, or movement class without other tables.

---

### BTR_Customer (alias: CUSTOMER_MASTER)

#### 1. Business Purpose

Represents the **customer master** — trading partner identity, geographic classification, credit terms, and current balance snapshot fields.

- **When created:** Customer onboarding.
- **Who creates:** Sales / admin users.
- **Historical vs current:** **Current state only** for most attributes. Coordinate changes may be tracked separately in `BTR_CustomerLocHist` (outside scoped list).

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `CustomerId` (VARCHAR(6)) |
| Natural key | `CustomerCode` (VARCHAR(10)) — not declared UNIQUE in DDL |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `CustomerName` / `CustomerCode` | Identity |
| `WilayahId` | Current sales region assignment |
| `KlasifikasiId` | Customer classification segment |
| `HargaTypeId` | Price list type |
| `Plafond` | Credit limit (current configured value) |
| `CreditBalance` | Current credit balance snapshot on master |
| `IsSuspend` | Whether customer is currently suspended |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Wilayah` | `WilayahId` | Geographic territory for peer grouping |
| `BTR_ReturJual` | `CustomerId` | Sales return documents for this customer |
| `BTR_VisitPlan` | `CustomerId` | Planned field visits |
| `BTR_CheckIn` | `CustomerId` (denormalized; VARCHAR(20) in CheckIn) | Actual field visits |

No `SalesPersonId` on customer master — salesman assignment is carried on sales documents (not in scoped tables).

#### 5. Historical Characteristics

**Mutable master data — current state only.**

`WilayahId`, `Plafond`, `KlasifikasiId`, and `CreditBalance` are overwritten on update. No effective-dating or change log in this table.

#### 6. Suitability for Historical Replay

**Optional** — required for entity resolution and **current** dimension attachment, but **not** a transaction history source for customer KPIs.

#### 7. Limitations

- `CreditBalance` is a point-in-time field on master, not a month-end receivable ledger.
- Cannot determine historical wilayah or plafond at month-end.
- No sales transaction history in scoped tables to compute customer omzet.

#### 8. Historical Replay Capability

- Customer identity resolution for `BTR_ReturJual`, `BTR_VisitPlan`, `BTR_CheckIn`
- Attach **current** wilayah for radar peer grouping (historically approximate)
- Sales return totals per customer per month (via `BTR_ReturJual`, not this table)

Cannot reconstruct: customer monthly omzet, open balance trends, overdue exposure, faktur count, product mix.

---

### BTR_SalesPerson (alias: SALESPERSON_MASTER)

#### 1. Business Purpose

Represents the **sales representative master** — identity and current territory/segment assignment.

- **When created:** HR / sales admin registers a salesman.
- **Who creates:** Admin users.
- **Historical vs current:** **Current state only.**

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `SalesPersonId` (VARCHAR(5)) |
| Natural key | `SalesPersonCode` (VARCHAR(10)) |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `SalesPersonName` | Display name |
| `SalesPersonCode` | Business code for reports and Entity Analytics |
| `WilayahId` | Current territory assignment |
| `SegmentId` | Sales segment |
| `Email` | Bridge to mobile field activity (`BTR_CheckIn.UserEmail`) |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Wilayah` | `WilayahId` | Territory |
| `BTR_VisitPlan` | `SalesPersonId` | Planned visits |
| `BTR_ReturJual` | `SalesPersonId` | Sales returns attributed to rep |
| `BTR_CheckIn` | `Email` ↔ `UserEmail` (indirect) | Field visit execution |

#### 5. Historical Characteristics

**Mutable master data — current state only.**

Territory and segment changes are not versioned.

#### 6. Suitability for Historical Replay

**Optional** — entity registry and dimension resolver. Essential for joining visit/return data to salesman identity; **not** sufficient for omzet or achievement KPIs without sales invoice tables.

#### 7. Limitations

- No historical territory assignment.
- No monthly target data in scoped tables (`BTR_SalesOmzetTarget` is outside scope).
- `BTR_CheckIn` links via `UserEmail`, not `SalesPersonId` — join reliability depends on email population.

#### 8. Historical Replay Capability

- Salesman identity resolution
- Sales return totals per salesman per month (via `BTR_ReturJual`)
- Visit plan compliance metrics per month (via `BTR_VisitPlan` + `BTR_CheckIn`, with email bridge caveat)

Cannot reconstruct: billed omzet, achievement %, open balance, piutang per rep.

---

### BTR_VisitPlan (alias: SALESPERSON_VISIT_PLAN)

#### 1. Business Purpose

Represents the **materialized daily visit plan** for a salesman — which customers are scheduled to be visited on a given date.

- **When created:** When visit plans are generated/materialized (nightly or on-demand).
- **Who creates:** System materialization from route templates (`PlanSource`, `HariRuteId`).
- **Historical vs current:** **Append-style by plan date.** Each `(SalesPersonId, VisitDate, CustomerId)` is unique; past plans are retained.

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `VisitPlanId` (VARCHAR(26)) |
| Natural key | `(SalesPersonId, VisitDate, CustomerId)` — enforced by `UX_BTR_VisitPlan` |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `SalesPersonId` | Assigned salesman |
| `VisitDate` | Calendar date of planned visit |
| `CustomerId` | Customer to visit |
| `NoUrut` | Visit sequence order |
| `HariRuteId` | Route day template reference |
| `PlanSource` | How the plan was produced (e.g. `Template`) |
| `MaterializedAt` | When this plan row was written |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_SalesPerson` | `SalesPersonId` | Rep identity |
| `BTR_Customer` | `CustomerId` | Customer identity |
| `BTR_CheckIn` | `CustomerId` + date proximity (no direct FK) | Planned vs actual visit comparison |

#### 5. Historical Characteristics

**Append-only plan history by visit date.**

Rows are not voided in schema; historical plans persist. If plans are regenerated for the same key, the unique constraint prevents duplicates — behavior on re-materialization is upsert/replace at application level (not visible in DDL).

#### 6. Suitability for Historical Replay

**Optional** — supports field-activity and route-compliance analytics for Salesman entity, not core financial KPIs.

#### 7. Limitations

- Does not record whether a visit actually occurred (requires `BTR_CheckIn`).
- No order or sales outcome data.
- Coverage begins when visit-plan materialization was deployed; no retroactive plans before feature existed.

#### 8. Historical Replay Capability

- Planned visit count per salesman per month
- Planned customer coverage per salesman per month
- Route compliance denominator (planned visits) when paired with `BTR_CheckIn`

Cannot reconstruct: sales omzet, customer purchase behavior, visit-to-order conversion.

---

### BTR_CheckIn (alias: SALESPERSON_VISIT_REAL)

#### 1. Business Purpose

Represents **actual field visit check-in/check-out events** captured from mobile — proof of customer visit with GPS coordinates and timestamps.

- **When created:** When a salesman checks in at a customer location via mobile app.
- **Who creates:** Field user (`UserEmail`).
- **Historical vs current:** **Append-only event log.** Events are retained with `CheckInDate` / `CheckInTime`.

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `CheckInId` (VARCHAR(26)) |
| Natural key | None declared |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `CheckInDate` / `CheckInTime` | Visit start (date stored as VARCHAR(10)) |
| `CheckOutTime` | Visit end |
| `UserEmail` | Mobile user — indirect link to salesman |
| `CustomerId` / `CustomerCode` / `CustomerName` | Denormalized customer snapshot at check-in time |
| `CheckInLatitude` / `CheckInLongitude` | GPS at arrival |
| `StatusSync` | Mobile sync status |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_SalesPerson` | `UserEmail` = `Email` | Rep identity (indirect) |
| `BTR_Customer` | `CustomerId` | Customer visited |
| `BTR_VisitPlan` | `CustomerId` + `VisitDate` ≈ `CheckInDate` | Plan vs actual |

#### 5. Historical Characteristics

**Append-only transaction/event history.**

Customer name/address are denormalized at event time — partially preserves customer label even if master changes later. `CustomerId` width (VARCHAR(20)) differs from `BTR_Customer.CustomerId` (VARCHAR(6)) — potential data-quality risk.

#### 6. Suitability for Historical Replay

**Optional** — field-activity layer for Salesman analytics; not used for financial Entity Analytics KPIs in current architecture.

#### 7. Limitations

- No `SalesPersonId` — requires email bridge to `BTR_SalesPerson`.
- No sales outcome (order, faktur) linked in schema.
- `CheckInDate` as VARCHAR may complicate date-range filtering vs native DATE types.
- Indexes on `CheckInDate` / `UserEmail` are commented out in DDL — performance risk for large backfills.

#### 8. Historical Replay Capability

- Actual visit count per user/customer per month
- Visit duration (check-in to check-out) where both times populated
- Geographic visit evidence

Cannot reconstruct: billed omzet, visit-to-sale conversion, customer financial KPIs.

---

### BTR_Supplier (alias: SUPPLIER_MASTER)

#### 1. Business Purpose

Represents the **supplier / principal master** — vendor identity and contact attributes.

- **When created:** Supplier onboarding.
- **Who creates:** Purchasing / admin users.
- **Historical vs current:** **Current state only.**

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `SupplierId` (VARCHAR(5)) |
| Natural key | `SupplierCode` (VARCHAR(10)) |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `SupplierName` / `SupplierCode` | Principal identity |
| `DepoId` | Depot association |
| `Keyword` | Search keyword |
| Contact / tax fields | Master reference data |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Invoice` | `SupplierId` | Purchase invoices |
| `BTR_ReturBeli` | `SupplierId` | Purchase returns |
| `BTR_Brg` | `SupplierId` | Items currently linked to principal |
| `BTR_Kategori` | `SupplierId` | Categories owned by principal |

#### 5. Historical Characteristics

**Mutable master data — current state only.**

#### 6. Suitability for Historical Replay

**Strongly Recommended** — entity registry for Supplier Entity Analytics; pairs with purchase transaction tables.

#### 7. Limitations

- Name changes are not versioned — historical reports use current `SupplierName`.
- No spend or performance data on master itself.

#### 8. Historical Replay Capability

- Supplier identity resolution for purchase transactions
- Principal catalog scope via `BTR_Kategori` and `BTR_Brg.SupplierId`

Purchase KPIs require `BTR_Invoice*` / `BTR_ReturBeli*`, not this table alone.

---

### BTR_Invoice (alias: PURCHASE_HEADER)

#### 1. Business Purpose

Represents a **purchase invoice (faktur beli) header** — inbound procurement document from a supplier into a warehouse.

- **When created:** When a purchase invoice is entered/posted in the purchasing workflow.
- **Who creates:** Purchasing user (`UserId`).
- **Historical vs current:** **Append-style transaction with soft void.** Active rows have `VoidDate = '3000-01-01'`. Voided invoices remain in table with actual void date.

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `InvoiceId` (VARCHAR(13)) |
| Natural key | `InvoiceCode` (VARCHAR(20)) — business document number |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `InvoiceDate` | Business date of purchase — **primary period key for replay** |
| `InvoiceCode` | Document number |
| `SupplierId` | Principal |
| `WarehouseId` | Receiving warehouse |
| `GrandTotal` / `Total` / `Disc` / `Tax` / `Dpp` | Financial amounts |
| `DueDate` | Payment due date |
| `CreateTime` / `LastUpdate` | Audit timestamps |
| `VoidDate` / `UserIdVoid` | Soft-delete / void marker |
| `UserId` | Creating user |
| `IsStokPosted` | **Used by `InvoiceViewDal` for stock posting status** — present in application model/DAL but **absent from checked-in DDL** |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Supplier` | `SupplierId` | Principal |
| `BTR_Warehouse` | `WarehouseId` | Receiving location |
| `BTR_InvoiceItem` | `InvoiceId` | Line items |
| `BTR_StokMutasi` | `ReffId` = `InvoiceId` (outside scoped list) | Stock impact when posted |

#### 5. Historical Characteristics

**Append-only transaction history with in-place void flag and posting-state mutation.**

The invoice row persists; void sets `VoidDate`. `IsStokPosted` (if present in deployed DB) mutates when stock is posted — **posting state at historical month-end is not versioned**, only current flag value is stored.

#### 6. Suitability for Historical Replay

**Essential** for Supplier Entity Analytics L1 (MTD purchase, invoice count) and Item inbound movement.

#### 7. Limitations

- `IsStokPosted` current value does not preserve whether invoice was posted **as of** a past month-end (affects PU-KPI-003 historical accuracy).
- DDL file incomplete vs runtime schema — verify `IsStokPosted`, `Note` columns in production.
- No line-level detail on header — requires `BTR_InvoiceItem`.

#### 8. Historical Replay Capability

- Supplier monthly purchase amount (`GrandTotal` sum by `SupplierId` × month, excluding voided)
- Supplier monthly invoice count
- Company-level purchase totals for share calculations
- Posting status distribution (with current-state caveat on `IsStokPosted`)

---

### BTR_InvoiceItem (alias: PURCHASE_ITEM)

#### 1. Business Purpose

Represents **line items on a purchase invoice** — SKU, quantities, unit costs, and line totals.

- **When created:** With parent `BTR_Invoice`.
- **Who creates:** Same purchasing user workflow.
- **Historical vs current:** **Append-style;** tied to parent invoice lifecycle (void on header excludes from active queries).

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `InvoiceItemId` (VARCHAR(17)) |
| Parent link | `InvoiceId` |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `InvoiceId` | Parent purchase header |
| `BrgId` | Item purchased |
| `QtyBeli` | Purchase quantity (base unit) |
| `HppSat` / `HppSatBesar` / `HppSatKecil` | Unit cost |
| `SubTotal` / `Total` | Line financial amounts |
| `QtyBonus` / `QtyPotStok` | Bonus / stock-deduction quantities |
| Unit structure fields | `QtyBesar`, `QtyKecil`, `Conversion`, satuan fields |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Invoice` | `InvoiceId` | Parent document and business date |
| `BTR_Brg` | `BrgId` | Item master |
| `BTR_Supplier` | via `BTR_Invoice.SupplierId` | Principal |
| `BTR_Kategori` | via `BTR_Brg.KategoriId` | Category |

#### 5. Historical Characteristics

**Append-only line history** (inactive when parent voided).

#### 6. Suitability for Historical Replay

**Essential** for item-level purchase mix, supplier SKU analytics, and inbound quantity reconstruction.

#### 7. Limitations

- No `SupplierId` on line — must join through header.
- Historical category/supplier for item comes from current `BTR_Brg`, not line-time snapshot.

#### 8. Historical Replay Capability

- Item monthly purchase quantity and value
- Supplier × item purchase mix per month
- Active SKU count per supplier (items with purchase activity in period)

---

### BTR_ReturBeli (alias: PURCHASE_RETURN_HEADER)

#### 1. Business Purpose

Represents a **purchase return header** — goods returned to a supplier from a warehouse.

- **When created:** Purchase return document entry.
- **Who creates:** Purchasing user (`UserId`).
- **Historical vs current:** **Append-style with soft void** (`VoidDate`).

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `ReturBeliId` (VARCHAR(13)) |
| Natural key | `ReturBeliCode` |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `ReturBeliDate` | Business date — period key |
| `SupplierId` | Principal |
| `WarehouseId` | Source warehouse |
| `GrandTotal` / financial columns | Return value |
| `VoidDate` | Soft void marker |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Supplier` | `SupplierId` | Principal |
| `BTR_Warehouse` | `WarehouseId` | Warehouse |
| `BTR_ReturBeliItem` | `ReturBeliId` | Line items |

No `InvoiceId` in DDL — **cannot link return to original purchase invoice** from schema alone.

#### 5. Historical Characteristics

**Append-only transaction history with void flag.**

#### 6. Suitability for Historical Replay

**Strongly Recommended** — net purchase calculations should subtract returns for supplier and item KPIs.

#### 7. Limitations

- No reference to originating `BTR_Invoice` in schema.
- Return reason limited to `Note` on header.

#### 8. Historical Replay Capability

- Supplier monthly purchase return amount
- Adjustment to net purchase spend per supplier per month

---

### BTR_ReturBeliItem (alias: PURCHASE_RETURN_ITEM)

#### 1. Business Purpose

**Line items on purchase returns** — SKU, quantities, and amounts returned to supplier.

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `ReturBeliItemId` (VARCHAR(17)) |
| Parent link | `ReturBeliId` |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `BrgId` | Item returned |
| `QtyBeli` | Return quantity |
| `HppSat` / `SubTotal` / `Total` | Cost and amounts |
| `StokStr` | Stock detail string at time of entry |

#### 4. Relationships

Same pattern as `BTR_InvoiceItem` — via `ReturBeliId` to header, `BrgId` to item master.

#### 5. Historical Characteristics

**Append-only line history** (excluded when parent voided).

#### 6. Suitability for Historical Replay

**Strongly Recommended** for net item inbound and supplier return mix.

#### 7. Limitations

- Same master-data staleness as purchase lines.
- No invoice linkage.

#### 8. Historical Replay Capability

- Item monthly purchase return quantity and value
- Supplier × item return mix per month

---

### BTR_ReturJual (alias: SALES_RETURN_HEADER)

#### 1. Business Purpose

Represents a **sales return header** — goods returned by a customer, attributed to a salesman and warehouse.

- **When created:** Sales return document entry.
- **Who creates:** User (`UserId`).
- **Historical vs current:** **Append-style with soft void.**

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `ReturJualId` (VARCHAR(13)) |
| Natural key | `ReturJualCode` |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `ReturJualDate` | Business date — period key |
| `CustomerId` | Customer returning goods |
| `SalesPersonId` | Attributed salesman |
| `WarehouseId` | Receiving warehouse |
| `JenisRetur` | Return type |
| `GrandTotal` / financial columns | Return value |
| `VoidDate` | Soft void |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Customer` | `CustomerId` | Customer |
| `BTR_SalesPerson` | `SalesPersonId` | Salesman |
| `BTR_Warehouse` | `WarehouseId` | Warehouse |
| `BTR_ReturJualItem` | `ReturJualId` | Line items |

No `FakturId` in DDL — **cannot link to original sales invoice** from schema alone.

#### 5. Historical Characteristics

**Append-only transaction history with void flag.**

This is the **only sales-side transaction header** in the scoped table list.

#### 6. Suitability for Historical Replay

**Optional** — provides partial sales-side history (returns only). **Not sufficient** for customer/salesman omzet KPIs.

#### 7. Limitations

- Returns are a subset of sales activity; gross sales require `BTR_Faktur`.
- No link to original faktur limits return-rate and mix analysis.

#### 8. Historical Replay Capability

- Customer monthly sales return amount
- Salesman monthly sales return amount
- Return document count per entity per month

Cannot reconstruct: gross sales omzet, net sales, or positive sales mix.

---

### BTR_ReturJualItem (alias: SALES_RETURN_ITEM)

#### 1. Business Purpose

**Line items on sales returns** — SKU, quantities, and amounts.

#### 2. Primary Key

| Key type | Column(s) |
|----------|-----------|
| Primary key | `ReturJualItemId` (VARCHAR(16)) |
| Parent link | `ReturJualId` |

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `BrgId` / `BrgCode` | Item (code denormalized on line) |
| `Qty` / `SubQty` | Returned quantities |
| `HrgSat` / `SubTotal` / `Total` | Pricing and amounts |

#### 4. Relationships

Via `ReturJualId` to header (customer, salesman, date); `BrgId` to item master.

#### 5. Historical Characteristics

**Append-only line history.**

`BrgCode` on line provides partial snapshot of item code at return time.

#### 6. Suitability for Historical Replay

**Optional** — item-level return analytics only.

#### 7. Limitations

- No sales-outbound lines in scoped tables to compute return rate.
- Supplemental detail may exist in `BTR_ReturJualItemQtyHrg` (outside scoped list).

#### 8. Historical Replay Capability

- Item monthly sales return quantity and value
- Customer × item return mix per month (via header join)

---

### BTR_Warehouse (alias: WAREHOUSE_MASTER)

#### 1. Business Purpose

Represents **warehouse / location master** — storage facility identity and flags.

- **When created:** Warehouse setup.
- **Who creates:** Admin.
- **Historical vs current:** **Current state only** (`IsAktif`, `IsSpecial`).

#### 2. Primary Key

`WarehouseId` (VARCHAR(5))

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `WarehouseName` | Display name |
| `IsAktif` | Active flag |
| `IsSpecial` | Special warehouse indicator |

#### 4. Relationships

Referenced by `BTR_Invoice`, `BTR_ReturBeli`, `BTR_ReturJual` for document warehouse context.

#### 5. Historical Characteristics

**Mutable master — current state only.**

#### 6. Suitability for Historical Replay

**Optional** — dimension resolver for warehouse name on historical documents.

#### 7. Limitations

- No stock balance data in this table.
- `BTR_ReturJual.WarehouseId` is VARCHAR(3) vs VARCHAR(5) on master — potential inconsistency.

#### 8. Historical Replay Capability

- Warehouse name resolution on purchase and return documents only.

---

### BTR_Kategori (alias: CATEGORY_MASTER)

#### 1. Business Purpose

Represents **product category master**, linked to a principal (supplier).

- **When created:** Category setup under a supplier.
- **Who creates:** Admin / purchasing.
- **Historical vs current:** **Current state only.**

#### 2. Primary Key

`KategoriId` (VARCHAR(5)); business code in `Code` (VARCHAR(15))

#### 3. Important Columns

| Column | Meaning |
|--------|---------|
| `KategoriName` | Category display name |
| `SupplierId` | Owning principal |
| `Code` | Category business code |

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Brg` | `KategoriId` | Items in category |
| `BTR_Supplier` | `SupplierId` | Principal owner |

#### 5. Historical Characteristics

**Mutable master — current state only.**

#### 6. Suitability for Historical Replay

**Optional** — Item radar peer grouping and supplier catalog structure at replay time.

#### 7. Limitations

- Item category changes on `BTR_Brg` are not historized — peer groups for past months may be wrong.

#### 8. Historical Replay Capability

- Category name resolution
- Supplier catalog penetration denominators (categories per supplier)

---

### BTR_Wilayah (alias: REGION_MASTER)

#### 1. Business Purpose

Represents **sales region / territory master**.

- **When created:** Territory setup.
- **Who creates:** Admin.
- **Historical vs current:** **Current state only.**

#### 2. Primary Key

`WilayahId` (VARCHAR(3))

#### 3. Important Columns

`WilayahName` — region display name

#### 4. Relationships

| Related table | Join | Business meaning |
|---------------|------|------------------|
| `BTR_Customer` | `WilayahId` | Customer territory |
| `BTR_SalesPerson` | `WilayahId` | Rep territory |

#### 5. Historical Characteristics

**Mutable master — current state only.**

#### 6. Suitability for Historical Replay

**Optional** — Customer radar peer grouping (wilayah axis).

#### 7. Limitations

- Historical customer or salesman territory assignment not preserved.

#### 8. Historical Replay Capability

- Region name resolution for entity dimensions at replay time.

---

## Cross-table Assessment

### Customer

**Can Customer monthly history be reconstructed from the scoped tables?**

**No — not for core Entity Analytics customer KPIs.**

| KPI class | Scoped-table support | Required tables |
|-----------|---------------------|-----------------|
| Monthly omzet (CU-KPI-009) | **Not supported** | `BTR_Faktur`, `BTR_FakturItem` (outside scope) |
| Open balance / overdue (CU-KPI-010, FI-KPI-013) | **Not supported** | `BTR_Piutang`, `BTR_PiutangElement`, payment tables (outside scope) |
| Sales returns | **Partial** | `BTR_ReturJual`, `BTR_ReturJualItem` |
| Field visits | **Partial** | `BTR_VisitPlan`, `BTR_CheckIn` + `BTR_Customer` |
| Wilayah peer grouping | **Approximate** | `BTR_Customer` + `BTR_Wilayah` (current assignment only) |

**What scoped tables do provide:** customer identity, current wilayah/plafond, monthly sales **return** totals, visit plan/actual counts.

---

### Salesman

**Can Salesman monthly history be reconstructed?**

**No — not for core sales performance KPIs.**

| KPI class | Scoped-table support | Required tables |
|-----------|---------------------|-----------------|
| MTD omzet (SF-KPI-008) | **Not supported** | `BTR_Faktur` (outside scope) |
| Achievement % (SF-KPI-009) | **Not supported** | `BTR_Faktur` + `BTR_SalesOmzetTarget` (target outside scope) |
| Open balance (SF-KPI-010) | **Not supported** | `BTR_Piutang` allocated by salesman (outside scope) |
| Sales returns | **Partial** | `BTR_ReturJual` |
| Visit plan / actual | **Partial** | `BTR_VisitPlan`, `BTR_CheckIn`, `BTR_SalesPerson` |

**Limitations:**

- `BTR_CheckIn` has no `SalesPersonId`; email bridge may be incomplete.
- Visit metrics coverage limited to post-deployment of field-activity features.
- No historical territory on `BTR_SalesPerson`.

---

### Supplier

**Can Supplier monthly history be reconstructed?**

**Yes — for purchase-centric KPIs**, with documented caveats.

**Required scoped tables:**

| Purpose | Tables |
|---------|--------|
| Entity registry | `BTR_Supplier` |
| Purchase spend & invoice count | `BTR_Invoice` (+ void filter) |
| Purchase line mix | `BTR_InvoiceItem` |
| Net purchase adjustment | `BTR_ReturBeli`, `BTR_ReturBeliItem` |
| Item / catalog context | `BTR_Brg`, `BTR_Kategori` |
| Warehouse context | `BTR_Warehouse` |

**Reconstructable from schema:**

- PU-KPI-001 MTD Purchase (sum `GrandTotal` by `SupplierId` × month)
- PU-KPI-002 Invoice Count
- PU-KPI-003 Posted % — **only if `IsStokPosted` exists in deployed DB**; current flag may not reflect historical month-end state
- Active SKU count, purchase share, item-level purchase mix

**Limitations:**

- Inventory value / at-risk exposure KPIs require `BTR_StokBalanceWarehouse` + current `BTR_Brg.Hpp` (outside scope or current-state)
- Sales-outbound rollups per supplier require `BTR_FakturItem` (outside scope)
- `IsStokPosted` is mutable — historical posting percent may be inaccurate
- DDL/runtime schema drift on `BTR_Invoice`

---

### Item

**Can Item monthly history be reconstructed?**

**Partial only.**

**Required scoped tables for what *is* possible:**

| Purpose | Tables |
|---------|--------|
| Entity registry | `BTR_Brg` |
| Inbound purchase | `BTR_InvoiceItem` → `BTR_Invoice` |
| Inbound return | `BTR_ReturBeliItem` → `BTR_ReturBeli` |
| Outbound return (only) | `BTR_ReturJualItem` → `BTR_ReturJual` |
| Category / supplier dims | `BTR_Kategori`, `BTR_Supplier` |

**Reconstructable:**

- Monthly purchase quantity/value per SKU
- Monthly purchase return quantity/value per SKU
- Monthly sales **return** quantity/value per SKU
- Net inbound purchase per month

**Not reconstructable from scoped tables:**

- IN-KPI-001 Inventory Value — requires `BTR_StokBalanceWarehouse` and HPP
- IN-KPI-020 Days of Supply — requires stock position and sales consumption (`BTR_FakturItem` or `BTR_StokMutasi`)
- IN-KPI-021 Recommended Purchase Qty — requires forecast/consumption inputs outside scope
- Gross sales velocity, dead stock, slow moving — require sales and stock ledger tables

**Limitations:**

- `BTR_Brg.Hpp` is current cost — historical inventory valuation is approximate
- Category/supplier on item is current master state

---

### Inventory

**Can historical inventory positions be reconstructed from the scoped tables?**

**No.**

| Data need | Scoped table? | Evidence |
|-----------|---------------|----------|
| Current stock quantity | **No** | `BTR_StokBalanceWarehouse` not in scope — stores `(BrgId, WarehouseId) → Qty` current balance |
| Stock movement ledger | **No** | `BTR_StokMutasi` not in scope — append-style `(BrgId, WarehouseId, MutasiDate, QtyIn, QtyOut, ReffId, JenisMutasi)` |
| Purchase inbound | **Partial** | `BTR_Invoice` / `BTR_InvoiceItem` record purchase intent; stock impact occurs on posting via `BTR_StokMutasi` |
| Sales outbound | **No** | `BTR_Faktur` not in scope |
| Returns | **Partial** | `BTR_ReturBeli*`, `BTR_ReturJual*` exist but insufficient alone for running balance |

**Replay requires (beyond scoped list):**

- `BTR_StokMutasi` — authoritative movement history with dates and reference documents
- `BTR_Stok` — stock posting batches linked via `ReffId`
- `BTR_StokBalanceWarehouse` — current position (starting point for reverse replay only if no ledger gaps)
- Sales documents (`BTR_Faktur` / `BTR_FakturItem`) for outbound consumption
- `BTR_Mutasi` — non-purchase/sale adjustments (opname, transfer)

Scoped purchase tables alone can reconstruct **inbound document totals** but not warehouse quantity on hand at a past date.

---

### Receivable

**Can historical receivable positions be reconstructed from the scoped tables?**

**No.**

| Data need | Scoped table? | Evidence |
|-----------|---------------|----------|
| Receivable open items | **No** | `BTR_Piutang` not in scope — `(PiutangId, CustomerId, PiutangDate, DueDate, Sisa, Terbayar, …)` |
| Receivable element history | **No** | `BTR_PiutangElement` not in scope — payment/charge events with `ElementDate` |
| Customer current balance | **Partial / misleading** | `BTR_Customer.CreditBalance` — single current snapshot, not historized |
| Sales creating receivable | **No** | `BTR_Faktur` not in scope |

**Required tables (outside scope):** `BTR_Piutang`, `BTR_PiutangElement`, `BTR_PiutangLunas`, `BTR_Faktur`, payment/collection tables (`BTR_Tagihan`, etc.).

**Limitations:** Month-end open balance and overdue exposure cannot be derived from scoped tables. Replaying piutang requires reconstructing outstanding `Sisa` from element history as-of each month-end.

---

## Remaining Unknowns

The following information **cannot be determined** from the scoped tables and **must** come from additional sources:

### Essential missing tables for full Entity Analytics backfill

| Missing table(s) | Information required |
|------------------|---------------------|
| `BTR_Faktur`, `BTR_FakturItem` | Customer omzet, salesman omzet, item sales velocity, product mix, last-sale dates |
| `BTR_Piutang`, `BTR_PiutangElement`, `BTR_PiutangLunas` | Open balance, overdue, collection history |
| `BTR_StokMutasi`, `BTR_Stok`, `BTR_StokBalanceWarehouse` | Inventory position, movement class, days of supply |
| `BTR_SalesOmzetTarget` | Salesman achievement % denominator |
| `BTR_BrgSatuan` | Unit conversion for inventory quantities (used by live `StokBalanceViewDal`) |
| `BTR_Mutasi`, `BTR_MutasiItem` | Stock adjustments, transfers, opname |

### Schema verification unknowns

| Item | Risk |
|------|------|
| `BTR_Invoice.IsStokPosted` | Referenced in `InvoiceViewDal` but absent from checked-in DDL — confirm production column exists |
| `BTR_Invoice.Note` | Same DDL drift |
| `BTR_CheckIn.CustomerId` width vs `BTR_Customer.CustomerId` | Join reliability |
| `BTR_ReturJual.WarehouseId` width vs `BTR_Warehouse.WarehouseId` | Join reliability |

### Historical master-data unknowns

| Item | Impact |
|------|--------|
| Customer wilayah at past date | Radar peer groups approximate |
| Item supplier/category at past date | Supplier and item relationship history approximate |
| Item HPP at past date | Inventory value history approximate |
| Salesman territory at past date | Salesman radar approximate |
| Customer plafond at past date | Credit-limit breach signals approximate |

### Business-rule unknowns (not in DDL)

| Item | Notes |
|------|-------|
| Void semantics | Convention `VoidDate = '3000-01-01'` inferred from DAL queries — consistent across invoice, retur, faktur patterns |
| Visit plan re-materialization | Whether old plans are deleted or upserted on regeneration — affects visit history completeness |
| Stock posting timing | Relationship between `BTR_Invoice.IsStokPosted` and `BTR_StokMutasi` creation — affects posted % KPI accuracy |

---

## Final Conclusion

The scoped transactional tables provide **uneven** support for Entity Analytics historical replay:

1. **Supplier domain — viable core.** `BTR_Invoice` + `BTR_InvoiceItem` + `BTR_ReturBeli*` + `BTR_Supplier` + master dimensions contain dated, void-filterable purchase history sufficient to reconstruct monthly purchase spend, invoice counts, and item-level purchase mix. Posted-percent KPIs depend on `IsStokPosted` (schema drift risk) and reflect current posting state, not guaranteed historical state.

2. **Item domain — partial.** Inbound purchase and return history per SKU is available. Inventory value, days of supply, sales velocity, and dead/slow stock **cannot** be reconstructed without stock ledger and sales invoice tables.

3. **Customer and Salesman domains — not viable from scoped tables alone.** The scoped set contains no sales invoice data (`BTR_Faktur*`). Only returns and field-activity tables provide partial activity signals. Core Entity Analytics KPIs (omzet, achievement, open balance) **require tables outside this investigation scope**.

4. **Inventory and receivable positions — not viable from scoped tables.** Current balance and movement history live in `BTR_StokBalanceWarehouse` / `BTR_StokMutasi`; receivables live in `BTR_Piutang*`. `BTR_Customer.CreditBalance` is not a substitute for historical piutang reconstruction.

5. **Master tables are necessary but not sufficient.** `BTR_Brg`, `BTR_Customer`, `BTR_SalesPerson`, `BTR_Supplier`, `BTR_Kategori`, `BTR_Wilayah`, and `BTR_Warehouse` provide entity identity and **current** dimensions only. They do not historize attribute changes.

**Bottom line for architecture review:** The listed tables are a **necessary subset** for purchase-side and dimension resolution replay, but **insufficient as the complete replay source** for Entity Analytics historical backfill. A transaction-period replay architecture must include at minimum **`BTR_Faktur`/`BTR_FakturItem`** (sales), **`BTR_Piutang*`** (receivables), and **`BTR_StokMutasi`/`BTR_StokBalanceWarehouse`** (inventory), plus **`BTR_SalesOmzetTarget`** for salesman achievement — regardless of whether dashboard snapshot history exists.
