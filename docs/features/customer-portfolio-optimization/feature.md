# Customer Portfolio Optimization Dashboard

**Feature:** M31 — Customer Portfolio Optimization Dashboard  
**Status:** Current  
**Route:** `/dashboard/customer-portfolio`  
**API:** `GET /api/dashboard/customer-portfolio`

**Related report (approved):** Customer Report — `/reports/customers`

---

## Purpose

Management needs a **unified portfolio lens** that goes beyond current-state attention (M17), forward risk (M29), and daily collection operations (M30). The Customer Portfolio Optimization Dashboard answers:

> *What should Management do with each customer — grow, retain, protect, collect, recover, or exit review — across the full customer portfolio?*

This is a **read-only**, **deterministic**, **explainable** optimization layer — not AI/ML. It prioritizes **portfolio allocation** (where to invest management attention), not merely reporting or collection queue management.

---

## Milestone Relationship

M31 **supplements** existing customer dashboards — it does not replace any of them.

```text
M17  Customer Analytics          →  "What is happening?"
M29  Customer Risk Forecast       →  "What may happen?"
M30  Collection Optimization     →  "What should Finance collect?"
M31  Portfolio Optimization       →  "What should Management do with each customer?"
```

| Dashboard | Question | M31 relationship |
| --------- | -------- | ---------------- |
| Customer Analytics (M17) | Who needs attention today? | **Primary current-state input** — attention signals, Top Omzet/Piutang, segmentation |
| Customer Risk Forecast (M29) | Who will likely become a risk? | **Primary forward-risk input** — risk category, signals, portfolio health score |
| Collection Optimization (M30) | Who should Finance contact first? | **Collect action links to M30** — never duplicates collection queue |
| Piutang (M14) | Who owes money today? | Reuses open balance and aging semantics via shared piutang load |
| Collection (M20) | Is recovery working? | Context only — recovery % not recomputed |
| Salesman Performance (M18) | Which rep's book needs attention? | **Summary cross-read** — owner, achievement %, exposure flags on portfolio rows |
| Branch / Warehouse Performance (M22) | Territory concentration? | **Optional context** — wilayah breakdown |
| Management Attention Center (M16) | What requires attention company-wide? | **Summary promotion** — portfolio health cards link to M31 |

---

## Architectural Constraint

> **M31 must be a composition milestone, not a calculation milestone.**

M31 **composes and prioritizes** insights already produced by upstream milestones. It must **not** recalculate:

- Aging buckets (`PiutangAgingBucketResolver` — M14 authoritative)
- Dormant rule (90 days — M17 authoritative)
- Plafond breach (current — M17 authoritative)
- Forward risk signals and categories (M29 authoritative)
- Collection priority score and action categories (M30 authoritative)
- Recovery vs Billing % (M20 authoritative)

New business logic in M31 is limited to: **lifecycle classification**, **portfolio tier**, **portfolio action resolution**, **attention default filter**, and **portfolio priority ordering**.

---

## Users

| User | Primary goals on this surface |
| ---- | ------------------------------ |
| **Owner / GM** | Portfolio health, concentration risk, working capital allocation across customers |
| **Sales Manager** | Assigned customer portfolio — growth opportunities, retention, collection coordination with Finance |
| **Finance administration** | Credit review and collection handoff — link to M30 when Collect action applies |

Single dashboard serves all audiences. No role-based menu split in V1.

---

## Customer Coverage

| Rule | Description |
| ---- | ----------- |
| CPO-01 | Portfolio includes **all customers** in master data with resolvable customer key |
| CPO-02 | Default view: **Attention Customers only** — actionable subset, not thousands of healthy rows |
| CPO-03 | User can switch to **All Customers** for complete portfolio visibility |
| CPO-04 | **Never Purchased** customers (master record, zero Faktur history) are included and flagged — not hidden |
| CPO-05 | Customer key: `DashboardCustomerKeyResolver` code-first — consistent with M17/M29/M30 |
| CPO-06 | Read-only — no Desktop write-back, no automatic credit holds or account changes |

