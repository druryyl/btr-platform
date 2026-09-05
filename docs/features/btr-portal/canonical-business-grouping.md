# Canonical Business Grouping

## Purpose

This artifact defines the canonical business-purpose hierarchy for the BTR Portal KPI collection. It preserves each KPI's authoritative Entity classification and assigns every KPI to one primary Business Group based on the management concern it supports.

Entity assignments and KPI identities are sourced from [`kpi-entity-classification.md`](kpi-entity-classification.md).

## Canonical Taxonomy

### Customer

| Business Group | Business Purpose |
|---|---|
| Commercial Value & Concentration | Understand customer revenue contribution and dependence on high-value customers. |
| Lifecycle & Engagement | Monitor whether customers are active, inactive, dormant, declining, or otherwise changing their purchasing engagement. |
| Receivable Exposure & Aging | Understand customer balances, overdue exposure, aging severity, concentration, and working capital tied in receivables. |
| Collection Performance & Forecast | Monitor payment realization, recovery efficiency, collection pacing, expected cash, and forecast confidence. |
| Credit & Payment Risk | Identify credit-limit issues, payment deterioration, and customers requiring risk intervention. |
| Portfolio Oversight | Assess overall portfolio health, strategic importance, management attention, and readiness of portfolio information. |
| Collection Action | Manage the volume, priority, impact, and escalation of customer collection actions. |

### Salesman

| Business Group | Business Purpose |
|---|---|
| Sales Performance & Target | Evaluate sales results, rankings, trends, and achievement against assigned targets. |
| Sales Forecast & Pacing | Project period-end sales and determine the pace required to close target gaps. |
| Territory & Location Performance | Compare sales contribution and concentration across warehouses and wilayah managed by the sales force. |
| Customer Portfolio & Receivables | Assess the quality of salesman-owned customer portfolios through receivable, overdue, and inactivity exposure. |
| Field Execution & Effectiveness | Monitor field-force coverage, visit execution, interaction quality, compliance, and sales outcomes generated from activity. |

### Item

| Business Group | Business Purpose |
|---|---|
| Inventory Value & Portfolio | Understand the size, value, composition, and category concentration of the item portfolio. |
| Inventory Health & Movement | Identify non-moving, slow-moving, aging, or otherwise at-risk items and the capital affected. |
| Stock Balance & Replenishment | Evaluate stock sufficiency, future stock position, purchase needs, and item-level remediation actions. |
| Location & Allocation | Understand how inventory and inventory risk are distributed across warehouses and locations. |

### Supplier

| Business Group | Business Purpose |
|---|---|
| Purchasing Volume & Processing | Monitor purchase value, invoice volume, posting progress, backlog, and procurement intake by location. |
| Supplier Dependency & Continuity | Assess purchasing concentration, supplier dependency, inactivity, and continuity exposure across Principals. |

---

# KPI Mapping

## Customer

### Commercial Value & Concentration

| KPI Code | KPI Name |
|---|---|
| CU-KPI-003 | Top Omzet Customer % |
| CU-KPI-009 | Top 10 Omzet (Ranking) |
| CU-KPI-066 | Total MTD Omzet (Portfolio) |
| EX-KPI-013 | Top 5 Customers (Critical Exposure) |

### Lifecycle & Engagement

| KPI Code | KPI Name |
|---|---|
| CU-KPI-005 | Active Customer Count |
| CU-KPI-006 | Dormant Customer Count |
| CU-KPI-029 | Inactivity Signal Count |
| CU-KPI-030 | Purchase Decline Signal Count |
| CU-KPI-068 | Never Purchased Count |
| CU-KPI-069 | Dormant Count (Lifecycle) |
| CU-KPI-070 | Declining Count |
| SA-KPI-005 | Total Customer |

### Receivable Exposure & Aging

| KPI Code | KPI Name |
|---|---|
| CU-KPI-001 | Overdue Customer Count |
| CU-KPI-002 | >90 Day Exposure |
| CU-KPI-004 | Top Piutang Customer % |
| CU-KPI-010 | Top 10 Piutang (Ranking) |
| CU-KPI-026 | Total Piutang (Context) |
| CU-KPI-047 | Overdue Exposure (Context) |
| CU-KPI-065 | Working Capital Tied Amount |
| CU-KPI-067 | Total Open Balance (Portfolio) |
| EX-KPI-004 | Total Piutang |
| EX-KPI-005 | Overdue Customer Count |
| EX-KPI-006 | Piutang > 90 Hari (Amount & %) |
| EX-KPI-007 | Top Customer % (Piutang Concentration) |
| FI-KPI-001 | Total Piutang |
| FI-KPI-002 | Total Customer (with balance) |
| FI-KPI-003 | Overdue Customer |
| FI-KPI-004 | Top 10 Customer % |
| FI-KPI-005 | Top 20 Customer % |
| FI-KPI-006 | Aging Bucket — Current |
| FI-KPI-007 | Aging Bucket — 1–30 Days |
| FI-KPI-008 | Aging Bucket — 31–60 Days |
| FI-KPI-009 | Aging Bucket — 61–90 Days |
| FI-KPI-010 | Aging Bucket — > 90 Days |
| FI-KPI-011 | Piutang > 90 Hari (Amount & %) |
| FI-KPI-012 | Top 10 Outstanding Customers |
| FI-KPI-013 | Overdue Exposure |
| FI-KPI-014 | >90d Exposure |
| FI-KPI-015 | Overdue Concentration % |
| FI-KPI-021 | Legacy Debt Count |
| FI-KPI-026 | Top Overdue Customers |
| FI-KPI-028 | Top Overdue Wilayah |
| FI-KPI-042 | Report Footer — Total Piutang |
| FI-KPI-043 | Report Footer — Total Customer |

