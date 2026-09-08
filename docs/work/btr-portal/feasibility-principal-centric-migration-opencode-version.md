# FEASIBILITY ASSESSMENT

## Principal-Centric Analytics Migration for BTR Portal

---

## 1. Executive Summary

### Request

Migrate BTR Portal analytics and dashboards from a Salesman-centric model to a Principal-centric model. The intended business model is:

```
Customer ↔ Principal
Principal ↔ Salesman
Customer ≠ Owned By Salesman
```

Currently, Faktur records link directly to `SalesPersonId`, and all sales analytics are grouped by Salesman. The request asks whether the portal should instead treat Principal as the primary ownership dimension for sales, with Salesman as a secondary operational dimension.

### Recommendation

**GO with moderate redesign.**

The migration is technically feasible and well-supported by existing infrastructure. The Entity Analytics platform already treats Supplier (Principal) as a first-class entity (M32.10). Purchasing dashboards already operate on a Principal-centric model. The main effort is creating Principal-level sales analytics equivalents and adding navigation integration.

### Feasibility Result

```
FEASIBLE
```

---

## 2. Request Understanding

**Requested capability:** Shift the primary analytical ownership dimension in sales dashboards from Salesman to Principal, while preserving Salesman as an operational/subordinate dimension.

**Business objective:** Align analytics with the actual business structure where:
- A Customer can be served by multiple Salesmen (via fakturs)
- A Salesman is responsible for a Principal (via `BTR_SalesPersonSupplier`)
- Customer relationships are not owned by individual Salesmen

**Expected outcome:** Dashboards, KPIs, and reports that answer Principal-level questions (e.g., "How is each Principal performing?") rather than only Salesman-level questions (e.g., "How is each Salesman performing?").

---

## 3. Current State Analysis

### Existing Business Flow

The current business flow (validated against source code and artifacts):

1. **Master Data:**
   - `BTR_Customer`: Has no `SalesPersonId` field. Customers are not owned by Salesmen at the master data level.
   - `BTR_SalesPersonSupplier`: Links Salesman to Principal (many-to-many).
   - `BTR_SalesPersonPrincipalTarget`: Sets monthly targets per Salesman per Principal.

2. **Transaction Flow:**
   - `BTR_Faktur` has `SalesPersonId` (invoice-time salesman) and `CustomerId`.
   - `BTR_FakturItem` links to `Brg.SupplierId` (Principal).
   - A single Customer can have Fakturs from multiple Salesmen across time.

3. **Analytics Attribution:**
   - Sales omzet is attributed to the invoice-time Salesman.
   - Piutang exposure is attributed to the invoice-time Salesman (on open Fakturs).
   - Principal achievement is calculated per Salesman per Principal (`BTRPD_SalesmanPrincipalAchievement`).

### Existing Components

#### Dashboard Architecture

| Dashboard | Primary Dimension | Current State |
|-----------|------------------|---------------|
| SA01 Sales | Company-level + Salesman ranking | Salesman-centric |
| SA02 Sales Forecast | Company-level | Agnostic |
| SF01 Salesmen Performance | **Salesman** | **Fully Salesman-centric** |
| SF02 Sales Force Overview | Team-level | Mostly agnostic |
| CU01–CU05 Customer Analytics | **Customer** | **Not Salesman-centric** (customer is independent entity) |
| FI01–FI04 Finance/Piutang | Customer + Salesman ranking | Mixed |
| PU01 Purchasing | **Principal/Supplier** | **Already Principal-centric** |
| IN01–IN05 Inventory | Item + Supplier dimension | Supplier is first-class |
| OP01 Locations | Warehouse + Wilayah | Agnostic |

#### KPI Catalog (by Entity Classification)

| Entity | KPI Count | Notes |
|--------|-----------|-------|
| Customer | 107 | Not salesman-centric; aligned with proposed model |
| Salesman | 53 | **All salesman-level KPIs** |
| Item | 38 | Agnostic |
| Supplier (Principal) | 23 | **All in Purchasing domain** |
| **Total** | **221** | |

#### Navigation Structure