---

## Customer Value

BTR has **no customer profitability data** in portal or as a per-customer aggregate in Desktop.

| Rule | Description |
| ---- | ----------- |
| CPO-10 | **Customer Value = Omzet Proxy** — MTD invoiced omzet (`FakturView.GrandTotal`, current month, non-void) |
| CPO-11 | Value references must be labeled or documented as **NOT profitability** |
| CPO-12 | Gross margin, contribution, discount %, and retur ratio are **out of scope** for M31 |
| CPO-13 | Net sales after retur is **not required** — portal consistently uses invoiced omzet |

Future milestone may introduce margin-based value. M31 must not imply profitability where none exists.

---

## Credit Exposure

| Rule | Description |
| ---- | ----------- |
| CPO-20 | Credit exposure = **open piutang** (`KurangBayar > 1`) — all-time open snapshot |
| CPO-21 | `CustomerModel.CreditBalance` is **ignored** for M31 — keeps semantics consistent across dashboards |
| CPO-22 | Plafond breach, projected breach, and credit utilization reuse M17/M29/M30 rules — not redefined |
| CPO-23 | Working capital tied up per customer = open balance (no payment-term netting in V1) |

---

## Customer Lifecycle

M31 adopts a **computed lifecycle** — not stored on master data. Thresholds are **configurable later**; V1 starting rules below are Product Owner approved.

| Stage | Business meaning | V1 starting rule | Primary inputs |
| ----- | ---------------- | ---------------- | -------------- |
| **Never Purchased** | Master record with no sales history | Zero Faktur history | Customer master + last Faktur absence |
| **New** | Recently acquired customer | First purchase within 90 days | First Faktur date vs business date |
| **Growing** | Expanding relationship, low forward risk | Sales increasing; no M29 severe/moderate decline; not New | MTD/prior omzet trend + M29 decline absence |
| **Mature** | Stable, ongoing purchaser | Active purchasing; not declining; not New | MTD activity + stable trend + no M29 decline |
| **Declining** | Purchase trajectory weakening | M29 declining forecast signals active | M29 CRF-D decline rules |
| **Dormant** | Inactive with prior history | No Faktur ≥ 90 days; prior history exists | M17 dormant rule — **authoritative** |

**Not adopted:** explicit **Lost** lifecycle stage. **Exit Review** is a portfolio **action**, not a lifecycle stage.

Lifecycle and M17 **Active** (invoiced MTD) are complementary — a customer can be Active this month and Declining on lifecycle if forward signals indicate deceleration.

---

## Portfolio Tier

Portfolio tier is **computed** — never derived from **Klasifikasi** master data.

| Tier | Business meaning |
| ---- | ---------------- |
| **Strategic** | Highest portfolio importance — material omzet and/or receivable with relationship significance |
| **High Value** | Significant contributor or exposure |
| **Medium Value** | Mid-tier commercial importance |
| **Low Value** | Limited omzet and exposure |

| Rule | Description |
| ---- | ----------- |
| CPO-30 | Tier computed from: **omzet**, **open piutang**, **purchase frequency**, **forward risk category** (M29) |
| CPO-31 | **Klasifikasi** is a **filter dimension only** — never tier assignment input |
| CPO-32 | **HargaType** is out of scope for tier logic |
| CPO-33 | Tier thresholds are configurable — Architect defines initial bands in implementation plan |

Purchase frequency is not an existing portal KPI — M31 introduces it as a tier input (e.g. Faktur count or active months in lookback window). Architect specifies formula; must remain deterministic and explainable.

---

## Portfolio Actions

One **mutually exclusive primary action** per customer row. Every row includes human-readable reason text and rule traceability.

