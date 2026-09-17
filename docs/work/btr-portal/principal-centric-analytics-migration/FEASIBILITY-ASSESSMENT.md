# FEASIBILITY ASSESSMENT

## Principal-Centric Analytics Migration for BTR Portal

| Field | Value |
| --- | --- |
| Status | Analysis only; no implementation |
| Assessment date | 2026-09-07 |
| Requested decision | NO-GO / minor changes / moderate redesign / major redesign |
| Evidence boundary | Repository artifacts, source code, and SQL definitions at assessment time; production-data profiling is an implementation validation activity per GAP-007 |
| Planning handoff | Ready — all blocking business and technical decisions in sections 9 and 11 are resolved; proceed to implementation planning |

---

## 1. Executive Summary

### Request

Assess whether BTR Portal analytics should migrate from a Salesman-centric model to a Principal-centric model based on the proposed operating model:

```text
Customer ↔ Principal
Principal ↔ Salesman
Customer ≠ Owned By Salesman
```

The requested scope includes dashboards, KPIs, reports, navigation, Entity Analytics, ReportingContext projections, the database/reporting model, and future roadmap direction.

### Recommendation

**GO with major redesign**, subject to remaining approved business decisions in section 9.

The proposal is directionally supported but is not fully proven by the current system:

- A Customer master record has no `SalesPersonId`; the schema does not model Customer ownership by Salesman.
- Salesman-to-Principal assignment and Salesman × Principal monthly targets are explicit.
- Sold line items can be attributed to a Principal through `FakturItem → Brg.SupplierId`.
- Supplier/Principal is already a first-class Entity Analytics type and a primary dimension in Purchasing and Inventory.

However:

- Salesman-to-Principal assignment is many-to-many (**confirmed by GAP-001 decision**), not one Principal per Salesman.
- No constraint verifies that a Faktur's Salesman is assigned to every Principal represented by its items.
- Customer-to-Principal is **transaction-derived**, not master data (**confirmed by GAP-003**). It is inferred from item sales through `FakturItem → Brg.SupplierId`.
- Historical Principal attribution uses the **Item master** (`BTR_Brg.SupplierId`); each Item belongs to exactly one Principal, treated as immutable for analytics (**confirmed by GAP-004**). No invoice-time Principal snapshot will be introduced in this initiative.
- Company sales use Faktur-header `GrandTotal`, while existing Principal sales-out logic sums Faktur-item `Total`. Mixed-Principal Fakturs therefore need an explicit **sales** reconciliation policy (**resolved by GAP-019**): line-item amounts are the authoritative Principal sales measure; header totals are not allocated and reconciliation to `GrandTotal` is not required.
- Accounts Receivable, open balance, collection performance, and credit exposure remain **Customer-level only**; they are not attributed to Principals (**confirmed by GAP-005**).
- Several Customer analytics paths present a single last-invoicing Salesman as if it were the Customer's assigned Salesman.
- Supplier Entity Analytics is currently produced by the Purchasing Management worker and its core trend KPIs represent **purchase-in**, not sales-out. Extending it is not a metadata-only change.

This is not a simple replacement of `SalesPersonId` with `SupplierId`. The safe target is a multi-dimensional analytical model:

```text
Principal              = commercial/product portfolio axis
Salesman (commercial)  = portfolio owner on transaction; single attribution for revenue, target, performance, bonus, and customer responsibility on sale (GAP-002)
Salesman (field)       = operational execution axis; attributed to the Salesman who performs the activity (GAP-002)
Customer               = independent account/credit axis
Customer–Principal     = transaction-derived analytical relationship (GAP-003)
Item–Principal         = one Principal per Item via master data; immutable for analytics (GAP-004)
Financial (AR/credit)  = Customer/company grain only; not Principal-attributed (GAP-005)
Salesman–Principal     = monthly target records for historical responsibility (GAP-006)
```

### Feasibility Result

```text
PARTIALLY FEASIBLE
```

The data needed for Principal sales-out and Customer–Principal sales relationships exists for current-state analytics. A system-wide migration is only partially feasible until sales reconciliation, target authority, terminology, and access-scope decisions are made.

### Complexity Decision

```text
GO WITH MAJOR REDESIGN
```

An additive Principal sales dashboard alone would be a moderate change. The requested migration affects the analytical ontology, KPI catalog, evidence grain, navigation, Entity Analytics producer ownership, historical replay, and roadmap; that broader scope is a major redesign.

---

## 2. Request Understanding

### Requested capability

Make Principal the primary commercial ownership dimension for BTR Portal analytics while no longer implying that a Salesman owns a Customer relationship.

### Business objective

Align management analytics with an operating model in which:

- Principals define the product portfolios sold to Customers.
- Salesmen execute selling and field work for assigned Principals.
- A Customer may interact with multiple Salesmen because it buys multiple Principals.
- Customer credit and receivable exposure remains a **Customer/company concern only**; Principal financial attribution is excluded (GAP-005).

### Expected outcome

- Principal sales performance, target, growth, reach, mix, risk, and coverage become directly answerable.
- Salesman analytics remain available for execution and coaching, interpreted as performance within assigned Principal portfolios rather than Customer ownership.
- Customer analytics can show which Principals a Customer buys, has stopped buying, or could buy.
- Portal questions and navigation distinguish commercial portfolio, customer account, and sales execution.
- Every KPI remains traceable to evidence at a compatible grain.

### Approved operating-model decisions

| ID | Decision | Status |
| --- | --- | --- |
| GAP-001 | Salesman–Principal is many-to-many commercial portfolio responsibility; Principal is the primary commercial responsibility dimension; Customer is not owned by Salesman. | **Approved** |
| GAP-002 | Commercial attribution dimensions are unified under the portfolio owner recorded on the transaction; field activity is a separate operational dimension attributed to the performer. | **Approved** |
| GAP-003 | Customer–Principal relationships are transaction-derived from sales history; no separate master assignment record is required or permitted before purchase. | **Approved** |
| GAP-004 | Each Item belongs to exactly one Principal via Item master; Principal ownership is immutable for analytics; historical attribution uses Item master with no invoice-time snapshot in this initiative. | **Approved** |
| GAP-005 | AR, open balance, collection performance, and credit exposure remain Customer-level; Principal analytics are sales/portfolio/market-performance only; no open-balance allocation across Principals. | **Approved** |
| GAP-006 | Historical Salesman–Principal responsibility is derived from monthly `BTR_SalesPersonPrincipalTarget` records; no effective-dated assignment model in this initiative. | **Approved** |
| GAP-007 | Initiative proceeds on approved attribution assumptions; production profiling is implementation validation and does not block planning. | **Approved** |

See sections 5.1 through 5.7 for full architectural consequences.

### Non-goals of this assessment

- It does not decide the unresolved business rules.
- It does not define a target architecture or implementation plan.
- It does not authorize changes to code, database, or permanent knowledge.

---

## 3. Current State Analysis

### 3.1 Evidence classification

#### Facts established from repository evidence

| ID | Fact | Evidence |
| --- | --- | --- |
| F-001 | `BTR_Customer` has no Salesman owner field. | `btr.sql/Tables/SalesContext/BTR_Customer.sql` |
| F-002 | Each Faktur stores one `SalesPersonId` and one `CustomerId`. | `btr.sql/Tables/SalesContext/BTR_Faktur.sql` |
| F-003 | Faktur items store `BrgId` but not `SupplierId`; Principal is resolved through the current item master. | `BTR_FakturItem.sql`, `BTR_Brg.sql` |
| F-004 | Salesman-to-Principal assignment is many-to-many by composite key and has no effective dates. | `BTR_SalesPersonSupplier.sql` |
| F-005 | Monthly targets are stored at Salesman × Principal × year × month grain. | `BTR_SalesPersonPrincipalTarget.sql` |
| F-006 | Existing Principal sales achievement groups item-line totals by Salesman and Supplier. | `FakturPrincipalOmzetDal.cs`, `DashboardSalesmanAggregator.cs` |
| F-007 | Existing Principal achievement is stored and read as a child of one Salesman. | `BTRPD_SalesmanPrincipalAchievement.sql`, `IDashboardSalesmanSnapshotDal.cs` |
| F-008 | Customer portfolio analytics use the Salesman on the latest Faktur as the displayed Salesman and as input to Salesman achievement/exposure context. | `CustomerLastFakturDal.cs`, `DashboardCustomerPortfolioAggregator.cs` |
| F-009 | Customer Entity Analytics labels a latest-Faktur-derived relationship as `AssignedSalesman`, singular and Top 1. | `CustomerRelationshipCatalog.cs`, `CustomerEntityAnalyticsProducer.cs` |
| F-010 | Customer Entity Analytics already materializes `TopPrincipalsByOmzet` from transaction-line relationships. | Same files as F-009 plus Customer relationship aggregation |
| F-011 | Supplier Entity Analytics is enabled and has full L0–L5 platform support. | `entity-analytics-roadmap-authoritative.md`, `entity-analytics-developer-guide.md` |
| F-012 | Supplier Entity Analytics core KPIs are MTD purchase, purchase invoice count, and posted percent; its Growth radar axis is purchase growth. | `SupplierEntityAnalyticsRegistrar.cs` |
| F-013 | Supplier relationships already include Top Customers, Top Salesmen, and Top Items by item-line sales amount. | `SupplierMtdItemRollupDal.cs`, `DashboardSupplierRelationshipAggregator.cs`, `SupplierEntityAnalyticsProducer.cs` |
| F-014 | The Supplier producer is owned by the `PurchasingManagement` worker domain and replaces the Supplier L0 snapshot. | `SupplierEntityAnalyticsProducer.cs` |
| F-015 | Portal navigation has 25 items in eight domain groups; Sales Force is explicitly Salesman-focused, while Purchasing and Supplier profiles are Principal-focused. | `portalMenuRegistry.ts`, `navigation-assets.md` |
| F-016 | The KPI classification forces all 221 KPIs into Customer, Salesman, Item, or Supplier, including company and location aggregates. | `kpi-entity-classification.md` |
| F-017 | Portal authentication is common JWT access; role-based menu/data visibility is deferred. | `btr-portal-domain.md`, `btr-portal-architecture.md`, router and API authorization conventions |
| F-018 | Piutang and payment models searched in the repository expose no `SupplierId`/Principal attribution. | Reporting and core Piutang/Payment source search; `BTR_Faktur` stores header `KurangBayar` |
| F-019 | Purchasing Management computes a Principal `SalesOutAmount` in memory, but that value is not persisted as a first-class Supplier KPI or monthly history. | `DashboardPurchasingManagementAggregator.cs`, `SupplierEntityAnalyticsProducer.cs`, ReportingContext table definitions |
| F-020 | Supplier `TopCustomersByOmzet` is sales-line-derived but its relationship metadata references purchase KPI `PU-KPI-001`; other Supplier omzet relationships reference Salesman KPI `SF-KPI-008`. | `SupplierRelationshipCatalog.cs`, `SupplierMtdItemRollupDal.cs` |
| F-021 | Route templates and visit plans can operationally associate a Customer with Salesmen, but the relationship is many-to-many/per-date rather than master ownership. | `BTR_SalesRuteItem.sql`, `BTR_VisitPlan.sql`, visit-plan feature artifacts |
| F-022 | Current Principal achievement excludes sales returns. | `sales-person-principal-target/feature.md` and Principal achievement implementation |
| F-023 | Salesman–Principal responsibility is officially many-to-many commercial portfolio assignment, not exclusive Customer ownership. | GAP-001 decision (2026-09-07) |
| F-024 | Commercial Ownership, Customer Responsibility, Invoice Attribution, Sales Target Attribution, Sales Performance Attribution, and Bonus Attribution are a single dimension sourced from the portfolio owner on the transaction; field activity is a separate operational dimension. | GAP-002 decision (2026-09-07T20:30:00+07:00) |
| F-025 | Customer–Principal relationships are transaction-derived from qualifying item sales; they are not maintained as independent master data and cannot be manually assigned before purchase. | GAP-003 decision (2026-09-07) |
| F-026 | Each Item belongs to exactly one Principal; Principal ownership is immutable for analytics and historical reporting; attribution uses Item master with no invoice-time Principal snapshot in this initiative. | GAP-004 decision (2026-09-07) |
| F-027 | Accounts Receivable, open balance, collection performance, and credit exposure are Customer-level metrics and are not attributed to individual Principals in this initiative. | GAP-005 decision (2026-09-07) |
| F-028 | Historical Salesman–Principal responsibility is derived from `BTR_SalesPersonPrincipalTarget` at monthly grain (`TargetYear`, `TargetMonth`); no effective-dated assignment model is introduced in this initiative. | GAP-006 decision (2026-09-07) |
| F-029 | Production-data profiling is an implementation validation activity; mixed-Principal invoices, assignment exceptions, Item–Supplier changes, and historical anomalies do not block the Principal-centric redesign. | GAP-007 decision (2026-09-07) |

#### Claims not established as facts

| ID | Claim requiring validation | Why it remains an assumption |
| --- | --- | --- |
| A-001 | Every Salesman is responsible for exactly one Principal. | **Disproven** by GAP-001 decision; schema and business rule both allow many-to-many. |
| A-002 | Every sold Principal is assigned to the Faktur's Salesman. | No database constraint or save-time validation was found proving this. |
| A-003 | Customer–Principal is an enduring serviced relationship or manually assignable before purchase. | **Disproven** by GAP-003 decision; relationship is transaction-derived only. |
| A-004 | Principal should replace Salesman on all management surfaces. | Field activity, route, visit, order, and coaching are intrinsically Salesman-grained. |
| A-005 | Production-data profiling must complete before any planning may begin. | **Superseded** by GAP-007; profiling is required during implementation validation, not as a prerequisite business decision. |
| A-006 | Principal reassignment for existing Items is a supported or frequent business scenario requiring invoice-time attribution. | **Disproven as in-scope requirement** by GAP-004 assumption; Item master attribution is approved for this initiative. TQ-003 remains a validation check. |
| A-007 | Principal visibility should be limited by Salesman assignment. | **Not adopted** per BQ-008 and GAP-018: existing role-based commercial visibility applies — any user authorized to access commercial analytics may view all Principals; no Principal-specific visibility limits in this initiative; architecture remains extensible for future Principal-level rules. |