| Menu Group | Purpose | Current Primary Dimension |
|------------|---------|--------------------------|
| MENU-EXECUTIVE | Company-wide scan | Company |
| MENU-SALES | Sales performance | Company + Salesman ranking |
| MENU-CUSTOMERS | Customer health | **Customer (not Salesman)** |
| MENU-FINANCE | Receivables | Customer + Salesman ranking |
| MENU-SALES-FORCE | Salesperson performance | **Salesman** |
| MENU-INVENTORY | Stock value/risk | Item + Supplier |
| MENU-PURCHASING | Purchase spend | **Principal (Supplier)** |
| MENU-OPERATIONS | Warehouse performance | Warehouse |

#### Reporting Context Database

Key tables reflecting the Salesman-centric model:

| Table | Primary Key Dimension | Purpose |
|-------|----------------------|---------|
| `BTRPD_SalesmanKpi` | SnapshotKey (company rollup) | Team-level salesman metrics |
| `BTRPD_SalesmanTopOmzet` | SalesPersonId | Top Salesmen by invoiced sales |
| `BTRPD_SalesmanTopPiutang` | SalesPersonId | Top Salesmen by receivable exposure |
| `BTRPD_SalesmanTopAchievement` | SalesPersonId | Top Salesmen by achievement % |
| `BTRPD_SalesmanSegmentation` | SalesPersonId | Salesman segmentation |
| `BTRPD_SalesmanAttention` | SalesPersonId | Attention signals per Salesman |
| `BTRPD_SalesmanRepHistory` | SalesPersonId | Historical performance per Salesman |
| `BTRPD_SalesmanPrincipalAchievement` | (SalesPersonId, SupplierId) | **Subordinate: per-Salesman, per-Principal breakdown** |

Key tables reflecting Principal-centric model (Purchasing):

| Table | Primary Key Dimension | Purpose |
|-------|----------------------|---------|
| `BTRPD_PurchasingKpi` | SnapshotKey | Company-level purchasing metrics |
| `BTRPD_PurchasingTopPrincipal` | Rank | Top Principals by purchase amount |
| `BTRPD_PurchasingManagementKpi` | SnapshotKey | Principal dependency metrics |
| `BTRPD_PurchasingManagementTopPrincipal` | Rank | Principals with compound dependency flags |

### Existing Database

#### Schema Validation

**Fact: Customer has NO SalesPersonId.**

```sql
-- BTR_Customer columns (validated)
CustomerId, CustomerName, CustomerCode,
WilayahId, KlasifikasiId, HargaTypeId,
Address fields, Tax fields,
IsSuspend, Plafond, CreditBalance,
Latitude, Longitude, Accuracy, CoordinateTimestamp, CoordinateUser
-- NO SalesPersonId
```

**Fact: Faktur HAS SalesPersonId.**

```sql
-- BTR_Faktur columns (validated)
FakturId, FakturDate, FakturCode, FakturCodeOri,
SalesPersonId,   -- ← Invoice-time salesman attribution
CustomerId,      -- ← No customer ownership by salesman
HargaTypeId, WarehouseId, ...
```

**Fact: Salesman-Principal assignment exists.**

```sql
-- BTR_SalesPersonSupplier (validated)
SalesPersonId, SupplierId  -- Many-to-many link
```

**Fact: Principal achievement is tracked but subordinate.**

```sql
-- BTRPD_SalesmanPrincipalAchievement (validated)
SalesmanPrincipalAchievementId, SnapshotKey,
SalesPersonId, SalesPersonCode, SalesPersonName,
SupplierId, SupplierName,
TargetAmount, CompletedOmzet, AchievementPercent, SortOrder
-- Indexed by (SnapshotKey, SalesPersonId) — subordinate to Salesman
```

### Existing Integrations

- **Entity Analytics Platform**: Supports Customer, Salesman, Supplier, and Item as first-class entities (M32.9–M32.11). Supplier already has full pipeline (L0–L5).
- **Domain Worker**: Refreshes all ReportingContext tables including Principal-level purchasing tables.

---

## 4. Impact Analysis

### Backend Impact