| Action | Business meaning | Owner |
| ------ | ---------------- | ----- |
| **Grow** | Opportunity customer — invest sales effort to expand relationship | Sales |
| **Retain** | Valuable customer showing decline — prevent churn | Sales |
| **Protect** | Strategic customer with elevated risk — defend relationship while managing exposure | Management + Sales |
| **Collect** | Collection required — **link to M30 Collection Queue** for operational detail | Finance |
| **Review Credit** | Credit or plafond issue requiring finance review | Finance |
| **Recover** | Dormant customer — attempt relationship reactivation | Sales |
| **Monitor** | Watch only — no immediate intervention | Management |
| **Exit Review** | Very low value combined with high risk — management review of continued relationship | Management |

| Rule | Description |
| ---- | ----------- |
| CPO-40 | Exactly **one primary action** per customer — highest-precedence qualifying action wins |
| CPO-41 | **Collect** action **links to M30** — portfolio page must not duplicate M30 priority queue or collection priority score |
| CPO-42 | M31 actions are **portfolio management** verbs — distinct from M29 forecast recommendations and M30 collection action categories |
| CPO-43 | Every action row includes: action key, owner, reason text, triggered rule ids |
| CPO-44 | Action precedence order is defined in implementation plan — must be deterministic and documented |

**Indicative precedence (Architect to finalize):** Collect and Review Credit (when credit urgency dominates) → Exit Review → Protect → Retain → Recover → Grow → Monitor.

Collect resolves to a **link** when M30 would assign a collection action; M31 does not embed M30 queue rows.

---

## Attention Default Filter

Default view shows **Attention Customers** — the actionable portfolio subset.

| Rule | Description |
| ---- | ----------- |
| CPO-50 | A customer qualifies for **Attention** when any of: M17 attention signal; M29 risk category above Healthy; portfolio action other than Monitor; lifecycle Declining, Dormant, or Never Purchased with material master presence |
| CPO-51 | **All Customers** view removes attention filter — full portfolio enumeration |
| CPO-52 | Attention count KPIs on dashboard must reconcile with filter logic |
| CPO-53 | Architect documents exact attention qualification rules in implementation plan |

---

## Portfolio Priority

Distinct from M30 **Collection Priority Score**.

| Rule | Description |
| ---- | ----------- |
| CPO-60 | **Portfolio Priority Score** orders customers within the portfolio queue — integer, deterministic, explainable |
| CPO-61 | Score composes: portfolio action weight, tier weight, M29 risk category weight, omzet/piutang impact component |
| CPO-62 | Tie-break: open balance or MTD omzet descending, then customer name |
| CPO-63 | M30 collection priority is **not** recomputed or merged into portfolio score — separate concerns |

---

## Salesman Context (Summary Only)

| Rule | Description |
| ---- | ----------- |
| CPO-70 | Portfolio **owner** = **last invoicing salesman** — existing portal attribution standard |
| CPO-71 | Do not use route-owner (`BTR_SalesRuteItem`) as portfolio owner |
| CPO-72 | Each portfolio row may show M18 **summary** fields: salesman name, achievement %, high piutang exposure flag |
| CPO-73 | Do not reproduce M18 Salesman Performance dashboard on M31 |

---

## Executive Dashboard Integration

| Rule | Description |
| ---- | ----------- |
| CPO-80 | M16 promotes **summary cards only** — no detailed portfolio tables on executive page |
| CPO-81 | Example executive metrics: Portfolio Healthy %, Customers At Risk count, Strategic Customers At Risk count |
| CPO-82 | Cards link to full M31 dashboard |
| CPO-83 | Alert Center integration is **out of scope** for M31 V1 |

---

## Investigation and Drill-Down Chain

Approved evidence path:

```text
Executive Dashboard (portfolio summary cards)
        ↓
Portfolio Optimization (M31)
        ↓
Customer Analytics (M17) / Risk Forecast (M29) / Collection Opt (M30)
        ↓
Customer Report (new — portal)
        ↓
Sales Report / Piutang Report
        ↓
BTR Desktop (operational resolution)
```