### 3.2 Current business and data flow

```text
Customer
  └─ Faktur header
       ├─ one invoice-time Salesman
       ├─ header totals / due date / open balance
       └─ Faktur items
            └─ Item
                 └─ one Principal via `Brg.SupplierId` (immutable for analytics per GAP-004)

Salesman
  ├─ many-to-many Supplier/Principal assignments
  ├─ Principal-specific monthly target rows
  ├─ commercial attribution on Faktur (portfolio owner)
  └─ field plans, visits, and operational execution (performer-attributed)
```

The model supports distinct meanings that current artifacts sometimes blur. **GAP-002** resolves how they relate:

1. **Commercial attribution (unified):** the Salesman recorded on a Faktur is the portfolio owner for Commercial Ownership, Customer Responsibility on the sale, Invoice Attribution, Sales Target Attribution, Sales Performance Attribution, and Bonus Attribution.
2. **Salesman–Principal responsibility (historical):** a Salesman is responsible for a Principal in periods where a `BTR_SalesPersonPrincipalTarget` row exists for that Salesman × Principal × `TargetYear` × `TargetMonth` combination (GAP-006). `BTR_SalesPersonSupplier` may indicate current eligibility but is not the approved historical responsibility source.
3. **Operational execution:** route templates, visit plans, and field activity attributed to the Salesman who actually performs the work; coverage execution does not transfer commercial attribution.
4. **Customer–Principal relationship (transaction-derived):** a Customer is related to a Principal when the Customer purchases Items belonging to that Principal; derived from sales history, not master assignment (per GAP-003).
5. **Financial exposure (Customer-only):** accounts receivable, open balance, collection performance, and credit exposure remain at Customer/company grain and are not allocated to Principals (per GAP-005).

Commercial attribution and operational execution are **not equivalent** and must not be conflated in KPI design. Financial metrics must not be inferred from Principal sales-out.

### 3.3 Degree of Salesman-centricity by subsystem

| Subsystem | Finding |
| --- | --- |
| Dashboard architecture | Mixed. Company, Customer, Inventory, Purchasing, and Location dashboards are not globally Salesman-centric; SA01 and Sales Force surfaces are. Customer and Finance surfaces contain Salesman attribution assumptions. |
| KPI architecture | Structurally biased by a four-entity forced classification. Company sales and location KPIs are sometimes classified as Salesman even when their actual grain is company or location. |
| Reporting pipeline | Header-level Sales and Piutang evidence is Salesman-attributed. Principal sales-out exists only as item-line aggregation, primarily under Salesman achievement and Supplier relationships. |
| Navigation | Sales Force is correctly people/execution-focused. No primary Principal commercial-performance path exists; Principal appears mainly under Purchasing and Supplier Entity Analytics. |
| Entity Analytics | Both Salesman and Supplier are first-class. Customer relationships incorrectly use singular `AssignedSalesman`; Supplier performance semantics are purchase/inventory-led. |
| Database/reporting model | Supports Principal assignment, line-derived sales via immutable Item–Principal master data (GAP-004), and transaction-derived Customer–Principal relationships; Customer-level financial metrics are not Principal-attributed (GAP-005). |

---

## 4. Impact Analysis

### 4.1 Dashboard and analytics-view disposition

| Surface | Disposition | Principal-centric impact |
| --- | --- | --- |
| EX01 Management Attention Center | Redesign | Company totals remain valid. Sales attention needs Principal commercial health alongside, or ahead of, Salesman contribution. Existing purchase/inventory Principal exposure remains valid. |
| EX02 Alert Center | Redesign | Salesman execution alerts remain; new Principal sales alerts need deduplication and ownership rules. Principal collection/credit alerts are excluded (GAP-005). |
| EX03 Entity Analytics | Redesign | Supplier profile must distinguish sales-out from purchase-in and inventory. Customer `Assigned Salesman` must be renamed/reworked. Pair-grain Customer–Principal analysis is absent. |
| SA01 Sales | Major redesign | Company totals and weekly trend remain valid. Top Salesman remains useful as contribution/coaching, but cannot be the only commercial decomposition. Principal target/achievement and contribution are missing. |
| SA02 Sales Forecast | Moderate redesign | Company forecast is valid. Principal forecast/pace and reconciliation to company target are missing. Salesman-level classification in the KPI catalog is misleading for company forecast KPIs. |
| SA03 Sales Report | Major redesign | Current evidence grain is one Faktur. A Faktur can contain multiple Principals, so Principal filters/amounts require line or allocated-header evidence and explicit reconciliation semantics. |
| CU01 Customer Analytics | Moderate redesign | Customer sales, piutang, lifecycle, credit, and concentration remain valid. Salesman portfolio/ownership interpretation must be removed; Principal mix and relationship health become relevant. |
| CU02 Customer Risk Forecast | Major redesign | Customer-level risk remains valid. Salesman-derived low-recovery context and single-Salesman attribution need review. Principal-specific decline/inactivity cannot be inferred from customer totals alone. |
| CU03 Collection Optimization | Moderate redesign | Customer collection queues remain valid. Routing work to a Salesman may remain operational, but cannot imply account ownership. Principal collection impacts are out of scope (GAP-005). |
| CU04 Customer Portfolio | Major redesign | Customer lifecycle and credit remain valid. Salesman filter, displayed Salesman, and Salesman achievement/exposure context are based on latest Faktur and become misleading. Principal portfolio mix and gaps are absent. |
| CU05 Customer Report | Major redesign | Customer totals remain valid. `Owner`/`Salesman` columns require semantic correction. Customer–Principal evidence needs a separate pair/relationship grain, not one scalar column. |
| FI01 Piutang | Largely valid | Customer/Faktur aging remains Customer-level. Principal attribution is excluded (GAP-005). |
| FI02 Collection | Moderate redesign | Customer and Wilayah views remain valid. Top Overdue Salesmen is invoice attribution, not Customer ownership; labels and decisions need correction. No Principal overdue/collection KPIs (GAP-005). |
| FI03 Cash Flow Forecast | Largely valid | Company/customer collection forecast remains valid. Principal collection forecast is out of scope (GAP-005). |
| FI04 Piutang Report | Moderate redesign | Facts remain valid. Sales column must be described as invoice-attributed Salesman. Principal financial columns are excluded (GAP-005). |
| SF01 Salesman Performance | Reposition, not remove | Remains valid for execution/coaching, target allocation, invoiced contribution, and assigned Principal mix. “Owned book” and dormant portfolio claims require correction. |
| SF02 Sales Force Overview | Valid | Salesman/day is the correct grain for field execution. Principal filters may be useful only if route/order assignment semantics are defined. |
| SF03 Salesman Field Activity | Valid | Salesman/day/visit remains intrinsically people-centric. It should not be migrated to Principal as primary grain. |
| IN01 Inventory | Valid / enhance | Supplier/Principal already exists as a primary rollup. Commercial sales-out context may enhance investigation but does not invalidate current metrics. |
| IN02 Inventory Risk | Valid / enhance | Supplier risk exposure is already Principal-relevant. Link to Principal commercial performance when available. |
| IN03 Inventory Forecast | Valid / enhance | Item forecast remains correct; Principal portfolio rollup is a possible new view. |
| IN04 Inventory Optimization | Valid / enhance | Item/warehouse actions remain correct; Principal program or portfolio constraints are future additions. |
| IN05 Inventory Report | Valid | Item/warehouse evidence remains valid; Supplier filter already has a natural meaning. |
| PU01 Purchasing | Valid but semantically separate | Already Principal-centric for purchase-in, posting, stock, and dependency. It must not be presented as Principal sales performance. |
| PU02 Purchasing Report | Valid | Purchase invoice evidence remains Principal/Supplier-grained. |
| OP01 Locations | Valid | Warehouse/Wilayah are the real dimensions. KPI entity classification should stop treating location sales aggregates as Salesman KPIs. |

### 4.2 KPI catalog impact

The existing catalog should not be mechanically converted from Salesman to Supplier. It needs a semantic reclassification.

| KPI group | Disposition |
| --- | --- |
| Company sales totals, target, achievement, trend, forecast | Preserve; classify as Company/Sales aggregate rather than Salesman. Add Principal decompositions that reconcile to these totals. |
| Salesman omzet, target, achievement, concentration | Preserve as execution/contribution KPIs. Remove Customer-ownership language. |
| Salesman piutang/overdue | Preserve only as invoice-attributed workload/exposure. Rename definitions and warnings to avoid “owned book.” |
| Salesman dormant Customer portfolio | Invalid as ownership KPI. Replace with latest-invoice attribution wording or redesign as Principal-specific Customer inactivity plus explicit work assignment. |
| Field activity KPIs | Preserve as Salesman KPIs. |
| Customer sales/credit/piutang/lifecycle KPIs | Preserve at Customer/company grain. Add Principal-specific **sales/portfolio** variants only where transaction history supports them; no Principal financial variants (GAP-005). |
| Supplier purchase, posting, inventory, and risk KPIs | Preserve. Explicitly label them purchase-in/inventory; they are not substitutes for Principal sales-out. |
| Forced entity classifications for Executive, Sales, and Location KPIs | Redesign catalog taxonomy to support Company, Transaction, Location, and Relationship scopes. |

New KPI groups that would emerge:

1. **Principal Sales Performance**
   - MTD sales-out amount and share
   - Principal target, achievement amount, and achievement percent
   - month-over-month and rolling sales-out growth
   - active Customer count and sales concentration
   - assigned/active Salesman coverage
   - Principal forecast, required pace, and target gap
2. **Customer–Principal Relationship**
   - MTD and rolling sales-out
   - first/last purchase date for that Principal
   - activity/lifecycle for that relationship
   - purchase frequency, item breadth, and share of Customer sales
   - decline/dormancy by Principal
3. **Principal Portfolio Health**
   - sales-out vs purchase-in vs inventory movement
   - stock-out/overstock/at-risk context
   - customer breadth and concentration
   - assignment/target coverage quality
4. **Principal Returns** (independent of Sales; do not reduce Sales-Out)
   - Good Return Amount
   - Broken Return Amount
   - Total Return Amount
   - Return Rate (%) — usable as a quality indicator and radar axis

Explicitly excluded by GAP-005 (not in scope for this initiative):

- Principal piutang, overdue, aging, DSO, recovery, cash collection, credit exposure, and collection quality.
- Allocation of invoice-level open balances across multiple Principals.

Also excluded pending other decisions:

- Principal margin when cost/rebate/claim semantics are not approved.

### 4.3 Business question and Navigation Playbook impact

#### Questions that become misleading or incomplete

| Question | Impact |
| --- | --- |
| MQ-003 “Which salespeople are carrying overdue or dormant books?” | “Carrying” implies ownership. Overdue can be invoice-attributed; dormancy currently assigns the whole Customer to the last invoicing Salesman. Redesign wording and evidence. |
| MQ-009 “Which salespeople are underperforming?” | Still valid for coaching, but Principal performance/mix should be a parallel diagnostic, not merely a drawer under Salesman. |
| MQ-010 “Which customers are declining?” | Customer-total decline remains valid but may hide that only one Principal relationship declined. Add Principal-specific relationship analysis. |
| MQ-014 “Are we too dependent on a few salespeople?” | Still valid as execution/key-person risk. It should no longer stand in for commercial portfolio dependence. |
| EQ-005 “Is growth sustainable?” | Currently routes only to Salesman underperformance and Customer decline. It needs Principal growth and portfolio breadth. |
| EQ-007 “Are we overly dependent?” | Already includes Supplier purchasing dominance and Salesman concentration, but lacks Principal sales-out concentration. |

#### Questions that remain valid

- Customer credit, piutang, aging, collection, and customer concentration questions.
- Inventory health, stock-out, purchasing backlog, and Principal purchase/stock dependency questions.
- Salesman field execution, route compliance, and coaching questions.
- Warehouse and Wilayah concentration questions.

#### New management questions

- Which Principals are driving or missing company sales growth?
- Which Principals are below target, and is the cause Customer reach, sales mix, or Salesman coverage?
- Which Customers are growing or declining for each Principal?
- Which Customers buy one Principal but not another eligible Principal?
- Which Principal relationships are newly active, dormant, or at risk?
- Is purchasing and inventory aligned with Principal sales-out?
- Are Principal targets fully allocated to Salesmen, and do allocated totals reconcile?
- Which Salesmen execute each Principal, and where is assignment coverage thin?

### 4.4 Navigation impact

Current navigation correctly separates:

- Sales (company sales),
- Sales Force (people execution),
- Purchasing (Principal purchase/stock risk),
- Entity Analytics (cross-domain entity profiles).

What is missing is a discoverable Principal **commercial performance** path.

Viable navigation options:

| Option | Advantages | Disadvantages | Assessment |
| --- | --- | --- | --- |
| Add Principal Performance under Sales | Clearly separates sales-out from Purchasing; supports target/forecast/report chain. | Principal profile remains cross-domain elsewhere. | Best fit if the primary request is sales analytics. |
| Add a new Principals domain group | Gives a single Principal home spanning sales, purchasing, inventory, and relationships. | Risks duplicating PU01 and Entity Analytics; requires a new domain-level mental model. | Viable only with Product Owner approval. |
| Use Supplier Entity Analytics as the only Principal entry | Reuses existing entity shell and compare/workspace. | Current profile is purchase-led; weak as a management dashboard and not in the Sales journey. | Insufficient by itself. |
| Replace Sales Force with Principals | Superficially simple. | Invalidates field execution and coaching navigation that is correctly Salesman-grained. | Reject. |

Recommended direction for future planning: preserve Sales Force and Purchasing; add Principal commercial performance under Sales, while making Supplier/Principal Entity Analytics the cross-domain entity detail.

Existing code reservations must be reconciled before assigning a menu code:

- Current navigation uses `EX03` for Entity Analytics, while an older navigation specification reserves `EX03` for a future Business Health page.
- Current navigation uses `SF02` for Sales Force Overview and `SF03` for Salesman Field Activity, while an older specification reserves `SF03` for Sales Force Effectiveness and `SF04` for Salesman Report.
- A prior migration feasibility draft proposes `SF04` for Principal Performance.

No new code should be selected until the navigation registry and roadmap reservations are made canonical.

### 4.5 Entity Analytics impact