| Component | Impact | Details |
|-----------|--------|---------|
| **Domain Aggregator** | MODERATE | New Principal-level sales aggregators needed (parallel to existing Salesman-level) |
| **Entity Analytics Producer** | MODERATE | Supplier producer exists (M32.10); needs sales-domain KPIs added |
| **KPI Registry** | MODERATE | New SF/SA prefix KPIs for Principal-level sales metrics |
| **Navigation Assets** | LOW | Add Principal menu item under Sales Force or new "Principals" menu |
| **Business Question Catalog** | LOW | Add Principal-centric business questions |

### Database Impact

| Table | Impact | Action |
|-------|--------|--------|
| `BTRPD_PrincipalKpi` | NEW | Principal-level sales KPI rollup (analogous to `BTRPD_SalesmanKpi`) |
| `BTRPD_PrincipalTopOmzet` | NEW | Top Principals by invoiced sales (analogous to `BTRPD_SalesmanTopOmzet`) |
| `BTRPD_PrincipalTopAchievement` | NEW | Top Principals by achievement % (new; no purchasing equivalent) |
| `BTRPD_PrincipalAttention` | NEW | Attention signals per Principal (analogous to `BTRPD_SalesmanAttention`) |
| `BTRPD_SalesmanPrincipalAchievement` | MODIFY | Promote from subordinate to first-class; add inverse index by SupplierId |
| `BTRPD_SalesmanKpi` | PRESERVE | Keep for Salesman-level coaching; add Principal rollup |
| `BTRPD_SalesmanTopOmzet` | PRESERVE | Keep for Salesman ranking; add Principal cross-filter |
| `BTRPD_SalesmanTopPiutang` | PRESERVE | Keep for Salesman exposure; add Principal cross-filter |

### Frontend Impact

| Component | Impact | Details |
|-----------|--------|---------|
| **SF01 Salesmen Dashboard** | MAJOR | Add Principal selector/filter; add Principal Performance view |
| **SA01 Sales Dashboard** | MODERATE | Add Principal achievement widget alongside Salesman ranking |
| **Navigation Sidebar** | MODERATE | Add "Principals" menu item under Sales Force or new group |
| **Entity Profile (Supplier)** | MODERATE | Add sales-domain KPIs to existing Supplier profile (currently only purchasing) |
| **Drill-down routes** | MODERATE | Add Principal → Sales Report route |

### Integration Impact

| Integration | Impact | Details |
|-------------|--------|---------|
| **Entity Analytics API** | LOW | Supplier entity already exists; add sales KPIs to existing producer |
| **Domain Worker** | MODERATE | Extend refresh pipeline to include Principal sales aggregators |

### Security Impact

| Area | Impact | Details |
|------|--------|---------|
| **Role-based access** | LOW | Existing Principal visibility rules apply; no new security dimensions |
| **Data filtering** | LOW | Principal-level data filtered by same `BTR_SalesPersonSupplier` assignments |

---

## 5. Gap Analysis

| Gap ID | Type | Description | Evidence |
|--------|------|-------------|----------|
| GAP-001 | Functional | No Principal-level sales KPI rollup table exists | `BTRPD_PrincipalKpi` does not exist; only `BTRPD_SalesmanKpi` |
| GAP-002 | Functional | No Principal sales ranking (Top Omzet by Principal) | `BTRPD_PrincipalTopOmzet` does not exist |
| GAP-003 | Functional | No Principal attention signals for sales domain | `BTRPD_PrincipalAttention` does not exist |
| GAP-004 | Functional | No navigation menu for Principal sales analytics | Navigation registry has no "Principals" menu under Sales |
| GAP-005 | Functional | Business question catalog lacks Principal-centric sales questions | `business-question-catalog-v3.md` has MQ-004 (Principal purchase risk) but no Principal sales performance question |
| GAP-006 | Data | Principal achievement is subordinate to Salesman in reporting schema | `BTRPD_SalesmanPrincipalAchievement` indexed by `SalesPersonId`, not `SupplierId` |
| GAP-007 | UX | Salesman Performance dashboard has no Principal filter or view | SF01 dashboard analysis shows no Principal dimension in widgets |
| GAP-008 | Integration | Entity Analytics Supplier profile lacks sales KPIs | M32.10 implemented Supplier with purchasing KPIs only; no sales achievement KPIs |
| GAP-009 | Conceptual | No Customer-Principal analytical entity exists | No table or KPI captures "which Principal serves which Customer" relationship |

---

## 6. Solution Options