### Collection Performance & Forecast

| KPI Code | KPI Name |
|---|---|
| CU-KPI-025 | Forecast Confidence |
| CU-KPI-049 | Recovery vs Billing % (Context) |
| CU-KPI-050 | Planning Confidence |
| FI-KPI-016 | Cash Collected MTD |
| FI-KPI-017 | Recovery vs Billing % |
| FI-KPI-018 | Payment Mix — Cash |
| FI-KPI-019 | Payment Mix — Giro |
| FI-KPI-020 | Payment Mix — Adjustment |
| FI-KPI-029 | Expected Cash Collection |
| FI-KPI-030 | Projected Month-End Collection |
| FI-KPI-031 | Collection Forecast % |
| FI-KPI-032 | Daily Cash Collection Average |
| FI-KPI-033 | Required Daily Collection |
| FI-KPI-034 | Remaining Collection Target |
| FI-KPI-035 | Days Remaining |
| FI-KPI-036 | Recovery vs Billing Forecast |
| FI-KPI-037 | Scenario Cash (Best / Expected / Worst) |
| FI-KPI-038 | Forecast Confidence (Cash) |
| FI-KPI-039 | Outstanding Due Remaining |
| FI-KPI-040 | Collection Gap |

### Credit & Payment Risk

| KPI Code | KPI Name |
|---|---|
| CU-KPI-007 | Plafond Breach Count |
| CU-KPI-008 | Suspended + Sales Count |
| CU-KPI-020 | Customers Forecasted at Risk |
| CU-KPI-021 | High Risk Customer Count |
| CU-KPI-022 | Elevated Risk Receivable |
| CU-KPI-023 | Elevated Risk Receivable % |
| CU-KPI-027 | Payment Delay Signal Count |
| CU-KPI-028 | Credit Limit Signal Count |
| CU-KPI-031 | Collection Risk Signal Count |
| CU-KPI-032 | Risk Category Count — Healthy |
| CU-KPI-033 | Risk Category Count — Watch |
| CU-KPI-034 | Risk Category Count — Attention |
| CU-KPI-035 | Risk Category Count — High Risk |
| CU-KPI-036 | Risk Category Count — Critical |
| CU-KPI-064 | Strategic At Risk Count |
| EX-KPI-020 | Customers At Risk Count |
| EX-KPI-021 | Strategic Customers At Risk Count |
| FI-KPI-022 | Aging Risk Summary — 1–30 Days |
| FI-KPI-023 | Aging Risk Summary — 31–60 Days |
| FI-KPI-024 | Aging Risk Summary — 61–90 Days |
| FI-KPI-025 | Aging Risk Summary — > 90 Days |
| FI-KPI-041 | Top Collection Risks (Table) |

### Portfolio Oversight

| KPI Code | KPI Name |
|---|---|
| CU-KPI-024 | Portfolio Health Score |
| CU-KPI-060 | Portfolio Health Score |
| CU-KPI-061 | Portfolio Healthy % |
| CU-KPI-062 | Attention Customer Count |
| CU-KPI-063 | Strategic Customer Count |
| CU-KPI-071 | Customer Report Row Metrics |
| EX-KPI-017 | Snapshot Freshness (Last Refreshed) |
| EX-KPI-018 | Domain Attention Summary Cards |
| EX-KPI-019 | Portfolio Healthy % |
| EX-KPI-023 | Alert Count — Customer |
| EX-KPI-029 | Platform Alerts (Pinned) |

### Collection Action

| KPI Code | KPI Name |
|---|---|
| CU-KPI-040 | Actions Today |
| CU-KPI-041 | Immediate Collection Count |
| CU-KPI-042 | Proactive Reminder Count |
| CU-KPI-043 | Credit Review Count |
| CU-KPI-044 | Sales Recovery Count |
| CU-KPI-045 | Management Escalation Count |
| CU-KPI-046 | Collection Impact Total |
| CU-KPI-048 | Due Within 7 Days |
| CU-KPI-051 | Immediate Impact Total |
| EX-KPI-024 | Alert Count — Collection |

