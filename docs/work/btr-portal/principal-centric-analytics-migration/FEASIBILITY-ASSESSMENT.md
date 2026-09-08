# FEASIBILITY ASSESSMENT

## Principal-Centric Analytics Migration for BTR Portal

| Field | Value |
| --- | --- |
| Status | Analysis only; no implementation |
| Assessment date | 2026-09-07 |
| Requested decision | NO-GO / minor changes / moderate redesign / major redesign |
| Evidence boundary | Repository artifacts, source code, and SQL definitions at assessment time; production-data profiling is an implementation validation activity per GAP-007 |
| Planning handoff | Not ready until remaining blocking business and technical decisions in sections 9 and 11 are resolved |

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
- Company sales use Faktur-header `GrandTotal`, while existing Principal sales-out logic sums Faktur-item `Total`. Mixed-Principal Fakturs therefore need an explicit **sales** reconciliation policy (GAP-019 remains open).
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
| A-007 | Principal visibility should be limited by Salesman assignment. | Portal currently has no role/data-scope model, and intended audiences are unresolved. |

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

Explicitly excluded by GAP-005 (not in scope for this initiative):

- Principal piutang, overdue, aging, DSO, recovery, cash collection, credit exposure, and collection quality.
- Allocation of invoice-level open balances across multiple Principals.

Also excluded pending other decisions:

- Principal margin when cost/rebate/claim semantics are not approved.
- Net Principal sales after returns unless return attribution and period policy are defined.

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

Principal **sales** line-total to Faktur-header reconciliation remains open under GAP-019.

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
| GAP-008 | Functional | Open | No Principal sales performance dashboard, forecast, attention list, or sales-out evidence report exists. |
| GAP-009 | Functional | Open | No full Customer × Principal lifecycle/history/portfolio analytics exists. |
| GAP-010 | Functional | Open | Supplier Entity Analytics performance and growth currently mean purchase-in, not sales-out. |
| GAP-011 | Technical | Open | Supplier Entity Analytics has a single Purchasing-owned replace producer; cross-worker KPI composition is unresolved. |
| GAP-012 | Technical | **Resolved** | Historical Principal attribution uses Item master per GAP-004; invoice-time persistence is out of scope for this initiative. |
| GAP-013 | UX | Open | Principal commercial performance has no primary navigation path. |
| GAP-014 | UX | Open | Customer pages and Entity Analytics show one Salesman in ways that imply assignment/ownership. |
| GAP-015 | Reporting | Open | Sales Report Faktur grain cannot prove Principal line amounts on mixed-Principal Fakturs. |
| GAP-016 | Catalog | Open | KPI entity classification cannot represent Company, Transaction, Location, or Relationship grain accurately. |
| GAP-017 | Governance | Open | Principal/Supplier terminology is equivalent in foundation knowledge but labels vary by surface; commercial naming rules are not fixed. |
| GAP-018 | Security | Open | Principal-scoped visibility has no current authorization model. |
| GAP-019 | Quality | Open | Principal line-total sales and Faktur-header GrandTotal reconciliation policy is not defined. |
| GAP-020 | Functional | Open | Returns, claims, discounts, tax, and net-sales treatment at Principal grain are not approved. |
| GAP-021 | Technical | Open | Principal `SalesOutAmount` is transient in Purchasing Management and has no first-class KPI/history persistence. |
| GAP-022 | Quality | Open | Supplier omzet relationship metadata points to purchase/Salesman KPI IDs rather than a Principal sales-out KPI. |
| GAP-023 | Governance | Open | EX03/SF03/SF04 navigation code reservations conflict across current registry and roadmap artifacts. |

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
- Rename misleading labels such as “Assigned Salesman” to portfolio-owner or commercial-attribution wording where the source is Faktur `SalesPersonId`.
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
2. Existing `TopPrincipalsByOmzet` and line-rollup patterns align with the approved model but must be extended to full history/portfolio scope where required (GAP-009 remains open functionally).
3. Option D (authoritative Customer–Principal master assignment) is **not** the target state for this migration.
4. Principal reach, dormancy, cross-sell, and portfolio-gap analytics must use transaction evidence and approved historical windows; they cannot rely on intended or planned assignments.
5. No UI, workflow, or integration should introduce pre-purchase Customer–Principal linking.

**Planning implications**