### GAP-001 through GAP-004: Principal-Level Sales Analytics

**Option A: Create New Principal Sales Tables (Recommended)**

Create new `BTRPD_PrincipalKpi`, `BTRPD_PrincipalTopOmzet`, `BTRPD_PrincipalAttention` tables modeled after existing Salesman equivalents. Promote `BTRPD_SalesmanPrincipalAchievement` to support reverse lookup by `SupplierId`.

- **Advantages:** Minimal disruption to existing Salesman-centric tables; clean separation; follows existing patterns
- **Disadvantages:** Duplication of table structure; requires new nav menu
- **Risk:** LOW — patterns are proven

**Option B: Extend Existing Tables with Principal Dimension**

Add `SupplierId` columns to existing Salesman tables and create views for Principal-level queries.

- **Advantages:** No new tables; single source of truth
- **Disadvantages:** Denormalized schema; complex queries; violates existing indexing strategy
- **Risk:** MEDIUM — schema complexity

**Recommendation:** Option A. Follows existing precedent (see Purchasing Principal tables).

### GAP-005: Business Question Catalog

**Option A: Add Principal-Centric Sales Questions (Recommended)**

Add new Management Questions:
- MQ-017: Which Principals are underperforming against target?
- MQ-018: Which Principals have weak Salesman coverage?

- **Advantages:** Minimal change; extends existing catalog
- **Disadvantages:** Requires navigation mapping
- **Risk:** LOW

**Recommendation:** Option A.

### GAP-006: Subordinate Principal Achievement

**Option A: Add Inverse Index (Recommended)**

Add index on `(SnapshotKey, SupplierId)` to `BTRPD_SalesmanPrincipalAchievement` and create view `V_PrincipalAchievement` for Principal-level queries.

- **Advantages:** Non-destructive; preserves existing data
- **Disadvantages:** View adds abstraction layer
- **Risk:** LOW

**Recommendation:** Option A.

### GAP-007: Navigation

**Option A: Add "Principals" Menu Under Sales Force (Recommended)**

Add `SF04 Principals → Principal Performance Dashboard` to navigation.

- **Advantages:** Clear separation from Salesman view; follows existing menu pattern
- **Disadvantages:** New menu item requires implementation
- **Risk:** LOW

**Recommendation:** Option A.

### GAP-008: Entity Analytics Supplier Profile

**Option A: Extend Supplier Producer with Sales KPIs (Recommended)**

Add sales-domain KPIs to existing `SupplierEntityAnalyticsProducer`:
- Principal Achievement %
- Principal Top Omzet Rank
- Principal Attention Signals

- **Advantages:** Reuses existing Entity Analytics pipeline
- **Disadvantages:** Requires KPI registry update
- **Risk:** LOW

**Recommendation:** Option A.

### GAP-009: Customer-Principal Relationship

**Option A: Derive from Faktur (Recommended)**

Create analytical view `V_CustomerPrincipalRelationship` joining `BTR_Faktur` → `BTR_FakturItem` → `BTR_Brg.SupplierId`. No new table needed.

- **Advantages:** No schema change; derives from existing transactional data
- **Disadvantages:** Computed at query time; not materialized
- **Risk:** LOW

**Option B: Materialize Customer-Principal Relationship Table**

Create `BTRPD_CustomerPrincipalRelationship` table with aggregated Customer-Principal metrics.

- **Advantages:** Fast queries; supports historical trend
- **Disadvantages:** Additional maintenance; requires worker logic
- **Risk:** MEDIUM

**Recommendation:** Option A (derive view). Materialization can be considered if performance requires.

---

## 7. Recommended Approach

**Preferred Solution:** Create Principal-level sales analytics as a parallel dimension to existing Salesman analytics, following the pattern already established in Purchasing.

**Rationale:**
1. The system already treats Principal as first-class in Purchasing (PU dashboards, PU-KPIs, Principal tables).
2. The Entity Analytics platform already supports Supplier as an entity type (M32.10).
3. Customer analytics are already independent of Salesman (CU dashboards), validating the proposed model.
4. The main effort is additive (new tables, new menu) rather than transformative (no existing behavior breaks).