| Rule | Description |
| ---- | ----------- |
| CPO-90 | Every portfolio row supports investigation workflow (M24 pattern) |
| CPO-91 | **Customer Report** is **in scope** for M31 — consolidated customer evidence layer |
| CPO-92 | Customer Report is read-only; period and column semantics defined in implementation plan |
| CPO-93 | Collect action drill-down links to M30 Collection Optimization dashboard |

---

## Customer Report (Approved)

New portal report surface — business purpose:

> Provide tabular customer-level evidence between portfolio dashboard and transaction reports — so investigation does not stop at Sales or Piutang report alone.

Minimum business expectations (Architect details grain and columns):

- One row per customer (or per customer × period — Architect decides)
- Customer identity: code, name, wilayah, klasifikasi (display/filter)
- MTD omzet, open piutang, last purchase date
- Lifecycle stage and portfolio tier (from M31 snapshot or live compose — Architect decides)
- Portfolio action and owner
- Link from M31 portfolio row opens Customer Report with customer pre-filter

---

## Dashboard Content (Business)

### Headline sections

1. **Executive summary** — plain-language portfolio brief (server-composed narrative)
2. **Portfolio health KPIs** — health score (from M29), strategic at risk, working capital tied, attention count
3. **Distribution charts** — lifecycle breakdown, tier breakdown, omzet vs piutang concentration
4. **Priority portfolio queue** — default Attention Customers; columns include tier, lifecycle, action, owner, salesman, reason
5. **Action segments** — expandable lists by portfolio action (Grow, Retain, Protect, etc.)
6. **Concentration and geography** — Top Omzet, Top Piutang (reuse M17 rankings or snapshot), wilayah breakdown
7. **Navigation** — links to M17, M29, M30, Customer Report, Sales Report, Piutang Report

### Filters (V1)

- View toggle: Attention Customers / All Customers
- Wilayah, Klasifikasi (filter only), Tier, Lifecycle, Action, Salesman (owner)

---

## Data Scope and Shared Semantics

| Rule | Description |
| ---- | ----------- |
| CPO-100 | Business date from `IBusinessDateProvider.Today` |
| CPO-101 | Sales omzet: current calendar month, non-void Fakturs |
| CPO-102 | Piutang: all-time open balance snapshot, `KurangBayar > 1` |
| CPO-103 | Dormant: 90 days — M17 `DormantDaysThreshold` authoritative |
| CPO-104 | M29 risk category and signals consumed in-memory — not recalculated |
| CPO-105 | M30 contexts consumed in-memory for Collect link resolution — not recalculated |
| CPO-106 | No AI/ML, probability scores, or optimization solvers |
| CPO-107 | UangMuka excluded from payment behavior aggregates (consistent with M29) |

---

## Snapshot Refresh

- **Worker:** `RefreshDashboardCustomerSnapshotWorker` (Customer domain, ~30 min)
- **Chain:** M17 → M29 → M30 → **M31 Portfolio Optimization**
- **Cross-reads:** M18 salesman snapshot (summary fields); optionally M22 location snapshot
- **Transaction:** M17 + M29 + M30 + M31 written atomically via `ReplaceCurrent`
- **Manual refresh:** `POST /api/admin/dashboard/refresh?domain=Customer`

M31 step receives in-memory contexts from M17, M29, and M30 aggregators — same pattern as M30 consuming M29.

---

## Prerequisites (Separate Work Items)

| Prerequisite | Reason |
| ------------ | ------ |
| **IsSuspend CustomerForm Desktop fix** | Checkbox not wired to save/load — master data integrity for Suspended+Sales signal; fix before M31 relies on suspend-related portfolio rules |
| **Customer Report portal surface** | Approved drill-down chain requires dedicated report — part of M31 delivery |

---

## Out of Scope (V1)