- Build Customer–Principal projections from Faktur item sales with explicit attribution and historical-window policy.
- Do not add `CustomerPrincipal` master tables or manual assignment screens unless a future separate domain initiative overturns GAP-003.
- Extend Customer and Supplier Entity Analytics relationship materialization beyond Top-N where portfolio questions require full pair population.
- Principal-specific Customer decline/dormancy KPIs remain dependent on pair history implementation and TQ-006 active-window definition.
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

### Business questions — blocking

| ID | Status | Question |
| --- | --- | --- |
| BQ-001 | **Resolved** | What does “Salesman is responsible for a Principal” mean: exclusive ownership, assignment eligibility, target allocation, or current work assignment? **Answer (GAP-001):** commercial responsibility for an assigned Principal portfolio; not exclusive Customer ownership. |
| BQ-002 | **Resolved** | Can one Salesman be responsible for multiple Principals and one Principal for multiple Salesmen? If yes, how is management accountability divided? **Answer (GAP-001):** yes, many-to-many; Salesman accountability is performance within assigned Principals, not divided Customer ownership. |
| BQ-003 | **Resolved** | Is Customer–Principal an observed purchase relationship or a maintained commercial assignment? **Answer (GAP-003):** transaction-derived purchase relationship only; not maintained master data; no manual pre-purchase assignment. |
| BQ-004 | Open | What is the authoritative Principal target: a Principal goal, or the sum of Salesman × Principal allocations? |
| BQ-005 | Open | How should discounts, tax, claims, bonuses, and other header/line differences be allocated to Principal sales-out? |
| BQ-006 | Open | Must Principal analytics be gross sales, net of returns, or another approved sales measure? |
| BQ-007 | **Resolved** | Should receivable and collection remain Customer/company-only, or must the business define Principal allocation for mixed invoices and payments? **Answer (GAP-005):** remain Customer/company-only; no Principal allocation of open balances in this initiative. |
| BQ-008 | Open | Who uses Principal commercial analytics and who may see which Principals? |
| BQ-009 | Open | Should “Principal” replace user-facing “Supplier” consistently while retaining `SupplierId` technically? |
| BQ-010 | **Resolved** | Which current Salesman decisions are true coaching/work-assignment decisions and must remain? **Answer (GAP-002):** field activity and coaching use the operational performer dimension; commercial decisions (revenue, target, performance, bonus) use the portfolio owner on the transaction. |

### Technical/data questions — blocking

| ID | Question |
| --- | --- |
| TQ-006 | What active/history window defines a Customer–Principal relationship and its dormant state? |
| TQ-007 | Can Supplier Entity Analytics safely compose multiple domain inputs under one refresh/freshness contract, or does the producer model need an approved extension? |
| TQ-009 | Which report grain will serve as evidence for Principal sales and pair KPIs? |
| TQ-010 | Which canonical KPI ID and semantics should replace the mismatched Supplier omzet relationship metadata? |

### Technical/data questions — implementation validation (GAP-007)

These are required during implementation planning and early build validation. They are **not** prerequisites for planning readiness.

| ID | Question |
| --- | --- |
| TQ-001 | What percentage of non-void Fakturs contain items from multiple Principals? |
| TQ-002 | What percentage of Faktur Salesman × item Principal pairs lack a matching `BTR_SalesPersonPrincipalTarget` record for the transaction month? |
| TQ-003 | How often has an item's `SupplierId` changed, and is any historical source available? |
| TQ-004 | How closely does `SUM(FakturItem.Total)` reconcile to Faktur `GrandTotal`, by Faktur and period? |
| TQ-005 | How many Customers transact with more than one Salesman and more than one Principal per month/year? |
| TQ-008 | What is the expected Customer × Principal population and 36-month backfill volume? |

### Operational questions