**Major Implementation Strategy:**
1. Create Principal sales KPI tables (`BTRPD_PrincipalKpi`, `BTRPD_PrincipalTopOmzet`, `BTRPD_PrincipalAttention`).
2. Extend `BTRPD_SalesmanPrincipalAchievement` with inverse index.
3. Add Principal Performance Dashboard (SF04).
4. Extend Entity Analytics Supplier profile with sales KPIs.
5. Add Principal-centric business questions to catalog.
6. Add navigation menu item.

**Implementation Constraints:**
- Preserve all existing Salesman-centric dashboards (SF01, SF02).
- No changes to master data (`BTR_Customer`, `BTR_Faktur` schema).
- No changes to existing KPI catalog IDs (add new IDs, do not modify).
- Principal achievement calculation follows existing `BTR_SalesPersonSupplier` assignment rules.

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Principal Salesman assignment coverage incomplete | HIGH | MEDIUM | Validate `BTR_SalesPersonSupplier` completeness before migration; flag unassigned Principals |
| Historical data inconsistency (pre-migration vs. post-migration) | MEDIUM | MEDIUM | New tables start with current period; historical analysis remains Salesman-based |
| User confusion (two dimensions: Salesman vs. Principal) | MEDIUM | HIGH | Clear navigation labels; documentation; phased rollout |
| Performance impact of new Principal aggregators | MEDIUM | LOW | Follow existing materialization patterns; monitor refresh times |
| Entity Analytics Supplier profile conflicts | LOW | LOW | Extending existing producer; no conflicts expected |
| Customer-Principal relationship ambiguity | MEDIUM | MEDIUM | Derive from Faktur; document attribution rules |

---

## 9. Open Questions

### Business Questions

| ID | Question | Priority |
|----|----------|----------|
| BQ-001 | Should Principal-level sales dashboards show only Principals assigned to the logged-in user's Salesman, or all Principals? | HIGH |
| BQ-002 | How should Principal achievement be calculated when a Customer is served by multiple Salesmen? (Pro-rata? Lastinvoice?) | HIGH |
| BQ-003 | Should Customer-Principal relationship be a first-class analytical entity, or derived on-demand? | MEDIUM |
| BQ-004 | Should the Salesman Performance dashboard (SF01) retain its current focus, or be split into Salesman view and Principal view? | MEDIUM |
| BQ-005 | What is the expected role of Principal in Executive Dashboard (EX01) — top-level attention signal or drill-down? | LOW |

### Technical Questions

| ID | Question | Priority |
|----|----------|----------|
| TQ-001 | Should new Principal tables use `SupplierId` or `PrincipalId`? (Current schema uses `SupplierId`) | HIGH |
| TQ-002 | What is the refresh frequency for Principal-level tables? (Same as Salesman: daily?) | MEDIUM |
| TQ-003 | Should Principal achievement target use `BTR_SalesPersonPrincipalTarget` directly, or aggregate to Principal level? | MEDIUM |

### Operational Questions

| ID | Question | Priority |
|----|----------|----------|
| OQ-001 | What is the rollout strategy — parallel run with Salesman view, or cutover? | HIGH |
| OQ-002 | Who are the primary users of Principal-level dashboards? (Purchasing team? Sales management? Both?) | MEDIUM |

---

## 10. Implementation Impact Inventory

### Backend

| Component | Change Type | Description |
|-----------|-------------|-------------|
| `btr.portal.worker` | NEW | Principal sales aggregator service |
| `btr.application` | NEW | Principal achievement calculation policy |
| `btr.infrastructure` | NEW | Principal DAL classes (analogous to Salesman DAL) |
| `btr.domain` | MINOR | Principal entity metadata for Entity Analytics |
| `BTRPD_SalesmanPrincipalAchievement` | MODIFY | Add index on `(SnapshotKey, SupplierId)` |

### Database

| Object | Change Type | Description |
|--------|-------------|-------------|
| `BTRPD_PrincipalKpi` | NEW | Principal-level sales KPI rollup |
| `BTRPD_PrincipalTopOmzet` | NEW | Top Principals by invoiced sales ranking |
| `BTRPD_PrincipalTopAchievement` | NEW | Top Principals by achievement % ranking |
| `BTRPD_PrincipalAttention` | NEW | Attention signals per Principal |
| `BTRPD_PrincipalRepHistory` | NEW | Historical performance per Principal |
| `BTRPD_SalesmanPrincipalAchievement` | MODIFY | Add index on `SupplierId` |
| `V_CustomerPrincipalRelationship` | NEW | Derived view for Customer-Principal analytics |