---

## Salesman

### Sales Performance & Target

| KPI Code | KPI Name |
|---|---|
| EX-KPI-001 | Achievement % |
| EX-KPI-002 | Total Achievement (Total Omzet MTD) |
| EX-KPI-003 | Total Target |
| EX-KPI-022 | Alert Count — Sales |
| SA-KPI-004 | Total Faktur |
| SA-KPI-006 | Weekly Invoiced Sales Trend |
| SA-KPI-007 | Top 10 Salesman (Omzet) |
| SA-KPI-008 | Target vs Achievement Chart |
| SF-KPI-001 | Below Target Count |
| SF-KPI-002 | Missing Target Setup Count |
| SF-KPI-006 | Top Omzet Salesman % |
| SF-KPI-008 | Top 10 Omzet (Ranking) |
| SF-KPI-009 | Top 10 Achievement % (Ranking) |
| SF-KPI-011 | Principal Achievement Table |
| — | Top Omzet |

### Sales Forecast & Pacing

| KPI Code | KPI Name |
|---|---|
| SA-KPI-009 | Current Sales |
| SA-KPI-010 | Current Achievement % |
| SA-KPI-011 | Forecast Sales (Expected) |
| SA-KPI-012 | Forecast Achievement % |
| SA-KPI-013 | Daily Average Sales |
| SA-KPI-014 | Required Daily Sales |
| SA-KPI-015 | Target Gap |
| SA-KPI-016 | Days Remaining |
| SA-KPI-017 | Scenario Projection (Best / Expected / Worst) |
| SA-KPI-018 | Forecast Confidence |
| SA-KPI-019 | Forecast Risk Indicator |

### Territory & Location Performance

| KPI Code | KPI Name |
|---|---|
| OP-KPI-004 | Top 1 Warehouse Sales % |
| OP-KPI-005 | Top 1 Wilayah Sales % |
| OP-KPI-009 | Top Warehouse by Sales (Ranking) |
| OP-KPI-011 | Top Wilayah by Sales (Ranking) |

### Customer Portfolio & Receivables

| KPI Code | KPI Name |
|---|---|
| FI-KPI-027 | Top Overdue Salesmen |
| SF-KPI-003 | High Overdue Exposure Count |
| SF-KPI-004 | High Piutang Exposure Count |
| SF-KPI-005 | Dormant Portfolio Count |
| SF-KPI-007 | Top Piutang Salesman % |
| SF-KPI-010 | Top 10 Piutang (Ranking) |

### Field Execution & Effectiveness

| KPI Code | KPI Name |
|---|---|
| SF-KPI-012 | Planned Visits |
| SF-KPI-013 | Actual Visits |
| SF-KPI-014 | Missed Visits |
| SF-KPI-015 | Unplanned Visits |
| SF-KPI-016 | Effective Calls |
| SF-KPI-017 | Visit Execution % |
| SF-KPI-018 | Effective Call Rate |
| — | Active Salesmen |
| — | Bottom Effective Call Rate |
| — | Bottom Visit Execution |
| — | GPS Valid Rate |
| — | Omzet Generated |
| — | Orders Generated |
| — | Sales Orders |
| — | Top Effective Call Rate |
| — | Top Orders |
| — | Top Visit Execution |

---

## Item

### Inventory Value & Portfolio

| KPI Code | KPI Name |
|---|---|
| EX-KPI-010 | Total Inventory Value |
| EX-KPI-011 | Top Category % (Inventory) |
| EX-KPI-014 | Top 5 Categories (Critical Exposure) |
| IN-KPI-001 | Total Inventory Value |
| IN-KPI-002 | Total Item |
| IN-KPI-003 | Top 10 Category (Ranking) |
| IN-KPI-010 | Category Risk Exposure |
| IN-KPI-026 | Report Footer — Total Inventory Value |
| IN-KPI-027 | Report Footer — Total Item |

### Inventory Health & Movement

| KPI Code | KPI Name |
|---|---|
| EX-KPI-025 | Alert Count — Inventory |
| EX-KPI-028 | Inventory Risk Summary (Alert Center) |
| IN-KPI-005 | Dead Stock Count & Value |
| IN-KPI-006 | Slow Moving Count & Value |
| IN-KPI-007 | Never Sold Count & Value |
| IN-KPI-008 | At-Risk Inventory % |
| IN-KPI-009 | Aging Distribution (Movement Classes) |
| IN-KPI-012 | Top 10 Dead / Slow Moving (Ranking) |
| IN-KPI-015 | Inventory Health Score |
| IN-KPI-024 | Recoverable Capital |