Supplier is already first-class, but the existing implementation is not ready to become the commercial spine:

- Its producer runs under `PurchasingManagement`.
- Its primary performance KPI is MTD Purchase.
- Its Growth axis is purchase growth.
- Its evidence routes lead to Purchasing and Inventory reports.
- It does have sales-derived Top Customers, Top Salesmen, and Top Items relationships.

Adding Principal sales-out KPIs raises a producer-ownership issue. A second worker writing the same Supplier entity type cannot safely call replace semantics independently without an approved merge/composition rule. Historical Supplier replay is also purchasing-led and would need sales evidence.

The current relationship metadata also needs correction: `TopCustomersByOmzet` contains sales-line omzet but names `PU-KPI-001` (MTD Purchase) as its metric KPI. That mismatch would make a Principal sales migration semantically unsafe even though the relationship values themselves are derivable.

Customer Entity Analytics requires immediate semantic correction if the proposed model is accepted:

- `AssignedSalesman` is not supported by master data.
- It is derived from one latest Faktur and discards multiple concurrent Salesmen.
- The Customer `Salesman` dimension and related links can mislead users.
- `TopPrincipalsByOmzet` is a valid observed relationship but currently only Top-N/MTD, not a full pair history.

### 4.6 Should Customer–Principal become first-class?

**No as a new Entity Analytics entity type at present; yes as a first-class analytical relationship/projection.**

Rationale:

- It is necessary to answer Principal-specific Customer growth, dormancy, reach, concentration, and cross-sell questions.
- Current L4 relationships only retain Top-N monthly links. They cannot represent the full Customer × Principal population, lifecycle, history, or pair KPIs.
- The pair has no independent master identity, contract, or pre-purchase assignment owner (GAP-003).
- The current Entity Analytics contract expects a stable single entity ID; a composite pair would add high-cardinality L0–L5 snapshots without a proven need for pair radar/ranking/compare.
- Transaction-derived relationships must not be relabeled as commercial ownership or pre-purchase assignment.

The analytical definition is now approved:

```text
Customer–Principal relationship (GAP-003) =
Customer has purchased one or more Items belonging to Principal,
derived from qualifying non-void Faktur item sales
within an approved historical window and attribution policy.
```

Pre-purchase manual Customer–Principal assignment is **not permitted**. All Customer–Principal analytics and portfolio calculations must be derived from sales transaction history.

### 4.7 ReportingContext and projection impact

| Area | Impact |
| --- | --- |
| Sales snapshot | Add Principal sales-out decomposition and reconciliation; preserve company totals and Salesman contribution. |
| Salesman snapshot | Keep people performance; reword portfolio signals; retain Salesman × Principal target/achievement as allocation/execution evidence. |
| Customer snapshot, risk, and portfolio | Remove singular ownership assumption; add relationship inputs where approved; revise Salesman-derived risk context. |
| Purchasing Management snapshot | Keep purchase/inventory semantics. It may provide Principal identity and cross-risk inputs but must not own all sales-out calculations by implication. |
| Entity Analytics producers | Define one authoritative way to compose Supplier KPIs from Sales, Purchasing, and Inventory workers without snapshot replacement loss. |
| Entity Analytics history/backfill | Add historical Principal sales-out and pair history using Item-master attribution (GAP-004). |
| Alert Center and Executive composition | Add Principal sales signals with category/deduplication policy. |
| Reports | Sales evidence needs line/Principal-compatible grain. Piutang and collection remain Customer/Faktur grain (GAP-005). |
| Refresh ordering | Cross-domain Principal health may depend on Sales, Purchasing, Inventory, and relationship projections; freshness semantics must be explicit. |

### 4.8 Database and core reporting-model impact

The core schema can support a current-period Principal sales-out view but not all requested semantics.

Required analytical capabilities:

- Principal sales-out current and monthly history.
- Principal target aggregation from Salesman × Principal rows.
- Principal × Salesman coverage and achievement.
- Full Customer × Principal relationship metrics and history.
- Reconciliation metadata for line-derived versus header-derived totals.
- Data-quality outputs for unassigned sales, unknown Principal, changed item Supplier, and mixed-Principal Fakturs.

Potential core-model changes cannot be selected in this assessment. Decisions are needed on:

- Principal target authority versus summed Salesman allocations.

Customer–Principal relationship semantics are resolved by GAP-003: transaction-derived only; no master assignment.

Item–Principal attribution semantics are resolved by GAP-004: one Principal per Item via master data; immutable for analytics; no invoice-time Principal snapshot in this initiative.

Principal financial attribution is resolved by GAP-005: AR, open balance, collection performance, and credit exposure remain Customer-level; no open-balance allocation across Principals.

Historical Salesman–Principal responsibility is resolved by GAP-006: monthly `BTR_SalesPersonPrincipalTarget` records; no effective-dated assignment model in this initiative.

Principal **sales** line-total to Faktur-header reconciliation is **resolved by GAP-019**: line-item amounts are the authoritative Principal sales measure; header-level totals are not allocated across Principals and reconciliation to Faktur-header `GrandTotal` is not required.

### 4.9 Security and integration impact

There is no existing Principal row-level security model to “reuse.” All authenticated users currently share portal navigation/data access. If Principal visibility is scoped by Salesman responsibility, user-to-Salesman identity, monthly target-based responsibility (GAP-006), management override, and cross-domain visibility rules become new security requirements.

External-system impact is unproven. BTrade/mobile and sync flows use Salesman and item data, but this assessment found no evidence that they persist a Customer–Principal owner relationship. Any operational enforcement of Principal assignment would require a separate integration assessment.

### 4.10 Future roadmap impact

| Roadmap area | Impact |
| --- | --- |
| M25 Sales Force Effectiveness | Preserve; it measures execution. Add Principal context only after order/item assignment semantics are validated. |
| M32.12 future entity types | Reprioritize only if new entity types remain valuable. Customer–Principal is not a normal master entity pack and needs relationship-grain platform support. |
| Principal Growth Planning | Directionally aligned. It depends on Principal sales-out history, target authority, active-Principal scope, and allocation/reconciliation decisions. |
| Customer forecasting/optimization/portfolio | Revisit single-Salesman fields and add Principal-specific decline/activation only after pair history exists. |
| Executive/Alert enhancements | Add Principal commercial signals only after KPI ownership and deduplication are settled. |
| Advanced collection analytics | Principal collection/credit analytics remain Customer-level only (GAP-005). |

---

## 5. Gap Analysis

| Gap ID | Type | Status | Description |
| --- | --- | --- | --- |
| GAP-001 | Business | **Resolved** | Salesman–Principal responsibility cardinality and meaning are now defined as many-to-many commercial portfolio assignment. See section 5.1. |
| GAP-002 | Business | **Resolved** | Commercial attribution dimensions are unified under the portfolio owner on the transaction; field activity is a separate operational dimension. See section 5.2. |
| GAP-003 | Data | **Resolved** | Customer–Principal is transaction-derived from sales history; no master assignment record. See section 5.3. |
| GAP-004 | Data | **Resolved** | Item–Principal attribution uses immutable Item master; no invoice-time Principal snapshot in this initiative. See section 5.4. |
| GAP-005 | Data | **Resolved** | Financial metrics remain Customer-level; no Principal open-balance allocation. See section 5.5. |
| GAP-006 | Data | **Resolved** | Historical Salesman–Principal responsibility uses monthly target records; no effective-dated assignment model. See section 5.6. |
| GAP-007 | Data | **Resolved** | Production profiling is implementation validation; approved assumptions allow planning to proceed. See section 5.7. |
| GAP-008 | Functional | **Resolved** | Principal sales performance dashboards, target tracking, and attention capabilities are reoriented as part of the migration, not introduced as a prerequisite capability set. See section 5.8. |
| GAP-009 | Functional | **Resolved** | Customer × Principal lifecycle/portfolio analytics are reoriented as part of the migration; no parallel capability set or master data model is introduced. See section 5.9. |
| GAP-010 | Functional | **Resolved** | Supplier/Principal Entity Analytics reoriented to represent sales-out performance of Principal products in the market; purchase-in analytics remain valid but outside Principal-centric performance scope. See section 5.10. |
| GAP-011 | Technical | **Resolved** | Principal Entity Analytics remains the authoritative aggregation layer; cross-worker KPI composition is an implementation concern. See section 5.11. |
| GAP-012 | Technical | **Resolved** | Historical Principal attribution uses Item master per GAP-004; invoice-time persistence is out of scope for this initiative. |
| GAP-013 | UX | **Resolved** | Principal commercial performance must have a clear primary navigation path; detailed navigation structure is an implementation planning concern. See section 5.12. |
| GAP-014 | UX | **Resolved** | Customer pages and Entity Analytics shall not imply exclusive ownership based on a single displayed Salesman; interpreted as commercial attribution, not ownership. See section 5.13. |
| GAP-015 | Reporting | **Resolved** | Principal sales attribution is calculated from Faktur line items, not Faktur header totals; mixed-Principal Fakturs are supported by line-item aggregation. See section 5.14. |
| GAP-016 | Catalog | **Resolved** | KPI entity classification framework redesign is out of scope; existing classifications used where practical. See section 5.15. |
| GAP-017 | Governance | **Resolved** | Principal is the standard user-facing term; Supplier retained technically where practical; no distinction recognized. See section 5.16. |
| GAP-018 | Security | **Resolved** | Principal-scoped authorization not introduced; Principal is an analytical dimension, not a security boundary. See section 5.17. |
| GAP-019 | Quality | **Resolved** | Principal sales are line-item authoritative; header totals not allocated; reconciliation to Grand Total not required. See section 5.18. |
| GAP-020 | Functional | **Resolved** | Principal performance measured using Sales-Out (DPP); sales and returns are independent KPIs; returns do not reduce the authoritative Sales KPI; tax/freight/rounding excluded. See section 5.19. |
| GAP-021 | Technical | **Resolved** | Principal `SalesOutAmount` established as first-class measure from transaction history; persistence strategy is implementation concern. See section 5.20. |
| GAP-022 | Quality | **Resolved** | Principal omzet and performance measurements realigned to sales-out KPIs from customer transactions; metadata implementation is planning concern. See section 5.21. |
| GAP-023 | Governance | **Resolved** | Navigation codes governed by single authoritative registry; conflicts resolved during implementation planning; no feasibility impact. See section 5.22. |

### 5.1 GAP-001 — Salesman–Principal responsibility cardinality and meaning

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | BQ-001, BQ-002 (cardinality) |

**Decision**

The relationship between Salesman and Principal (Supplier) is officially defined as **many-to-many**.

- A Salesman may be responsible for multiple Principals.
- A Principal may be managed by multiple Salesmen.
- This relationship represents **commercial responsibility for a Principal portfolio**, not exclusive ownership of Customers.
- Customer relationships are independent of Salesman ownership. A Customer may purchase products from multiple Principals and therefore interact with multiple Salesmen through those Principal relationships.

**Architectural consequences**

1. Salesman is **not** the primary ownership dimension of the business.
2. Principal becomes the **primary commercial responsibility dimension**.
3. Salesman analytics must be interpreted as **performance within the Principals assigned to that Salesman**.
4. Customer analytics remain **customer-centric** and are not owned exclusively by a single Salesman.
5. Future dashboard and KPI designs **must not assume** a 1:1 or 1:N Salesman → Principal relationship.

**Planning implications**

- Preserve Salesman field-activity surfaces on performer attribution; align commercial KPIs to portfolio-owner attribution (GAP-002).
- Design Principal commercial dashboards, targets, and rankings at Principal grain first; show Salesman contribution as a secondary decomposition within assigned Principals.
- Remove or relabel any Customer analytics that imply a single Salesman owner.
- Treat `BTR_SalesPersonPrincipalTarget` as the authoritative historical responsibility source per GAP-006; use `BTR_SalesPersonSupplier` only where current eligibility is needed, without implying exclusivity.

### 5.2 GAP-002 — Commercial attribution versus operational execution

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision timestamp | 2026-09-07T20:30:00+07:00 |
| Resolves | GAP-002, BQ-010 |

**Decision**

The Salesman recorded on a transaction represents the **commercial owner of the portfolio**.

Commercial Ownership, Customer Responsibility, Invoice Attribution, Sales Target Attribution, Sales Performance Attribution, and Bonus Attribution are treated as the **same business dimension** and are sourced from the portfolio owner.

Operational execution may occasionally be performed by another Salesman for coverage purposes. Such execution does **not** transfer portfolio ownership, revenue attribution, target achievement, performance attribution, or bonus eligibility.

Field Activity metrics are attributed to the Salesman who actually performs the activity and are considered a **separate operational measurement dimension**.

**Architectural consequences**

1. **Two Salesman measurement dimensions** exist: commercial (portfolio owner on transaction) and operational (field performer).
2. Faktur `SalesPersonId` is the authoritative source for all commercial Salesman attribution KPIs.
3. Field activity, route compliance, visit completion, and coaching KPIs use the **performer** dimension, not the portfolio owner by default.
4. Coverage or substitute execution must be modeled explicitly when operational and commercial Salesmen differ; it must not silently reassign revenue, targets, or bonuses.
5. Customer analytics must not treat a single Salesman as exclusive master owner of the Customer account (GAP-001), but may use the portfolio owner on each transaction for commercial responsibility on that sale.

**Planning implications**

- Classify every Salesman KPI as either **commercial-attribution** or **field-activity** grain before migration.
- Preserve and strengthen SF02/SF03 field-activity surfaces on performer attribution.
- Align SA01, SF01, target/achievement, bonus, and invoice-attributed piutang KPIs to portfolio-owner semantics.
- Introduce explicit coverage/substitute reporting only if operational evidence exists; do not infer commercial reassignment from visits or routes alone.
- Rename misleading labels such as "Assigned Salesman" to portfolio-owner or commercial-attribution wording where the source is Faktur `SalesPersonId`.
- Entity Analytics Customer relationships that imply singular ownership require rework per GAP-001 and GAP-002 together.

### 5.3 GAP-003 — Customer–Principal relationship definition

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-003, BQ-003 |

**Decision**

Customer–Principal relationships are **transaction-derived**.

A Customer is considered related to a Principal when the Customer purchases one or more Items belonging to that Principal.