### Frontend

| Component | Change Type | Description |
|-----------|-------------|-------------|
| SF04 Principal Performance Dashboard | NEW | Principal-level sales analytics dashboard |
| SA01 Sales Dashboard | MODIFY | Add Principal achievement widget |
| Navigation Sidebar | MODIFY | Add "Principals" menu item |
| Entity Analytics Supplier Profile | MODIFY | Add sales KPIs to existing profile |
| Business Question Catalog | MODIFY | Add Principal-centric questions |

### Integration

| Component | Change Type | Description |
|-----------|-------------|-------------|
| Entity Analytics Producer | MODIFY | Extend Supplier producer with sales KPIs |
| Domain Worker | MODIFY | Add Principal refresh step |
| KPI Registry | MODIFY | Register new Principal sales KPIs |

### Security

| Component | Change Type | Description |
|-----------|-------------|-------------|
| Role-based filtering | NO CHANGE | Existing Principal visibility rules apply |
| Data access | NO CHANGE | Same `BTR_SalesPersonSupplier` filter applies |

---

## 11. Planning Readiness

### Status

```
NOT READY
```

### Blocking Issues

1. **BQ-001 (Principal visibility scope):** Must determine whether Principal dashboards show all Principals or only assigned Principals before designing navigation and data access.
2. **BQ-002 (Multi-Salesman Customer attribution):** Must define how Principal achievement is calculated when a Customer has fakturs from multiple Salesmen before designing achievement logic.
3. **OQ-001 (Rollout strategy):** Must decide between parallel run or cutover before planning implementation sequence.

### Planner Guidance

**Implementation scope:** Moderate. Estimated 3–5 milestones:
- Milestone 1: Database schema (new tables, indexes)
- Milestone 2: Domain worker (Principal aggregators)
- Milestone 3: Entity Analytics extension (Supplier sales KPIs)
- Milestone 4: Frontend (SF04 dashboard, navigation)
- Milestone 5: Business question catalog update

**Major dependencies:**
- `BTR_SalesPersonSupplier` data quality (coverage validation required)
- Existing Salesman-centric tables must remain functional during transition
- Entity Analytics Supplier producer (M32.10) must be complete before extension

**Sequencing concerns:**
- Principal tables should be created before frontend dashboard (data first, UI second)
- Navigation changes can be done in parallel with backend
- Business question catalog update is low-effort, can be done last

**Review concerns:**
- Validate Principal achievement calculation against manual calculations
- Compare Principal totals with Salesman totals (should reconcile at company level)
- Test multi-Salesman Customer scenarios

---

## Appendix: Evidence Summary

### Facts Validated from Codebase

1. **`BTR_Customer` has no `SalesPersonId`** — Validated in `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_Customer.sql`
2. **`BTR_Faktur` has `SalesPersonId`** — Validated in `src/j05-btr-distrib/btr.sql/Tables/SalesContext/BTR_Faktur.sql`
3. **`BTR_SalesPersonSupplier` exists** — Validates Principal-Salesman many-to-many relationship
4. **`BTRPD_SalesmanPrincipalAchievement` exists and is subordinate** — Validated; indexed by `SalesPersonId`
5. **Entity Analytics supports Supplier entity** — Validated in `docs/features/entity-analytics/entity-analytics-developer-guide.md`
6. **Purchasing dashboards are Principal-centric** — Validated in KPI catalog (PU-KPI prefix)
7. **Customer dashboards are NOT Salesman-centric** — Validated in KPI catalog (CU-KPI prefix has 107 KPIs, none attributed to Salesman)
8. **No `BTRPD_PrincipalKpi` or equivalent exists** — Validated via schema search

### Assumptions (Not Validated)

1. Business users intend Principal as primary sales ownership dimension — **To be confirmed with business**
2. Current Salesman-centric behavior is intentional and not legacy technical debt — **To be validated with business**
3. Multi-Salesman Customer scenarios are rare or acceptable — **To be validated with data**