| ID | Question |
| --- | --- |
| OQ-001 | Will existing Salesman dashboards run in parallel while terminology and Principal analytics are validated? |
| OQ-002 | Which team maintains Salesman–Principal assignments and resolves unassigned/invalid sales? |
| OQ-003 | How will users distinguish Principal sales-out, purchase-in, inventory, and target metrics in training/SOPs? |
| OQ-004 | What historical limitations are acceptable to management? |

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
NOT READY
```

### Blocking issues

1. ~~The proposed responsibility cardinality is not validated and is not enforced by the schema.~~ **Resolved by GAP-001.** Historical responsibility derivation resolved by GAP-006.
2. ~~Customer–Principal has not been defined as observed behavior versus maintained commercial assignment.~~ **Resolved by GAP-003.** Transaction-derived only; active/history window policy remains open (TQ-006).
3. Principal sales amount and Faktur-header reconciliation rules are unresolved.
4. Principal target authority and allocation rules are unresolved.
5. ~~Principal receivable/collection scope is unresolved and currently unsupported.~~ **Resolved by GAP-005.** Financial metrics remain Customer-level; Principal financial KPIs are out of scope.
6. ~~Historical Principal attribution reliability is unknown.~~ **Resolved by GAP-004.** Item-master attribution approved; immutability assumption documented; TQ-003 remains a validation check.
7. The Supplier Entity Analytics cross-domain producer ownership model is unresolved.
8. ~~Production-data profiling required by section 9 has not been completed.~~ **Resolved by GAP-007.** Profiling is an implementation validation activity and is not a planning prerequisite.
9. Navigation ownership and intended audience for Principal commercial analytics are unapproved.

### Conditions for planning readiness

Planning may begin when:

- BQ-004 through BQ-006 and BQ-008 through BQ-009 have approved answers. (BQ-001, BQ-002 resolved by GAP-001; BQ-003 resolved by GAP-003; BQ-007 resolved by GAP-005; BQ-010 resolved by GAP-002.)
- ~~TQ-001 through TQ-005 and TQ-008 have measured results.~~ **Resolved by GAP-007:** these are implementation validation activities, not planning prerequisites.
- A canonical KPI grain/attribution matrix is approved. (Commercial vs field-activity classification approved by GAP-002; Customer–Principal as transaction-derived relationship approved by GAP-003; Item–Principal via immutable Item master approved by GAP-004; Customer-only financial metrics approved by GAP-005; monthly target-based Salesman–Principal historical responsibility approved by GAP-006; production profiling deferred to implementation validation per GAP-007; technical matrix still required.)
- A Principal sales-to-company reconciliation rule is approved.
- ~~Customer–Principal is approved as either observed analytical relationship or maintained domain relationship.~~ **Resolved by GAP-003:** transaction-derived analytical relationship only.
- ~~Principal collection KPIs are explicitly excluded or supported by an approved allocation rule.~~ **Resolved by GAP-005:** Principal financial/collection/credit KPIs are excluded; no open-balance allocation.
- One navigation/product responsibility model is selected.
- Entity Analytics has an approved multi-domain Supplier composition boundary.

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
| Is the proposed model proven? | Partially. Approved business assumptions (GAP-001–GAP-007) support planning; remaining open items are sales reconciliation, target authority, navigation, and Entity Analytics composition. |
| What is Salesman–Principal responsibility? | **Many-to-many commercial portfolio assignment (GAP-001).** Historical period responsibility is derived from monthly `BTR_SalesPersonPrincipalTarget` records (GAP-006). Principal is the primary commercial responsibility dimension; Customer is not owned by Salesman. |
| How are Salesman attribution dimensions defined? | **Commercial attribution is unified under the portfolio owner on the transaction (GAP-002, 2026-09-07T20:30:00+07:00).** Field activity is a separate operational dimension attributed to the performer. |
| Can Principal sales analytics be built? | Yes for line-derived current/historical sales, subject to attribution and reconciliation decisions. |
| Can Principal piutang/collection analytics be built safely now? | **No, and not in scope.** GAP-005 confines AR, open balance, collection performance, and credit exposure to Customer-level metrics. |
| What Principal financial metrics are in scope? | **None in this initiative (GAP-005, 2026-09-07).** Principal analytics are limited to sales, growth, product, portfolio, and market-performance from sales transactions. |
| Should Customer–Principal be first-class? | Not as a new Entity Analytics entity type now; yes as a first-class **transaction-derived** analytical relationship/projection with pair-scoped evidence and history (GAP-003). |
| What is the Customer–Principal relationship model? | **Transaction-derived from item sales history (GAP-003, 2026-09-07).** No master assignment; no pre-purchase manual linking. |
| How is historical Principal attribution determined? | **Item master (`Brg.SupplierId`); one Principal per Item; immutable for analytics (GAP-004, 2026-09-07).** No invoice-time snapshot in this initiative. |
| How is historical Salesman–Principal responsibility determined? | **Monthly `BTR_SalesPersonPrincipalTarget` records (`TargetYear`, `TargetMonth`) (GAP-006, 2026-09-07).** No effective-dated assignment model in this initiative. |
| Does production profiling block planning? | **No (GAP-007, 2026-09-07).** Proceed on approved assumptions; profile during implementation validation; material findings handled separately. |
| Should Salesman analytics be removed? | No. Reposition them around execution, attribution, allocation, and coaching. |
| Overall recommendation | GO with major redesign. |
| Planning readiness | NOT READY. |