The relationship is **not maintained as independent master data** and does not require a separate Customer–Principal assignment record.

Management cannot manually assign a Customer to a Principal before any purchase occurs. Therefore, all Customer–Principal analytics and portfolio calculations shall be derived from **sales transaction history**.

**Architectural consequences**

1. Customer–Principal is a **derived analytical relationship**, not a master-data entity or pre-purchase assignment.
2. Existing `TopPrincipalsByOmzet` and line-rollup patterns align with the approved model but must be extended to full history/portfolio scope where required; GAP-009 is resolved as a reorientation scope, not a future-add capability.
3. Option D (authoritative Customer–Principal master assignment) is **not** the target state for this migration.
4. Principal reach, dormancy, cross-sell, and portfolio-gap analytics must use transaction evidence and approved historical windows; they cannot rely on intended or planned assignments.
5. No UI, workflow, or integration should introduce pre-purchase Customer–Principal linking.

**Planning implications**

- Build Customer–Principal projections from Faktur item sales with explicit attribution and historical-window policy.
- Do not add `CustomerPrincipal` master tables or manual assignment screens unless a future separate domain initiative overturns GAP-003.
- Extend Customer and Supplier Entity Analytics relationship materialization beyond Top-N where portfolio questions require full pair population.
- Principal-specific Customer decline/dormancy KPIs depend on pair history implementation; the active-window policy is resolved by TQ-006 (Active = transaction within previous 6 months; Dormant = no transaction within previous 6 months; history retained indefinitely).
- Reject any design that treats transaction-derived purchase history as commercial ownership or pre-sale commitment.

### 5.4 GAP-004 — Item–Principal attribution and historical reporting

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-004, GAP-012 |

**Decision**

An Item belongs to **exactly one Principal**. Principal ownership of an Item is treated as **immutable for analytics and historical reporting purposes**.

Historical Principal attribution shall continue to be derived from the **Item master**. No invoice-time Principal snapshot will be introduced in the current initiative.

**Assumption**

Principal reassignment for existing Items is **not a supported or expected business scenario**. If such a requirement emerges in the future, historical attribution preservation will be evaluated separately.

**Architectural consequences**

1. `FakturItem → Brg.SupplierId` is the approved historical and current Principal attribution path.
2. No `SupplierId` column on `BTR_FakturItem` and no invoice-time Principal persistence are required for this migration.
3. Historical replay/backfill may use existing item-master joins without waiting for schema extension.
4. Principal sales-out, Customer–Principal, and Supplier relationship analytics share the same Item–Principal source rule.
5. Future Item Principal reassignment, if ever approved, is a **separate initiative** with its own attribution-preservation design.

**Planning implications**

- Implement Principal line attribution through `Brg.SupplierId` only; do not scope invoice-time Principal snapshots.
- Document the immutability assumption in KPI definitions, evidence routes, and migration disclosures.
- Run TQ-003 as a validation check to confirm reassignment is rare or absent; treat violations as data-quality exceptions, not a blocker to Item-master attribution.
- Remove historical-accuracy caveats that assume invoice-time Principal was required once GAP-004 is accepted.
- Do not block Principal dashboard or backfill work pending Faktur-item schema changes.

### 5.5 GAP-005 — Principal financial metrics and open-balance scope

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-005, BQ-007 |

**Decision**

Accounts Receivable, Open Balance, Collection Performance, and Credit Exposure are **Customer-level financial metrics** and shall **not** be attributed to individual Principals.

Principal analytics are limited to **sales, growth, product, portfolio, and market-performance measurements** derived from sales transactions.

No allocation of invoice-level open balances across multiple Principals will be introduced in the current initiative.

**Architectural consequences**

1. Piutang, aging, DSO, recovery, collection quality, and credit exposure remain on **Customer, Faktur, or company** surfaces only.
2. Principal dashboards, Entity Analytics, and KPI catalog additions must not publish Principal receivable or collection KPIs.
3. Mixed-Principal Fakturs do not require open-balance split logic; only **sales-out** reconciliation may still be required (GAP-019).
4. Finance dashboards (FI01–FI04) require semantic correction for Salesman attribution but not Principal financial decomposition.
5. Future Principal financial analytics, if ever required, is a **separate initiative** with its own allocation design.

**Planning implications**

- Scope Principal migration to sales, growth, portfolio, and market-performance KPIs only.
- Explicitly mark Principal piutang/collection/credit KPI proposals as out of scope.
- Preserve and strengthen Customer/company financial analytics without Principal filters or decompositions.
- Do not design open-balance allocation rules, payment split logic, or Principal credit limits in this plan.
- Separate sales reconciliation (line totals vs Faktur `GrandTotal`) from financial attribution in documentation and implementation.

### 5.6 GAP-006 — Historical Salesman–Principal responsibility

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-006 |

**Decision**

Historical Salesman–Principal responsibility shall be derived from **`BTR_SalesPersonPrincipalTarget`**.

Responsibility is managed as a **monthly planning construct** (`TargetYear`, `TargetMonth`) rather than as a separate effective-dated assignment relationship.

A Salesman is considered responsible for a Principal during periods where a **Principal Target record exists** for that Salesman and Principal combination.

No additional effective-dated Salesman–Principal assignment model will be introduced in the current initiative.

**Architectural consequences**

1. Monthly target rows are the **approved historical responsibility evidence** for Salesman × Principal analytics.
2. `BTR_SalesPersonSupplier` is not the source of truth for period responsibility; it may still support current eligibility or operational views but must not override target-based history.
3. Achievement, coverage, ranking, and coaching analytics for a period must join sales evidence to target presence for that `TargetYear`/`TargetMonth`.
4. No effective-dated assignment tables, history tables, or backfill of assignment effective dates are required for planning.
5. Future effective-dated assignment, if ever required, is a **separate initiative**.

**Planning implications**

- Use `BTR_SalesPersonPrincipalTarget` for historical filters, coverage checks, and responsibility-scoped Principal analytics.
- Align TQ-002 and data-quality rules to test Faktur Salesman × Principal pairs against **target records for the transaction month**, not only `BTR_SalesPersonSupplier`.
- Label UI and KPI metadata clearly as **target-based monthly responsibility**, not permanent assignment or Customer ownership.
- Preserve existing achievement storage (`BTRPD_SalesmanPrincipalAchievement`) as execution evidence aligned with target grain.
- Do not scope effective-dated `BTR_SalesPersonSupplier` extensions in this migration plan.

### 5.7 GAP-007 — Production-data profiling and implementation validation

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-007 |

**Decision**

The current initiative will proceed using the **approved business assumptions** regarding Principal attribution, Salesman responsibility, and Item–Principal ownership.

Production data profiling is considered an **implementation validation activity** rather than a prerequisite business decision.

Mixed-Principal invoices, assignment exceptions, Item–Supplier changes, and other historical anomalies shall be assessed during **implementation planning and data validation**. Any material findings will be evaluated separately and will **not block** the Principal-centric redesign.

**Architectural consequences**

1. Feasibility and planning are no longer gated on pre-implementation production profiling results.
2. TQ-001 through TQ-005 and TQ-008 become **implementation validation outputs**, not blocking business decisions.
3. Data-quality handling for anomalies must be designed into implementation (Unknown/Unassigned, exception reporting, disclosure), but absence of profiling does not delay plan start.
4. Material production findings may trigger follow-on remediation workstreams without reversing the approved analytical model.
5. The approved decisions in GAP-001 through GAP-006 remain the governing business rules unless a separate decision overturns them.

**Planning implications**

- Begin implementation planning on approved attribution semantics; schedule production profiling as an early implementation workstream.
- Include mixed-Principal Faktur analysis, target-responsibility exception rates, Item `SupplierId` change checks, and reconciliation outlier review in the implementation plan validation phase.
- Define how dashboards and KPIs surface data-quality exceptions without blocking Principal sales-out publication.
- Treat any required remediation of master data or historical anomalies as scoped follow-up, not a feasibility blocker.
- Keep section 9 validation checks as mandatory implementation inputs even though they are no longer planning prerequisites.

### 5.8 GAP-008 — Principal sales performance dashboard and related capabilities

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-008 |

**Decision**

The Principal-centric initiative is a **migration of the primary analytical dimension from Salesman to Principal**, not the introduction of a separate set of Principal capabilities.

Existing Salesman-centric dashboards, KPIs, rankings, target tracking, performance monitoring, and attention-oriented capabilities shall be reviewed and, where appropriate, **reoriented to use Principal as the primary business dimension**.

No additional Principal-specific capability set is required as a prerequisite for the migration. Capability additions, removals, consolidations, or replacements shall be determined during implementation planning based on the final Principal-centric navigation and KPI model.

**Architectural consequences**

1. The initiative does not create a parallel Principal capability surface that must exist before Salesman-centric surfaces are migrated or retired.
2. Principal sales performance dashboards, forecast, attention list, and sales-out evidence reports are **outputs of the migration**, not prerequisites for it.
3. Existing Salesman-centric surfaces (SA01, SF01, etc.) will be evaluated for reorientation, replacement, or preservation during implementation planning.
4. The final Principal-centric navigation and KPI model will determine which existing capabilities are retained, modified, or removed.
5. Implementation planning must define the disposition of each existing Salesman-centric capability rather than assuming all Principal capabilities must be built from scratch.

**Planning implications**

- Frame GAP-008 as a migration scope question, not a new-build requirement.
- During implementation planning, inventory each existing Salesman-centric capability and decide its Principal-centric disposition: reorient, replace, consolidate, or remove.
- Do not block feasibility or planning on the absence of a standalone Principal sales dashboard; it will emerge from the reorientation work.
- Use section 4.1 (Dashboard disposition) as the starting inventory for capability decisions.
- The navigation registry and KPI catalog redesign will determine the final Principal-centric surface layout.

### 5.9 GAP-009 — Customer × Principal lifecycle/history/portfolio analytics reorientation

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-009 |

**Decision**

The Principal-centric initiative is a migration of the primary analytical dimension from Salesman to Principal.

Existing customer lifecycle, portfolio, retention, growth, penetration, and history analytics shall be reviewed and reoriented from a Salesman-centric perspective to a **Customer × Principal perspective** where business value justifies the capability.

No separate Customer × Principal capability set will be introduced in parallel with existing Salesman-centric capabilities. The implementation planning phase shall determine which existing capabilities are migrated, replaced, consolidated, or retired as part of the Principal-centric redesign.

No separate Customer–Principal master data model will be introduced. All Customer × Principal analytics shall continue to be derived from transaction history.

**Architectural consequences**

1. Customer lifecycle, portfolio, retention, growth, penetration, and history analytics are **reoriented**, not replaced by a parallel capability surface.
2. The Customer × Principal perspective replaces Salesman-centric ownership interpretations where justified by business value; implementation planning determines the final disposition of each capability.
3. No master assignment table or pre-purchase Customer–Principal relationship model is introduced; all analytics remain transaction-derived per GAP-003.
4. Existing Salesman-centric customer analytics that imply singular ownership or exclusive responsibility must be reworked to reflect portfolio-owner attribution (GAP-002) or removed where no longer valid.
5. The migration approach means customer-side surfaces evolve together with Principal-centric redesign rather than waiting for a separate Customer × Principal feature build.

**Planning implications**

- Treat GAP-009 as a reorientation scope during implementation planning, not a future-add capability.
- Inventory existing customer lifecycle, portfolio, retention, growth, penetration, and history analytics; decide disposition per surface: reorient to Customer × Principal, migrate, consolidate, or retire.
- Ensure reoriented analytics use the same transaction-derived evidence path approved by GAP-003 and GAP-004.
- Do not introduce parallel Customer × Principal master tables, assignment workflows, or pre-purchase linking during this initiative.
- Use section 4.1 (Dashboard disposition) and the Customer surface reviews (CU01–CU05) as the starting point for capability decisions.

### 5.10 GAP-010 — Supplier Entity Analytics sales-out versus purchase-in semantics

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-010 |

**Decision**

Within the Principal-centric redesign, Supplier/Principal Entity Analytics shall represent **sales-out performance of Principal products in the market** and shall not be interpreted as purchase-in performance from suppliers.

Existing Supplier-centric capabilities, KPIs, dashboards, and analytics shall be reviewed and reoriented where necessary to ensure that performance, growth, contribution, ranking, and portfolio measurements are based on **customer sales transactions**.

Purchase-in analytics remain valid operational capabilities but are outside the scope of Principal-centric performance measurement.

**Architectural consequences**

1. Supplier/Principal Entity Analytics primary performance KPIs, growth axes, and radar dimensions must be reoriented from purchase-in to sales-out metrics.
2. Existing purchase-in KPIs (MTD Purchase, purchase growth, purchase invoice count, posted percent) are preserved but reclassified as purchasing/procurement analytics, not Principal sales performance.
3. The Supplier Entity Analytics producer must incorporate sales-out evidence from the Sales domain; this is not a metadata relabeling and requires cross-domain data composition (**GAP-011 resolved**: composition mechanism is an implementation concern; Principal Entity Analytics remains the authoritative aggregation layer).
4. Sales-out relationship metrics (Top Customers, Top Salesmen, Top Items by item-line sales) already exist but are currently secondary to purchase-in KPIs; they become the primary Principal performance measures.
5. Navigation, evidence routes, and drill-down paths for Supplier/Principal surfaces must lead to sales-out evidence where the context is Principal commercial performance.

**Planning implications**

- Inventory all existing Supplier Entity Analytics KPIs, growth axes, radar dimensions, and relationship metrics; classify each as sales-out or purchase-in.
- Reorient Principal-facing Supplier Entity Analytics surfaces to sales-out as the primary performance view; retain purchase-in as a separate operational context.
- Ensure the Supplier Entity Analytics producer has access to sales evidence; resolve cross-domain composition ownership per GAP-011.
- Do not remove or degrade purchase-in analytics; relocate them to a purchasing-specific performance context where they remain operationally valid.
- Use the Principal-centric KPI catalog redesign (GAP-016) to formally separate sales-out and purchase-in KPIs by domain and grain.

### 5.11 GAP-011 — Principal Entity Analytics cross-worker KPI composition

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-011 |

**Decision**

Principal Entity Analytics shall remain the **authoritative aggregation layer** for Principal-centric KPIs and business measurements.