| Item | Reason |
| ---- | ------ |
| HargaType in portfolio logic | PO decision — operationally useful, not portfolio-relevant |
| Return ratio / retur analytics per customer | Future milestone |
| Net sales after retur | Semantic inconsistency with existing portal |
| Profitability / gross margin / contribution | No data |
| Per-customer DSO headline | Deferred (M29 excluded) |
| Field activity (M18.5) integration | Future enhancement |
| M30 collection queue duplication | Reuse via link only |
| Alert Center new signal registration | Not in V1 |
| Replacing M17, M29, or M30 dashboards | Supplement only |
| Historical portfolio snapshot retention | Point-in-time only |
| AI/ML portfolio scoring | Deterministic rules only |
| Automatic account actions or Desktop write-back | Read-only product |
| Custom date-range filtering | Platform deferred capability |
| Role-based menu or data filtering | Platform deferred capability |

---

## Acceptance Criteria

Management can verify M31 by confirming:

1. **Portfolio question answered** — each attention customer row shows one primary action (Grow, Retain, Protect, Collect, Review Credit, Recover, Monitor, or Exit Review) with owner and reason.
2. **Default view is actionable** — dashboard opens on Attention Customers; All Customers toggle shows full portfolio including Never Purchased.
3. **Composition integrity** — M29 risk category and M17 dormant/plafond signals match upstream dashboards for same customer on same refresh.
4. **Collect links to M30** — Collect action navigates to Collection Optimization; M30 queue is not duplicated on M31.
5. **Value labeling** — omzet-based value is presented as proxy, not profitability.
6. **Lifecycle and tier visible** — every portfolio row shows computed lifecycle stage and portfolio tier; Klasifikasi appears as filter only.
7. **Salesman summary** — portfolio owner shows last invoicing salesman with optional achievement and exposure flags from M18.
8. **Executive summary** — M16 shows portfolio health cards linking to M31; no detailed tables on executive page.
9. **Investigation chain** — portfolio row → Customer Report → Sales/Piutang Report path works with customer pre-filter.
10. **Read-only** — no transactional writes from portal.

---

## Future Extensibility

Design hooks for later milestones — not V1.

| Future capability | Extension point |
| ----------------- | --------------- |
| Customer profitability weighting | Replace or augment omzet proxy in tier and action logic |
| Return ratio per customer | New signal input from Desktop RF1 aggregate |
| Field activity integration | Visit execution as portfolio engagement input |
| Alert Center integration | Register portfolio action exceptions |
| Lifecycle threshold configuration | UI or appsettings for New/Growing/Mature bands |
| Historical portfolio trend | Snapshot history table per refresh |

---

## Related Artifacts

- Discovery analysis: [portal-analysis-m31-customer-portfolio-optimization.md](../../work/btr-portal/portal-analysis-m31-customer-portfolio-optimization.md)
- Implementation plan: [implementation-plan-m31-customer-portfolio-optimization.md](../../work/btr-portal/implementation-plan-m31-customer-portfolio-optimization.md)
- Upstream features:
  - [customer-risk-forecast/feature.md](../customer-risk-forecast/feature.md) (M29)
  - [collection-optimization/feature.md](../collection-optimization/feature.md) (M30)
- Portal domain: [btr-portal-domain.md](../btr-portal/btr-portal-domain.md)
- Customer attention UX: [customer-attention-list-ux/feature.md](../btr-portal/customer-attention-list-ux/feature.md) (M17)

---

## Document Maintenance

When M31 is implemented:

1. ~~Update this document status to **Current**~~ ✓
2. ~~Add M31 dashboard, lifecycle, tier, and action concepts to `docs/features/btr-portal/btr-portal-domain.md`~~ ✓
3. ~~Add operational usage to `docs/features/btr-portal/btr-portal-operational.md`~~ ✓
4. ~~Record implementation summary under `docs/work/btr-portal/`~~ ✓ — [implementation-summary-m31-customer-portfolio-optimization.md](../../work/btr-portal/implementation-summary-m31-customer-portfolio-optimization.md)

**Authoritative discovery source:** [portal-analysis-m31-customer-portfolio-optimization.md](../../work/btr-portal/portal-analysis-m31-customer-portfolio-optimization.md) — Section 18 Product Owner Decisions.