### Stock Balance & Replenishment

| KPI Code | KPI Name |
|---|---|
| IN-KPI-013 | Projected Inventory Value @ Horizon |
| IN-KPI-014 | Average Days of Supply (Company) |
| IN-KPI-016 | Stock-Out Risk Items / Value |
| IN-KPI-017 | Overstock / Understock Value |
| IN-KPI-018 | Scenario Projected Value (Best/Expected/Worst) |
| IN-KPI-019 | Forecast Confidence (Inventory) |
| IN-KPI-020 | Days of Supply (Item) |
| IN-KPI-021 | Recommended Purchase Qty (Indicative) |
| IN-KPI-028 | Recommended Purchase Value (Item) |
| IN-KPI-022 | Critical Actions Count |
| IN-KPI-023 | Recommended Purchase Budget |
| IN-KPI-025 | Action Counts by Type (Purchase / Delay / Transfer / Clearance) |

### Location & Allocation

| KPI Code | KPI Name |
|---|---|
| EX-KPI-027 | Alert Count — Location |
| OP-KPI-001 | Top 1 Warehouse Inventory % |
| OP-KPI-002 | Top 3 Warehouse Inventory % |
| OP-KPI-003 | Top 1 Warehouse At-Risk % |
| OP-KPI-006 | Inactive Warehouse With Stock Count |
| OP-KPI-007 | Top Warehouse by Inventory (Ranking) |
| OP-KPI-008 | Top Warehouse by At-Risk (Ranking) |
| OP-KPI-012 | Location Attention Signal Counts |

---

## Supplier

### Purchasing Volume & Processing

| KPI Code | KPI Name |
|---|---|
| EX-KPI-008 | Pending Posting (Count & Value) |
| EX-KPI-026 | Alert Count — Purchasing |
| OP-KPI-010 | Top Warehouse by Purchasing (Ranking) |
| PU-KPI-001 | Grand Total Purchase |
| PU-KPI-002 | Total Invoice |
| PU-KPI-003 | Posted % |
| PU-KPI-004 | Pending Posting Value |
| PU-KPI-005 | Qualified Backlog Count & Value |
| PU-KPI-013 | Report Footer — Grand Total Purchase |
| PU-KPI-014 | Report Footer — Total Invoice |

### Supplier Dependency & Continuity

| KPI Code | KPI Name |
|---|---|
| EX-KPI-009 | Top Principal % (Purchasing) |
| EX-KPI-012 | Top Supplier % (Inventory) |
| EX-KPI-015 | Top 5 Suppliers (Critical Exposure) |
| EX-KPI-016 | Top 5 Principals (Critical Exposure) |
| IN-KPI-004 | Top 10 Supplier (Ranking) |
| IN-KPI-011 | Supplier Risk Exposure |
| PU-KPI-006 | Top 1 Principal % |
| PU-KPI-007 | Top 3 Principal % |
| PU-KPI-008 | Compound Dependency Count |
| PU-KPI-009 | Purchasing Inactivity Flag |
| PU-KPI-010 | Principal At-Risk Count |
| PU-KPI-011 | Top 10 Principal (Ranking) |
| PU-KPI-012 | Principal Exposure Comparison |

---

# Borderline Grouping Decisions

The following KPIs have plausible secondary concerns but retain one primary Business Group:

| KPI | Primary Business Group | Decision Basis |
|---|---|---|
| EX-KPI-013 — Top 5 Customers (Critical Exposure) | Customer — Commercial Value & Concentration | Its primary management purpose is understanding dependence on a small set of high-value customers; receivable exposure is a secondary context. |
| CU-KPI-043 — Credit Review Count | Customer — Collection Action | It represents an operational action workload, even though the action addresses credit risk. |
| EX-KPI-029 — Platform Alerts (Pinned) | Customer — Portfolio Oversight | It provides cross-portfolio management attention, with customer and collection issues as the authoritative primary entity context. |
| IN-KPI-024 — Recoverable Capital | Item — Inventory Health & Movement | The capital is recoverable specifically because unhealthy or non-moving item stock requires intervention. |
| PU-KPI-009 — Purchasing Inactivity Flag | Supplier — Supplier Dependency & Continuity | Supplier inactivity primarily indicates a continuity or relationship gap rather than purchase-processing throughput. |

# Coverage Validation

| Entity | Business Groups | KPIs Mapped |
|---|---:|---:|
| Customer | 7 | 107 |
| Salesman | 5 | 53 |
| Item | 4 | 38 |
| Supplier | 2 | 23 |
| **Total** | **18** | **221** |

Validation criteria:

- Every source KPI is mapped.
- Every source KPI is mapped exactly once.
- Every KPI remains under its authoritative Entity.
- KPI Codes and KPI Names are preserved exactly.