The composition of Principal KPIs from multiple workers, projections, or data producers is an **implementation concern** and is outside the scope of this feasibility assessment.

Implementation planning and architecture design shall determine the most appropriate mechanism for composing Principal analytics while preserving a **single authoritative Principal analytics view** for portal consumers.

**Architectural consequences**

1. The requirement for a single authoritative Principal analytics view is a **design constraint**, not an implementation blocker; the feasibility assessment does not resolve the specific composition mechanism.
2. Cross-worker KPI composition (Sales, Purchasing, Inventory contributing to Supplier/Principal Entity Analytics) is deferred to implementation architecture.
3. The current single-producer model (Purchasing-owned SupplierEntityAnalyticsProducer) is recognized as insufficient for sales-out KPI composition but is not a feasibility-level decision to restructure.
4. Historical replay, snapshot management, and refresh ordering for a multi-source Principal analytics view are implementation concerns.
5. Portal consumers shall see one Principal analytics profile regardless of how many workers contribute data; composition transparency is a technical design goal.

**Planning implications**

- Do not block feasibility or planning on the specific cross-worker composition mechanism.
- Include multi-source Principal Entity Analytics composition as an early implementation architecture workstream.
- Ensure the implementation plan addresses snapshot ownership, merge/replace semantics, and refresh ordering when multiple workers contribute to one Principal analytics profile.
- Preserve the requirement that the portal sees a single authoritative Principal view; no partial or conflicting profiles should be exposed to consumers.
- Use section 5.10 (GAP-010) as the semantic driver: sales-out and purchase-in are separate domains that may compose into one Principal analytics profile.

### 5.12 GAP-013 — Principal commercial performance navigation path

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-013 |

**Decision**

The Principal-centric initiative establishes Principal as the **primary commercial performance dimension** within the portal.

Navigation, dashboards, KPIs, rankings, attention-oriented capabilities, and analytical journeys shall be reviewed and reoriented where appropriate to ensure Principal commercial performance can be accessed through a **clear primary navigation path**.

The detailed navigation structure is an implementation planning concern and is outside the scope of this feasibility assessment.

**Architectural consequences**

1. Principal commercial performance must be discoverable through the portal navigation; it cannot be hidden behind Purchasing or Entity Analytics alone.
2. The detailed navigation placement (new domain group, repositioned menu item, or enhanced Entity Analytics entry) is deferred to implementation planning.
3. Existing navigation code reservations (EX03, SF03, SF04 per GAP-023) do not block this decision; the requirement is that Principal commercial performance has a primary path, not that a specific code is assigned now.
4. Navigation design must respect the dual-axis model (GAP-002): commercial portfolio performance and operational field execution remain separate journeys.
5. Sales Force navigation remains valid for execution and coaching; Principal navigation covers commercial portfolio performance, target, forecast, and relationship health.

**Planning implications**

- Require Principal commercial performance to have a clear, primary navigation path in the implementation plan; do not leave it discoverable only through cross-links or Entity Analytics drill-down.
- Evaluate navigation placement during implementation planning using the options assessed in section 4.4.
- Resolve navigation code reservation conflicts (GAP-023) before finalizing the navigation registry.
- Ensure the Principal navigation path does not duplicate or conflict with existing Purchasing (PU01) or Entity Analytics (EX03) surfaces.

### 5.13 GAP-014 — Customer pages and Entity Analytics Salesman ownership implication

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-014 |

**Decision**

Customer pages, dashboards, and Entity Analytics shall **not imply exclusive Customer ownership** based solely on a single displayed Salesman.

The Principal-centric initiative recognizes that Customer commercial activity may span multiple Principals and multiple Salesman responsibilities.

Existing Customer-centric capabilities shall be reviewed and updated where necessary to ensure that displayed Salesman information is interpreted as **commercial attribution or portfolio responsibility** rather than exclusive Customer ownership.

The detailed presentation and navigation changes are implementation planning concerns and are outside the scope of this feasibility assessment.

**Architectural consequences**

1. Customer Entity Analytics `AssignedSalesman` label and singular Salesman dimension (F-009, F-014) are **inconsistent** with the approved model; they must be reworked to reflect commercial attribution on transactions, not master ownership.
2. `CustomerLastFaktur` single-Salesman display (F-008) remains valid as a recency indicator but must not imply exclusive ownership; labeling must clarify it is the last-invoicing Salesman, not the Customer's assigned Salesman.
3. Customer portfolio analytics that use a single Salesman as filter or owner must be reinterpreted under GAP-002: the Salesman on each Faktur is the portfolio owner for that transaction, not for the Customer account.
4. Presentation changes are deferred to implementation planning; the feasibility-level requirement is that ownership implication is corrected, not the specific UI mechanism.

**Planning implications**

- Include Customer Entity Analytics `AssignedSalesman` semantic correction as a mandatory implementation item.
- Review all Customer pages (CU01–CU05) for single-Salesman ownership implication; correct labels and interpretation guidance.
- Use GAP-001 and GAP-002 as the governing rules: Customer is not owned by Salesman; Salesman attribution is per-transaction portfolio owner.
- Detailed UI and navigation changes are deferred to implementation planning.

### 5.14 GAP-015 — Principal sales attribution evidence grain

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-015 |

**Decision**

Principal sales attribution shall be calculated from **Faktur line items**, not from Faktur header totals.

Each Faktur Item is attributed to exactly one Principal through its associated Item master data. Mixed-Principal Fakturs are therefore supported by **aggregating sales at the line-item level**.

No allocation of Faktur header totals across Principals will be introduced in the current initiative.

**Architectural consequences**

1. Principal sales-out KPIs are derived from `FakturItem.Total` grouped by `Brg.SupplierId`; they are not derived from `Faktur.GrandTotal`.
2. Mixed-Principal Fakturs do not require header-level allocation; each line item contributes to its Principal independently.
3. Company-level sales totals remain header-derived; Principal line totals may not reconcile exactly to `Faktur.GrandTotal` due to discounts, tax, returns, and rounding. Reconciliation is **resolved by GAP-019**: Principal sales use line-item amounts exclusively; header totals are not allocated and reconciliation to `GrandTotal` is not required.
4. Reports that currently display Faktur-header `GrandTotal` as a Salesman-attributed total must be reinterpreted when filtered or decomposed by Principal; the line-item grain is the only evidence that supports Principal decomposition.
5. The existing `FakturPrincipalOmzetDal` line-aggregation pattern is consistent with this decision and may serve as the evidence basis.

**Planning implications**

- Use line-item aggregation as the standard evidence path for all Principal sales-out KPIs, dashboards, reports, and Entity Analytics.
- Do not allocate Faktur header totals across Principals; the line-item grain is the approved attribution path.
- Document the line-item to header reconciliation gap explicitly in KPI definitions and report footnotes; resolve reconciliation semantics per GAP-019.
- Preserve company-level totals from header `GrandTotal` for company surfaces; do not force company totals through line-item aggregation unless explicitly validated.

### 5.15 GAP-016 — KPI entity classification framework scope

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-016 |

**Decision**

The current initiative is limited to **migrating the primary analytical dimension from Salesman to Principal** and does not include a redesign of the KPI Entity Analytics classification framework.

Existing entity classifications shall continue to be used where practical for Principal-centric analytics. More advanced classifications involving Company, Transaction, Location, or Relationship analytical grains may be introduced in future enhancements if required by business capabilities.

The absence of these classifications is **not considered a blocker** for the Principal-centric redesign.

**Architectural consequences**

1. The existing four-entity forced classification (Customer, Salesman, Item, Supplier) remains the catalog structure for this initiative; it is imperfect but functional.
2. Company, Transaction, Location, and Relationship analytical grains are recognized as necessary for accurate classification but are deferred to future catalog enhancements.
3. Principal-centric KPIs will be classified under existing entity categories (Supplier, Customer, Salesman) where practical; new classification taxonomy is not required before Principal analytics can be delivered.
4. KPI metadata (source, grain, attribution) is documented in definitions rather than enforced by classification; implementation planning should ensure KPI definitions carry explicit grain information.
5. A future KPI taxonomy redesign may reorganize entities by analytical grain (Company, Transaction, Location, Relationship) instead of by business entity; this is a separate initiative.

**Planning implications**

- Do not block Principal-centric KPI or dashboard delivery on KPI classification framework redesign.
- Use existing entity categories for Principal KPIs; annotate KPI definitions with explicit grain and attribution metadata.
- Include KPI classification framework redesign as a future roadmap item if business capabilities require it.
- Ensure the KPI catalog carries sufficient metadata (grain, attribution, source domain) to support future reclassification without silent semantic change.

### 5.16 GAP-017 — Principal versus Supplier terminology governance

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-017 |

**Decision**

For the Principal-centric redesign, **Principal** is established as the standard business and user-facing term.

Existing technical artifacts, database structures, and identifiers that use the term **Supplier** may be retained where practical, but shall be interpreted as representing the same business concept.

Portal navigation, dashboards, KPIs, reports, documentation, and business communications shall use **Principal** consistently unless a specific technical context requires otherwise.

No separate distinction between Principal and Supplier is recognized within the scope of the current initiative.

**Architectural consequences**

1. All user-facing surfaces (dashboards, KPIs, navigation, reports, documentation) shall use **Principal** as the standard label.
2. Database table names, column names, identifiers, and technical artifacts may retain `Supplier`/`SupplierId` where renaming is impractical; these are implementation details, not business terminology.
3. No Principal/Supplier equivalence table, mapping, or translation layer is required; they are the same business concept.
4. `BTR_SalesPersonSupplier`, `BTR_SalesPersonPrincipalTarget`, `Brg.SupplierId`, and similar technical identifiers remain as-is; user-facing interpretation uses Principal.
5. Foundation knowledge artifacts (DOMAIN.md) may be updated to formalize the terminology standard if the Product Owner requests it.

**Planning implications**

- Use **Principal** consistently in all new and revised user-facing surfaces, KPI definitions, documentation, and business communications.
- Do not rename database objects or technical identifiers unless explicitly requested as part of a separate schema modernization effort.
- Ensure KPI metadata, dashboard labels, and report headers use Principal; technical column/table references may continue using Supplier identifiers.
- No separate Principal/Supplier equivalence logic is required in implementation.

### 5.17 GAP-018 — Principal-scoped authorization and visibility

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-018 |

**Decision**

The Principal-centric redesign **does not introduce Principal-scoped authorization**.

Principal is an **analytical and reporting dimension**, not a security boundary. Existing portal authorization and visibility rules shall remain unchanged unless future business requirements explicitly require Principal-level access restrictions.

The absence of Principal-scoped visibility controls is **not considered a blocker** for the Principal-centric migration.

**Architectural consequences**

1. All authenticated portal users retain their current data access; no Principal-level row security or data-scope restriction is introduced.
2. Principal is treated as a reporting/business dimension (like Item or Customer), not as a security boundary (like Company or Branch).
3. Principal-scoped authorization, if ever required, is a **separate initiative** with its own RBAC, data-scope, and user-to-identity design.
4. Navigation and KPI visibility remain governed by existing portal authorization; no Principal filter or scope is injected into the access model.

**Planning implications**

- Do not introduce Principal-level row security, menu visibility, or data-scope filtering during this initiative.
- Preserve existing portal authorization and visibility rules; Principal analytics are accessible to all authorized users.
- Include Principal-scoped authorization as a future roadmap item only if business requirements explicitly require it.
- Do not design or implement Principal-level access controls, role assignments, or data-scope logic in implementation planning.

### 5.18 GAP-019 — Principal sales-to-Faktur-header reconciliation policy

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-019 |

**Decision**

Principal sales performance shall be calculated from **Faktur line-item amounts** and attributed to Principals at the item level.

Faktur header-level values such as Grand Total, taxes, rounding adjustments, freight charges, and other invoice-level adjustments are **not considered Principal attribution measures** and shall not be allocated across Principals.

As a result, aggregated Principal sales totals are **not required to reconcile to Faktur header Grand Totals**. Principal analytics shall use line-level sales values as the authoritative source for sales-out performance measurement.

**Architectural consequences**

1. Principal sales-out KPIs use `FakturItem.Total` as the authoritative measure; `Faktur.GrandTotal` is not used for Principal attribution.
2. Tax, rounding, freight, discount, and other invoice-level adjustments are not split across Principals; they remain at the Faktur or company level.
3. Aggregated Principal line totals may differ from company `GrandTotal` totals; this is expected and does not indicate data error.
4. KPI definitions, dashboard footnotes, and report headers must disclose the line-item evidence grain and the expected gap with header totals.
5. Company-level surfaces may continue to use header `GrandTotal` where appropriate; the line-item grain is required only for Principal decomposition.

**Planning implications**

- Use line-level sales values as the authoritative source for all Principal sales-out KPIs, dashboards, and reports.
- Do not allocate Faktur header adjustments across Principals; disclose the expected line-to-header gap explicitly.
- Document the reconciliation gap in KPI definitions, evidence routes, and report footnotes.
- Preserve company-level totals from header `GrandTotal` for company surfaces; do not force company totals through line-item aggregation unless explicitly validated.

### 5.19 GAP-020 — Principal sales measure definition and returns model

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-020 |

**Decision**

Principal commercial performance shall be measured using **Principal Sales-Out (DPP)** at the Principal level.

Principal Sales-Out is defined as sales transaction amounts attributed to a Principal after deducting **commercial discounts** attributable to the sale, and **before tax**.

**Sales and Returns are independent KPIs.** Good Returns and Broken Returns are tracked separately and **do not automatically reduce** the authoritative Principal Sales KPI. Entity Analytics shall evaluate both dimensions independently through trends, rankings, attention signals, and comparison views. Return Rate (%) may be used as a quality indicator and radar axis.

Tax, freight, rounding adjustments, and other non-commercial invoice-level amounts are **not considered Principal sales performance measures** and shall be excluded from Principal performance calculations.

Returns shall be modeled as separate KPIs, including (where applicable):

- Good Return Amount
- Broken Return Amount
- Total Return Amount
- Return Rate (%)

The detailed calculation formulas for Principal KPIs shall be standardized during implementation planning and applied consistently across all Principal-centric analytics and dashboards.

**Architectural consequences**

1. Principal sales-out KPIs are computed as Sales-Out (DPP): line-item sales amounts after commercial discounts, before tax; they are **not** reduced by returns, claims, or other post-sale adjustments.
2. Returns (Good Return, Broken Return, Total Return, Return Rate) are **separate independent KPIs**; they may be used in rankings, attention signals, radar dimensions, relationship analytics, and quality indicators, but do not modify the authoritative Principal Sales KPI.
3. Tax, freight, rounding, and other invoice-level adjustments are excluded from Principal performance measures (consistent with GAP-019).
4. Return and claim attribution to a Principal requires a defined line-item or principal-identifiable relationship; the detailed formula is standardized during implementation planning.
5. All Principal-centric dashboards and KPIs must use the same Sales-Out (DPP) definition to avoid inconsistent numbers across surfaces.
6. Company-level sales totals and Principal Sales-Out may differ; the gap is expected and documented per GAP-019 reconciliation policy.

**Planning implications**

- Standardize the Principal Sales-Out (DPP) calculation formula during implementation planning and apply it consistently.
- Model Returns as independent KPIs; do not subtract returns from the authoritative Sales-Out measure.
- Define how returns are attributed to a Principal (via line-item, item master, or principal-identifiable sales evidence).
- Ensure tax, freight, and rounding are excluded from all Principal performance KPIs.
- Document the Sales-Out (DPP) and Returns KPI definitions in KPI metadata, dashboard documentation, and report footnotes.

### 5.20 GAP-021 — Principal SalesOutAmount as first-class performance measure

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-021 |

**Decision**

Principal `SalesOutAmount` is established as a **first-class Principal performance measure** derived from sales transaction history.

The authoritative source for Principal sales performance remains **transaction-level sales data attributed to Principals**. Historical trends, rankings, forecasts, and other Principal-centric analytics shall be based on this authoritative sales history.

The persistence, projection, caching, or historical storage strategy for Principal KPI values is an **implementation architecture concern** and is outside the scope of this feasibility assessment.

**Architectural consequences**

1. `SalesOutAmount` is no longer a transient in-memory value in Purchasing Management; it is a recognized Principal performance measure.
2. The authoritative source of Principal sales performance is transaction-level sales evidence (line-item attribution per GAP-015), not Purchasing Management in-memory computation.
3. Historical trends, rankings, and forecasts derive from the same authoritative sales history; they are not dependent on a specific projection implementation.
4. Whether Principal KPI values are persisted in a snapshot table, computed on demand, cached, or materialized in a projection is an implementation architecture decision.
5. The first-class measure status does not by itself prescribe the storage mechanism; implementation planning determines persistence strategy.

**Planning implications**

- Establish Principal `SalesOutAmount` as a first-class KPI computed from authoritative transaction-level sales history.
- Basis all Principal trends, rankings, forecasts, and Entity Analytics on transaction sales history, not transient in-memory values.
- Evaluate persistence, projection, caching, and historical storage strategy during implementation architecture design.
- Do not block feasibility or planning on the specific storage mechanism; it is an implementation concern.

### 5.21 GAP-022 — Principal omzet KPI metadata realignment

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-022 |

**Decision**

Within the Principal-centric redesign, Principal omzet and related performance measurements shall be defined using **Principal sales-out KPIs derived from customer sales transactions**.

Any existing KPI relationships, metadata mappings, or analytical references that associate Supplier/Principal omzet with **purchase-oriented KPIs** or **Salesman-oriented KPIs** shall be reviewed and realigned to the approved Principal sales-out performance model.

The detailed KPI metadata implementation is an implementation planning concern and is outside the scope of this feasibility assessment.

**Architectural consequences**

1. Supplier/Principal omzet relationship metadata must reference a Principal sales-out KPI, not a purchase KPI (e.g., PU-KPI-001) or Salesman KPI (e.g., SF-KPI-008).
2. Existing mismatched relationships (F-020: `TopCustomersByOmzet` referencing purchase KPI; other omzet relationships referencing Salesman KPI) must be realigned to the approved sales-out model.
3. The realignment covers KPI relationship metadata, evidence routes, and analytical references, not just display labels.
4. The specific KPI ID assignment and metadata update mechanism is an implementation planning concern.
5. Realigned metadata must be consistent across Entity Analytics relationship catalogs, evidence resolvers, and KPI catalog definitions.

**Planning implications**

- Audit all Supplier/Principal omzet relationship metadata and realign references to Principal sales-out KPIs.
- Replace purchase- and Salesman-oriented KPI references with the approved Principal sales-out KPI IDs.
- Ensure relationship catalogs, evidence resolvers, and KPI definitions are updated consistently.
- Include the detailed KPI metadata implementation in the implementation plan; it is an implementation concern, not a feasibility blocker.

### 5.22 GAP-023 — Navigation code registry governance

| Field | Value |
| --- | --- |
| Status | **Resolved** |
| Decision date | 2026-09-07 |
| Resolves | GAP-023 |

**Decision**

Navigation code identifiers (including EXxx, SFxx, and related capability codes) shall be governed by a **single authoritative navigation registry**.

Any conflicts between current registry definitions, roadmap reservations, implementation plans, or other artifacts shall be resolved during **implementation planning and documentation consolidation**.

Navigation code conflicts are considered documentation and governance issues and **do not affect the feasibility** of the Principal-centric redesign.

**Architectural consequences**

1. A single authoritative navigation registry is the source of truth for all navigation code identifiers; any other artifact (roadmap, implementation plan, draft) is not authoritative until consolidated.
2. Existing EX03/SF03/SF04 reservations conflicts (F-015, section 4.4) are documentation inconsistencies to be consolidated, not architectural blockers.
3. The navigation code assignment for the new Principal commercial performance path (GAP-013) is deferred until the registry is made canonical.
4. The portal navigation implementation must follow the consolidated registry; no new code should be selected before consolidation.
5. Resolution is an implementation planning and documentation responsibility, not a feasibility decision.

**Planning implications**

- Consolidate all navigation code definitions and reservations into a single authoritative registry during implementation planning.
- Resolve EX03, SF03, SF04 and related conflicts before assigning any new Principal navigation codes.
- Treat navigation code conflicts as documentation/governance work; they do not block the Principal-centric redesign.
- Ensure the single registry becomes the reference for portal menu implementation, roadmap, and feature documentation.

---

## 6. Solution Options

### Option A — Mechanical replacement

Replace Salesman rankings/filters with Principal and relabel Supplier as Principal.

**Advantages**

- Lowest apparent UI effort.

**Disadvantages**

- Breaks valid field/execution analytics.
- Misattributes piutang and payments.
- Hides reconciliation and historical-data defects.
- Conflates purchase-in with sales-out.

**Recommendation:** Reject.

### Option B — Add Principal commercial analytics but preserve all current semantics

Add Principal sales dashboards and pair analytics without correcting misleading Salesman ownership language.

**Advantages**

- Lower migration risk.
- Preserves existing user paths.

**Disadvantages**

- Creates two conflicting ownership models.
- Leaves Customer portfolio and Entity Analytics semantically inconsistent.
- Increases duplicate KPIs and navigation ambiguity.

**Recommendation:** Use only as a short validation/prototype boundary, not the target state.

### Option C — Dual-axis analytical model

Establish Principal as the commercial/product portfolio axis, Salesman (commercial) as the portfolio-owner transaction-attribution axis, Salesman (field) as the operational execution axis, Customer as independent account/credit axis, and Customer–Principal as a transaction-derived analytical relationship (GAP-003).

**Advantages**

- Fits the data that actually exists.
- Preserves useful Salesman field and coaching analytics.
- Enables Principal sales, planning, and portfolio questions.
- Avoids inventing Principal receivables.
- Makes attribution semantics explicit.

**Disadvantages**

- Requires major catalog, projection, report, navigation, and Entity Analytics redesign.
- Requires resolution of remaining blocking business decisions; production profiling occurs during implementation validation (GAP-007).

**Recommendation:** Preferred.

### Option D — Persist Principal as authoritative transaction/account ownership

Add invoice-time Principal, Customer–Principal assignments, effective-dated responsibility, and allocation rules as core operational concepts.

**Advantages**

- Strong future historical and operational integrity.
- Can eventually support governed Principal receivable/account views.

**Disadvantages**

- Changes the core business model and transactional workflows.
- Requires mobile/sync/Desktop impact analysis and data migration.
- Cannot be chosen by a feasibility assessment.

**Recommendation:** Evaluate separately only if the business requires authoritative ownership rather than observational analytics. **Customer–Principal master assignment is ruled out by GAP-003. Invoice-time Principal persistence is ruled out by GAP-004 for this initiative. Principal financial/open-balance allocation is ruled out by GAP-005. Effective-dated Salesman–Principal assignment is ruled out by GAP-006.**

---

## 7. Recommended Approach

Adopt **Option C, the dual-axis analytical model**, provided the blocking decisions are resolved.

At solution level:

- Preserve Customer/company credit and receivable truth.
- Preserve Salesman field-activity analytics on performer attribution (GAP-002).
- Align commercial Salesman KPIs to portfolio-owner attribution on Faktur (GAP-002).
- Promote Principal sales-out, target, growth, reach, coverage, and cross-domain portfolio health.
- Treat Customer–Principal as a complete, historical, **transaction-derived** analytical relationship rather than a Top-N link (GAP-003).
- Correct singular “Assigned Salesman” and “owned book” semantics; use portfolio-owner wording for commercial attribution.
- Separate Principal sales-out from purchase-in on dashboards, KPIs, radar axes, reports, and evidence.
- Require reconciliation from Principal line metrics to company Faktur metrics, with named exceptions.
- Do not introduce Principal financial, collection, or credit KPIs (GAP-005).

The following constraints are mandatory:

1. Salesman–Principal responsibility is many-to-many commercial portfolio assignment (GAP-001); do not design 1:1 or 1:N Salesman → Principal exclusivity.
2. Commercial Salesman attribution uses the portfolio owner on the transaction (GAP-002); field activity uses the performer (GAP-002).
3. Coverage execution must not reassign revenue, targets, performance, or bonus without explicit commercial rules.
4. Company and Customer piutang, open balance, collection performance, and credit exposure remain authoritative at Customer/company grain only (GAP-005).
5. Purchase growth may not be reused or relabeled as Principal sales growth.
6. Item–Principal attribution uses `Brg.SupplierId` from Item master; immutable for analytics; no invoice-time Principal snapshot in this initiative (GAP-004); validate immutability assumption with TQ-003.
7. New Principal KPIs need line-compatible evidence.
8. No exclusive Customer account ownership may be inferred from latest Faktur; commercial responsibility on a sale follows portfolio owner (GAP-002).
9. Customer–Principal relationships must be derived from sales transaction history only; no pre-purchase master assignment (GAP-003).
10. Principal analytics exclude AR, open balance, collection performance, and credit exposure; no open-balance allocation across Principals (GAP-005).
11. Historical Salesman–Principal responsibility uses monthly `BTR_SalesPersonPrincipalTarget` records; no effective-dated assignment model (GAP-006).
12. Principal analytics use a multi-KPI model: Sales (Sales-Out DPP), Returns, Inventory, and Target are independent KPIs; Returns do not reduce the authoritative Sales KPI and are analyzed separately (GAP-020, BQ-006).

---

## 8. Risks

| Risk | Impact | Probability | Mitigation |
| --- | --- | --- | --- |
| Business model is oversimplified; assignments are many-to-many or vary by office | High | Medium | GAP-001 confirms many-to-many cardinality; GAP-007 allows planning to proceed; validate office-specific rules during implementation profiling. |
| Historical Principal sales changes when `Brg.SupplierId` changes | Medium | Low | GAP-004 treats Item–Principal as immutable for analytics; GAP-007 schedules TQ-003 during implementation validation. |
| Principal totals do not reconcile to Faktur `GrandTotal` | High | High | Define line/header allocation and reconciliation policy before KPI approval. |
| Principal piutang/collection is published without valid allocation | High | Low | GAP-005 excludes Principal financial attribution; prohibit Principal AR/collection/credit KPIs in scope. |
| Customer dashboards continue implying one Salesman owner | High | Medium | GAP-002 defines commercial attribution from portfolio owner; GAP-001 prohibits exclusive Customer ownership. Correct labels and distinguish commercial vs operational dimensions. |
| Purchase growth is mistaken for sales growth in Supplier profiles | High | High | Use separate KPI names, source domains, axes, and evidence routes. |
| Supplier producer writes from multiple domains and loses metrics | High | Medium | Approve one cross-domain composition/merge ownership model before implementation planning. |
| Customer–Principal pair cardinality increases snapshot and backfill cost | Medium | High | Profile population/retention and define active relationship window before sizing. |
| Sales Force analytics are removed despite valid execution use | High | Medium | Preserve Salesman surfaces and reposition them explicitly. |
| Principal navigation duplicates Purchasing and Entity Analytics | Medium | High | Approve one navigation responsibility model and cross-link strategy. |
| Assignment data is incomplete or violated by actual Fakturs | High | Medium | GAP-006 defines target-based monthly responsibility; GAP-007 defers TQ-002 profiling to implementation validation with exception handling rather than plan blocking. |
| Historical targets cannot identify authoritative Principal goal versus allocations | High | Medium | Decide target authority and reconciliation rules. |
| Principal-scoped access leaks or hides data | High | Medium | Keep common access initially or design explicit RBAC/data scope separately. |
| KPI IDs/definitions are mutated and historical comparisons silently change | High | Medium | Version definitions; add new IDs where semantics change; preserve historical labels. |
| Roadmap work builds on obsolete ownership assumptions | Medium | High | Gate affected Customer, planning, and Entity Analytics roadmap items on approved decisions. |

---

## 9. Open Questions

**All questions in this section are resolved.** Business questions BQ-001–BQ-010 and blocking technical questions TQ-006, TQ-007, TQ-009, TQ-010 carry approved answers with no remaining blocking decisions. Implementation-validation TQs (TQ-001–TQ-005, TQ-008) are resolved as implementation-time data profiling and validation activities (GAP-007); no Product Owner decision is required. Operational questions OQ-001–OQ-004 carry approved answers.

### Business questions — blocking

| ID | Status | Question |
| --- | --- | --- |
| BQ-001 | **Resolved** | What does “Salesman is responsible for a Principal” mean: exclusive ownership, assignment eligibility, target allocation, or current work assignment? **Answer (GAP-001):** commercial responsibility for an assigned Principal portfolio; not exclusive Customer ownership. |
| BQ-002 | **Resolved** | Can one Salesman be responsible for multiple Principals and one Principal for multiple Salesmen? If yes, how is management accountability divided? **Answer (GAP-001):** yes, many-to-many; Salesman accountability is performance within assigned Principals, not divided Customer ownership. |
| BQ-003 | **Resolved** | Is Customer–Principal an observed purchase relationship or a maintained commercial assignment? **Answer (GAP-003):** transaction-derived purchase relationship only; not maintained master data; no manual pre-purchase assignment. |
| BQ-004 | **Resolved** | What is the authoritative Principal target: a Principal goal, or the sum of Salesman × Principal allocations? **Answer:** Principal Target is derived from the sum of Salesman allocations. No standalone Principal Target authority exists in Phase 1. Future planning initiatives may introduce an independent Principal planning model if required. |
| BQ-005 | **Resolved** | How should discounts, tax, claims, bonuses, and other header/line differences be allocated to Principal sales-out? **Answer:** Principal Sales-Out is defined as Sales-Out (DPP), calculated after commercial discounts and before tax. Tax is excluded from all Principal performance KPIs. Claims, rebates, and other post-sale adjustments remain separate KPIs and do not modify the authoritative Principal Sales-Out measure. |
| BQ-006 | **Resolved** | Must Principal analytics be gross sales, net of returns, or another approved sales measure? **Answer:** Separate Sales and Returns Measures — Principal Analytics shall treat Sales and Returns as independent KPIs. The authoritative Principal Sales KPI is Sales-Out (DPP). Good Returns and Broken Returns are tracked separately and do not automatically reduce the authoritative Sales KPI. Entity Analytics shall evaluate both dimensions independently through trends, rankings, attention signals, and comparison views. Return Rate (%) may be used as a quality indicator and radar axis. |
| BQ-007 | **Resolved** | Should receivable and collection remain Customer/company-only, or must the business define Principal allocation for mixed invoices and payments? **Answer (GAP-005):** remain Customer/company-only; no Principal allocation of open balances in this initiative. |
| BQ-008 | **Resolved** | Who uses Principal commercial analytics and who may see which Principals? **Answer:** Role-Based Commercial Visibility (Phase 1 Simplification) — Principal Analytics inherits the existing commercial analytics visibility model. Any user authorized to access commercial analytics may view all Principals. No Principal-specific authorization or ownership restrictions will be introduced in this initiative. The architecture should remain extensible for future Principal-level visibility rules if business requirements emerge. |
| BQ-009 | **Resolved** | Should “Principal” replace user-facing “Supplier” consistently while retaining `SupplierId` technically? **Answer (Principal vs Supplier Terminology):** Use **Principal** as the standard business term in all user-facing screens, dashboards, analytics, KPI catalogs, reports, and documentation. `SupplierId` remains the existing technical identifier in the database, codebase, APIs, and integrations. No technical renaming or database migration is required as part of this initiative. **Rule:** Principal = user-facing terminology; Supplier/SupplierId = internal technical terminology. |
| BQ-010 | **Resolved** | Which current Salesman decisions are true coaching/work-assignment decisions and must remain? **Answer (GAP-002):** field activity and coaching use the operational performer dimension; commercial decisions (revenue, target, performance, bonus) use the portfolio owner on the transaction. |

### Technical/data questions — blocking

| ID | Question |
| --- | --- |
| TQ-006 | **Resolved.** What active/history window defines a Customer–Principal relationship and its dormant state? **Answer:** A Customer–Principal relationship is established by the existence of at least one historical transaction between the Customer and the Principal. Relationship history is retained indefinitely and is never removed by inactivity. Relationship status is determined by the most recent transaction date: **Active Relationship** = last transaction within the previous 6 months; **Dormant Relationship** = no transaction within the previous 6 months. This rule applies consistently across Customer Coverage, Customer Lifecycle, Relationship Analytics, and Entity Analytics features. |
| TQ-007 | **Resolved.** Can Supplier Entity Analytics safely compose multiple domain inputs under one refresh/freshness contract, or does the producer model need an approved extension? **Answer:** Supplier (Principal) Entity Analytics may compose KPIs originating from multiple domains under a single Entity Analytics refresh and freshness contract. Each source domain remains the authoritative owner of its data and KPI calculations. Entity Analytics acts as a composition and presentation layer only. No producer-model extension is required for this initiative. Any future requirements for cross-domain orchestration, dependency management, or independent freshness guarantees shall be evaluated separately. |
| TQ-009 | **Resolved.** Which report grain will serve as evidence for Principal sales and pair KPIs? **Answer:** The authoritative evidence grain for Principal Sales and related commercial KPIs shall be the Invoice Item (Faktur Item) level. Principal ownership is determined from the sold item/product, and all Principal analytics must be traceable to the underlying Invoice Item records. Higher-level reports (Invoice, Daily, Monthly, Customer, Principal) are analytical aggregations only. Invoice Item remains the canonical audit and drill-down evidence source. |
| TQ-010 | **Resolved.** Which canonical KPI ID and semantics should replace the mismatched Supplier omzet relationship metadata? **Answer:** The existing Supplier omzet relationship metadata shall be updated to reference a dedicated Principal commercial KPI rather than legacy Salesman or Purchasing KPI identifiers. A single canonical Principal Sales-Out KPI shall be established as the authoritative commercial performance measure for Principal Entity Analytics, relationships, rankings, dashboards, and related reporting. All Supplier/Principal omzet relationship metadata shall reference this canonical KPI to ensure consistent semantics across the analytics platform. |

### Technical/data questions — implementation validation (GAP-007)

These are required during implementation planning and early build validation. They are **not** prerequisites for planning readiness. **All implementation-validation questions (TQ-001 through TQ-005, TQ-008) are resolved as implementation-time data profiling and validation activities.** Their answers influence implementation effort, data quality remediation, and migration planning, but do not affect the feasibility conclusion, target architecture, or approved Principal-centric analytics model. No Product Owner decision is required; the implementation team shall validate and document the findings during delivery.

| ID | Question |
| --- | --- |
| TQ-001 | **Resolved (implementation-time data profiling).** What percentage of non-void Fakturs contain items from multiple Principals? **Answer:** This is an implementation-time data profiling activity rather than an open design decision. The implementation team shall measure the percentage of non-void Fakturs containing items from multiple Principals and document the findings during delivery. The result does not affect the approved architecture because Principal attribution and evidence reporting are based on Invoice Item granularity (TQ-009). No additional business or architectural decision is required. |
| TQ-002 | **Resolved (implementation-time data profiling and data quality assessment).** What percentage of Faktur Salesman × item Principal pairs lack a matching `BTR_SalesPersonPrincipalTarget` record for the transaction month? **Answer:** This is classified as implementation-time data profiling and data quality assessment rather than an open design decision. The implementation team shall measure the percentage of Faktur Salesman × Principal transaction pairs that lack a matching `BTR_SalesPersonPrincipalTarget` record for the transaction month and document the findings. The result does not affect the approved Principal-centric architecture. Any identified gaps shall be handled through implementation-time validation, reporting, or data quality rules as appropriate. |
| TQ-003 | **Resolved (implementation-time data profiling and validation).** How often has an item's `SupplierId` changed, and is any historical source available? **Answer:** This question is classified as an implementation-time data profiling and validation activity. The answer influences implementation effort, data quality remediation, and migration planning, but does not affect the feasibility conclusion, target architecture, or approved Principal-centric analytics model. No Product Owner decision is required. The implementation team shall validate and document the findings during delivery. |
| TQ-004 | **Resolved (implementation-time data profiling and validation).** How closely does `SUM(FakturItem.Total)` reconcile to Faktur `GrandTotal`, by Faktur and period? **Answer:** This question is classified as an implementation-time data profiling and validation activity. The answer influences implementation effort, data quality remediation, and migration planning, but does not affect the feasibility conclusion, target architecture, or approved Principal-centric analytics model. No Product Owner decision is required. The implementation team shall validate and document the findings during delivery. |
| TQ-005 | **Resolved (implementation-time data profiling and validation).** How many Customers transact with more than one Salesman and more than one Principal per month/year? **Answer:** This question is classified as an implementation-time data profiling and validation activity. The answer influences implementation effort, data quality remediation, and migration planning, but does not affect the feasibility conclusion, target architecture, or approved Principal-centric analytics model. No Product Owner decision is required. The implementation team shall validate and document the findings during delivery. |
| TQ-008 | **Resolved (implementation-time data profiling and validation).** What is the expected Customer × Principal population and 36-month backfill volume? **Answer:** This question is classified as an implementation-time data profiling and validation activity. The answer influences implementation effort, data quality remediation, and migration planning, but does not affect the feasibility conclusion, target architecture, or approved Principal-centric analytics model. No Product Owner decision is required. The implementation team shall validate and document the findings during delivery. |

### Operational questions

Phase 1 operational impacts with approved resolutions (OQ-001 through OQ-004).

| ID | Question |
| --- | --- |
| OQ-001 | **Resolved.** Will existing Salesman dashboards run in parallel while terminology and Principal analytics are validated? **Answer:** Existing Salesman dashboards shall remain available. The Principal-centric initiative introduces additional Principal Analytics and does not replace Salesman Analytics. Salesman and Principal dashboards serve different analytical purposes and may coexist indefinitely. No temporary parallel-run validation period or dashboard replacement strategy is required. |
| OQ-002 | **Resolved.** Which team maintains Salesman–Principal assignments and resolves unassigned/invalid sales? **Answer:** Salesman–Principal assignment maintenance is owned by the Commercial/Sales Management function. The system shall identify and report unassigned or invalid Salesman–Principal mappings, but responsibility for reviewing and correcting assignment data remains with the business owners of the commercial structure. No additional governance model is required for this initiative; existing commercial master data ownership applies. |
| OQ-003 | **Resolved.** How will users distinguish Principal sales-out, purchase-in, inventory, and target metrics in training/SOPs? **Answer:** Principal Analytics shall use explicit KPI names and definitions rather than generic terms such as "Supplier Omzet" or "Principal Omzet". Training materials, SOPs, dashboards, KPI catalogs, and reports shall consistently distinguish: Principal Sales-Out, Principal Purchase-In, Principal Inventory, Principal Target, Principal Achievement, and Principal Returns. No additional governance or architectural changes are required. Consistent KPI naming and KPI catalog definitions are sufficient to prevent ambiguity. (Cross-reference: TQ-010 establishes the single canonical Principal Sales-Out KPI as the authoritative commercial performance measure.) |
| OQ-004 | **Resolved.** What historical limitations are acceptable to management? **Answer:** The Principal-centric initiative does not require perfect historical reconstruction. Current and future reporting periods shall be accurate according to the approved Principal attribution model. Historical periods shall be reconstructed using the best available data and may contain known limitations where complete attribution evidence is unavailable. Historical data quality limitations are acceptable provided they are documented and do not affect the accuracy of current and future analytics. |

### Required data-validation checks during implementation planning (GAP-007)

These checks are mandatory implementation inputs. Material findings are evaluated separately and do not block the approved redesign.

1. Distinct Salesmen per Customer by month and trailing 12 months.
2. Distinct Principals per Customer by month and trailing 12 months.
3. Distinct Principals per Faktur and distribution of mixed-Principal invoices.
4. Assignment coverage for every Faktur Salesman × item Principal pair against `BTR_SalesPersonPrincipalTarget` for the transaction month.
5. Principal line-total to Faktur-header reconciliation, including outliers.
6. Unknown/blank Supplier rates on sold items.
7. Target coverage and reconciliation by Salesman, Principal, and company.
8. Customer–Principal population size, activity, and retention-window sensitivity.
9. Evidence of item Supplier changes or legacy remapping.

---

## 10. Implementation Impact Inventory

This inventory identifies what planning must consider; it does not prescribe implementation slices.

### Backend / ReportingContext

| Component area | Expected impact |
| --- | --- |
| `DashboardSnapshotAgg/Services/DashboardSalesmanAggregator.cs` | Reposition Customer portfolio semantics; preserve allocation/execution metrics. |
| Customer, Customer Risk Forecast, Customer Portfolio aggregators/workers | Remove singular ownership inference and incorporate approved relationship semantics. |
| Sales aggregators/workers and forecast policies | Add Principal decomposition, target, trend, and forecast with reconciliation. |
| Purchasing Management aggregators/workers | Preserve purchase semantics; expose inputs for cross-domain Principal composition. |
| `EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsProducer.cs` | Major change in KPI source composition/ownership and sales evidence. |
| Supplier and Customer Entity Analytics registrars, producers, evidence resolvers, relationship catalogs | New semantics, KPI packs, relationships, evidence, and history; correct current sales-relationship KPI metadata. |
| Entity Analytics repository/engine seams | Potential pair-grain/full-relationship support beyond Top-N L4. |
| Entity Analytics historical replay/backfill loaders and bundles | Add Principal sales-out and Customer–Principal history with stated limitations. |
| Executive and Alert Center composers/registries | New Principal sales attention and deduplication. |
| Investigation registry and navigation metadata | New Principal and relationship evidence paths. |
| Sales, Customer, and Piutang report query/DAL contracts | Principal-compatible evidence and corrected Salesman labels. |

### Database

| Object/area | Expected impact |
| --- | --- |
| `BTRPD_Salesman*` snapshots | Preserve; revise semantics where they imply Customer ownership. |
| `BTRPD_SalesmanPrincipalAchievement` | Preserve as allocation/execution evidence; Principal-first read/index/history needs assessment. |
| Sales snapshot tables | Principal sales-out/target/forecast dimensional storage is absent. |
| Customer snapshot/risk/portfolio tables | Salesman scalar fields and relationship semantics require review. |
| Generic Entity Analytics L0–L5 tables | Can store Supplier KPIs; full pair analytics may need a new relationship KPI/history capability or composite entity policy. |
| Entity Analytics backfill checkpoint/replay | Additional source and volume impact. |
| Core `BTR_FakturItem` / item history | No invoice-time Principal persistence in this initiative (GAP-004). |
| `BTR_SalesPersonSupplier` | Current eligibility reference only; not historical responsibility source (GAP-006). |
| `BTR_SalesPersonPrincipalTarget` | Authoritative monthly historical responsibility source (GAP-006). |
| Customer–Principal analytical storage | New full-population relationship grain if approved. |
| Reconciliation/data-quality storage | New outputs for unknown, unassigned, mixed, and unreconciled data. |

### Frontend

| Component area | Expected impact |
| --- | --- |
| SA01/SA02 and a Principal commercial-performance surface | Principal contribution, target, trend, forecast, attention, and drill-down. |
| Sales report view/models/filters | Principal-compatible evidence grain and reconciliation disclosure. |
| CU01–CU05 views/components/filters | Remove ownership implication; add Principal relationship context where approved. |
| SF01 view, attention list, detail drawer, profile | Reword attribution/book semantics; retain coaching and Principal allocation. |
| EX01/EX02 | Principal sales attention and cross-domain routing. |
| Entity Analytics home/workspace/profile/compare | Supplier sales-vs-purchase separation and Customer–Principal relationship analysis. |
| `portalMenuRegistry.ts` and menu code docs | New Principal commercial entry if approved; preserve Sales Force. |
| Investigation query/filter services | Principal and pair identifiers/evidence filters. |

### Reports and integrations

| Area | Expected impact |
| --- | --- |
| Sales report | Highest impact: current Faktur grain is insufficient for Principal evidence. |
| Piutang/collection reports | Label correction only; Principal financial attribution excluded (GAP-005). |
| Desktop Supplier sales reports (`FakturPerSupplier`, `OmzetSupplier`) | Candidate evidence/reuse, subject to semantic and reconciliation validation. |
| BTrade/mobile/sync | No change for analytics-only scope; separate assessment if assignments become enforced operationally or Item Principal reassignment is introduced. |
| Worker scheduling/health | New cross-domain dependencies and freshness reporting. |

### Security

| Element | Expected impact |
| --- | --- |
| JWT authentication | No inherent change. |
| Role/menu visibility | New requirement only if audiences differ. |
| Principal row-level data scope | Entirely new if assignment-scoped visibility is requested. |
| User-to-Salesman mapping | Required only for assignment-scoped access; not established today. |

### Permanent knowledge artifacts requiring later synchronization

- `docs/foundation/DOMAIN.md` if relationship/responsibility definitions are approved.
- `docs/foundation/WORKFLOW.md` if commercial planning/accountability flow changes.
- `docs/features/btr-portal/btr-portal-domain.md`.
- `docs/features/btr-portal/btr-portal-architecture.md`.
- KPI catalog and entity classification.
- Business question catalog v3 and question navigation map.
- Navigation asset registry/playbook/specification.
- Dashboard feature artifacts for SA, CU, FI, SF, PU, and EX surfaces.
- Entity Analytics developer guide and authoritative roadmap.
- Principal Growth Planning feasibility/feature artifacts.

---

## 11. Planning Readiness

### Status

```text
READY
```

### Blocking issues

1. ~~The proposed responsibility cardinality is not validated and is not enforced by the schema.~~ **Resolved by GAP-001.** Historical responsibility derivation resolved by GAP-006.
2. ~~Customer–Principal has not been defined as observed behavior versus maintained commercial assignment.~~ **Resolved by GAP-003.** Transaction-derived only; active/history window policy resolved by TQ-006 (Active = transaction within previous 6 months; Dormant = no transaction within previous 6 months; history retained indefinitely).
3. ~~Principal sales amount and Faktur-header reconciliation rules are unresolved.~~ **Resolved by GAP-019.** Principal sales use line-item amounts exclusively; header totals are not allocated and reconciliation to `GrandTotal` is not required.
4. ~~Principal target authority and allocation rules are unresolved.~~ **Resolved by BQ-004.** Principal Target is derived from the sum of Salesman allocations; no standalone Principal Target authority exists in Phase 1.
5. ~~Principal receivable/collection scope is unresolved and currently unsupported.~~ **Resolved by GAP-005.** Financial metrics remain Customer-level; Principal financial KPIs are out of scope.
6. ~~Historical Principal attribution reliability is unknown.~~ **Resolved by GAP-004.** Item-master attribution approved; immutability assumption documented; TQ-003 remains a validation check.
7. ~~The Supplier Entity Analytics cross-domain producer ownership model is unresolved.~~ **Resolved by GAP-011 and TQ-007.** Principal Entity Analytics remains the authoritative aggregation layer; cross-domain composition under a single refresh/freshness contract is approved with each source domain authoritative for its own KPIs; no producer-model extension required.
8. ~~Production-data profiling required by section 9 has not been completed.~~ **Resolved by GAP-007.** Profiling is an implementation validation activity and is not a planning prerequisite.
9. ~~Navigation ownership and intended audience for Principal commercial analytics are unapproved.~~ **Resolved by GAP-013, GAP-018, GAP-023, and BQ-008.** Principal commercial performance requires a clear primary navigation path (GAP-013); detailed navigation structure is an implementation planning concern; navigation codes are governed by a single authoritative registry resolved during implementation planning (GAP-023); no Principal-scoped authorization is introduced and all authorized commercial users may view all Principals (GAP-018, BQ-008).

### Conditions for planning readiness

Planning may begin when:

- ~~BQ-004 through BQ-006 and BQ-008 through BQ-009 have approved answers.~~ **Resolved:** BQ-004 (target = sum of Salesman allocations, Phase 1), BQ-005 (Sales-Out DPP after commercial discounts, before tax), BQ-006 (Sales-Out DPP as authoritative measure; Sales and Returns are independent KPIs), BQ-008 (role-based commercial visibility; all Principals visible to authorized commercial users), BQ-009 (Principal is user-facing term; SupplierId remains technical). (BQ-001, BQ-002 resolved by GAP-001; BQ-003 resolved by GAP-003; BQ-007 resolved by GAP-005; BQ-010 resolved by GAP-002.)
- ~~TQ-001 through TQ-005 and TQ-008 have measured results.~~ **Resolved by GAP-007:** these are implementation validation activities, not planning prerequisites.
- A canonical KPI grain/attribution matrix is approved. (Commercial vs field-activity classification approved by GAP-002; Customer–Principal as transaction-derived relationship approved by GAP-003; Item–Principal via immutable Item master approved by GAP-004; Customer-only financial metrics approved by GAP-005; monthly target-based Salesman–Principal historical responsibility approved by GAP-006; production profiling deferred to implementation validation per GAP-007; technical matrix resolved by TQ-009 (Invoice Item is the canonical evidence grain), TQ-010 (single canonical Principal Sales-Out KPI), GAP-019 (line-item authoritative; no header reconciliation), GAP-020 (Sales-Out DPP as authoritative measure; sales and returns are independent KPIs), and BQ-005/BQ-006 (measure definition).)
- ~~A Principal sales-to-company reconciliation rule is approved.~~ **Resolved by GAP-019.** Line-item amounts are authoritative for Principal sales; header totals are not allocated and reconciliation to `GrandTotal` is not required.
- ~~Customer–Principal is approved as either observed analytical relationship or maintained domain relationship.~~ **Resolved by GAP-003:** transaction-derived analytical relationship only.
- ~~Principal collection KPIs are explicitly excluded or supported by an approved allocation rule.~~ **Resolved by GAP-005:** Principal financial/collection/credit KPIs are excluded; no open-balance allocation.
- ~~One navigation/product responsibility model is selected.~~ **Resolved by GAP-013.** Principal commercial performance requires a clear primary navigation path; the detailed navigation structure is an implementation planning concern, resolved against a single authoritative navigation registry (GAP-023).
- ~~Entity Analytics has an approved multi-domain Supplier composition boundary.~~ **Resolved by TQ-007 and GAP-011.** Multiple domain inputs compose under a single Entity Analytics refresh/freshness contract; source domains remain authoritative for their own KPIs; Entity Analytics acts as composition and presentation layer.

### Planner guidance

The future plan must cover:

- semantic correction before or together with new Principal capabilities;
- preservation of Salesman field/execution analytics;
- Principal sales-out evidence and reconciliation before executive promotion;
- relationship history before Principal-specific Customer decline/dormancy claims;
- versioned KPI definitions rather than silent mutation;
- explicit treatment of historical limitations and unknown/unassigned records;
- early implementation production profiling and data-validation per GAP-007;
- synchronized updates to permanent knowledge after accepted decisions.

The plan must not:

- treat purchase growth as sales growth;
- allocate piutang, open balance, or credit exposure to Principals (GAP-005);
- infer exclusive Customer account ownership from latest Faktur;
- remove Salesman execution surfaces;
- claim historical Principal accuracy requires invoice-time snapshots in this initiative;

---

## Appendix A — Evidence Index

### Permanent and work artifacts

- `docs/foundation/PRODUCT.md`
- `docs/foundation/DOMAIN.md`
- `docs/foundation/LANDSCAPE.md`
- `docs/foundation/WORKFLOW.md`
- `docs/features/btr-portal/btr-portal-domain.md`
- `docs/features/btr-portal/btr-portal-architecture.md`
- `docs/features/btr-portal/btr-portal-kpi-catalog.md`
- `docs/features/btr-portal/kpi-entity-classification.md`
- `docs/features/btr-portal/business-question-catalog-v3.md`
- `docs/features/btr-portal/navigation-assets.md`
- `docs/features/btr-portal/portal-navigation-ux-analysis.md`
- `docs/features/btr-portal/question-navigation-map.md`
- `docs/features/entity-analytics/entity-analytics-developer-guide.md`
- `docs/work/btr-portal/entity-analytics/entity-analytics-roadmap-authoritative.md`
- `docs/work/btr-portal/FEASIBILITY-ASSESSMENT-principal-growth-planning.md`

### Core schema and reporting source

- `btr.sql/Tables/SalesContext/BTR_Customer.sql`
- `btr.sql/Tables/SalesContext/BTR_Faktur.sql`
- `btr.sql/Tables/SalesContext/BTR_FakturItem.sql`
- `btr.sql/Tables/BrgContext/BTR_Brg.sql`
- `btr.sql/Tables/SalesContext/BTR_SalesPersonSupplier.sql`
- `btr.sql/Tables/SalesContext/BTR_SalesPersonPrincipalTarget.sql`
- `btr.sql/Tables/ReportingContext/BTRPD_SalesmanPrincipalAchievement.sql`
- `btr.infrastructure/SalesContext/FakturInfo/FakturPrincipalOmzetDal.cs`
- `btr.infrastructure/SalesContext/FakturInfoAgg/CustomerLastFakturDal.cs`
- `btr.infrastructure/ReportingContext/DashboardSnapshotAgg/SupplierMtdItemRollupDal.cs`
- `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardSalesmanAggregator.cs`
- `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardCustomerPortfolioAggregator.cs`
- `btr.application/ReportingContext/DashboardSnapshotAgg/Services/DashboardSupplierRelationshipAggregator.cs`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Producers/CustomerEntityAnalyticsProducer.cs`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Producers/SupplierEntityAnalyticsProducer.cs`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Registrars/CustomerRelationshipCatalog.cs`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Registrars/SupplierEntityAnalyticsRegistrar.cs`
- `btr.application/ReportingContext/EntityAnalyticsAgg/Registrars/SupplierRelationshipCatalog.cs`
- `btr.portal.web/src/navigation/portalMenuRegistry.ts`
- `btr.portal.web/src/router/index.ts`

## Appendix B — Final Decision Summary

| Question | Assessment |
| --- | --- |
| Is the portal wholly Salesman-centric? | No. It is mixed, but Sales, Sales Force, parts of Customer/Finance, KPI classification, and navigation questions embed Salesman-centric assumptions. |
| Is the proposed model proven? | Partially proven. Approved business decisions (GAP-001–GAP-023) and resolved open questions (BQ-001–BQ-010, TQ-006, TQ-007, TQ-009, TQ-010) support planning; sales reconciliation, target authority, navigation, and Entity Analytics composition were resolved during this assessment. |
| What is Salesman–Principal responsibility? | **Many-to-many commercial portfolio assignment (GAP-001).** Historical period responsibility is derived from monthly `BTR_SalesPersonPrincipalTarget` records (GAP-006). Principal is the primary commercial responsibility dimension; Customer is not owned by Salesman. |
| How are Salesman attribution dimensions defined? | **Commercial attribution is unified under the portfolio owner on the transaction (GAP-002, 2026-09-07T20:30:00+07:00).** Field activity is a separate operational dimension attributed to the performer. |
| Can Principal sales analytics be built? | Yes for line-derived current/historical sales. Attribution evidence grain is resolved (TQ-009: Invoice Item), sales measure is resolved (GAP-020, BQ-005, BQ-006: Sales-Out DPP as the authoritative measure; sales and returns are independent KPIs), reconciliation is resolved (GAP-019: line-item authoritative; no header reconciliation required). |
| Can Principal piutang/collection analytics be built safely now? | **No, and not in scope.** GAP-005 confines AR, open balance, collection performance, and credit exposure to Customer-level metrics. |
| What Principal financial metrics are in scope? | **None in this initiative (GAP-005, 2026-09-07).** Principal analytics are limited to sales, growth, product, portfolio, and market-performance from sales transactions. |
| Should Customer–Principal be first-class? | Not as a new Entity Analytics entity type now; yes as a first-class **transaction-derived** analytical relationship/projection with pair-scoped evidence and history (GAP-003). |
| What is the Customer–Principal relationship model? | **Transaction-derived from item sales history (GAP-003, 2026-09-07).** No master assignment; no pre-purchase manual linking. |
| How is historical Principal attribution determined? | **Item master (`Brg.SupplierId`); one Principal per Item; immutable for analytics (GAP-004, 2026-09-07).** No invoice-time snapshot in this initiative. |
| How is historical Salesman–Principal responsibility determined? | **Monthly `BTR_SalesPersonPrincipalTarget` records (`TargetYear`, `TargetMonth`) (GAP-006, 2026-09-07).** No effective-dated assignment model in this initiative. |
| Does production profiling block planning? | **No (GAP-007, 2026-09-07).** Proceed on approved assumptions; profile during implementation validation; material findings handled separately. |
| Should Salesman analytics be removed? | No. Reposition them around execution, attribution, allocation, and coaching. |
| Overall recommendation | GO with major redesign. |
| Planning readiness | READY. |
