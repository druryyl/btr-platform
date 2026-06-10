# BTR Portal Analysis — M20 Collection Dashboard

**Status:** Analysis complete — Product Owner decisions recorded (Section 13). Ready for Architect.  
**Scope:** Business analysis only. No implementation plans, API design, database design, or code.  
**Date:** 2026-06-08 (analysis) · Product Owner decisions recorded 2026-06-08  
**Context:** BTR Portal V2 (M16–M19 complete) follows a management philosophy: *What requires management attention?* M20 introduces a **Collection Dashboard** at `/dashboard/collection` to answer: *Are receivables being converted into cash, and which receivables require collection attention?*

**Approved roadmap position:** M17 Customer Analytics → M18 Salesman Performance → M19 Slow Moving & Dead Stock → **M20 Collection Dashboard** → … → M25 Sales Force Effectiveness

**Reference documents:** `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md`, `docs/features/btr-portal/btr-portal-domain.md`, `docs/features/btr-portal/btr-portal-operational.md`, `docs/features/btr-portal/btr-portal-architecture.md`, `docs/features/materialized-dashboard/materialized-dashboard-domain.md`, `docs/work/btr-portal/M17-Customer-Analytics-Analysis.md`, `docs/work/btr-portal/M18 Salesman Performance - Analysis.md`, `btr-reporting-investigation.md`, `docs/archive/btr-portal-api-scaffolding/portal-analysis-m13-m15.md`

---

## 1. Executive Summary

BTR Portal today exposes **receivable exposure** well at company, customer, and salesman levels — but does **not** answer whether the **collection process** is working. Piutang Dashboard (M14) shows total outstanding, aging distribution, overdue customer count, and Top 10 customers by balance. Customer Analytics (M17) and Salesman Performance (M18) surface **collection exposure** signals (overdue, >90d, high piutang) from the **open-balance** perspective. None of these dashboards measure **cash recovered**, **payment pace vs billing**, **collection workload composition**, or **recovery risk patterns** that distinguish collection management from receivable reporting.

**Critical discovery:** BTR Desktop records **billing documents** (`BTR_Tagihan`), **payment entries** (`BTR_PiutangLunas`), and **per-Faktur lifecycle events** (`PiutangTrackerDal` union). It does **not** record structured collection activities such as follow-up calls, visit outcomes, promise-to-pay dates, collector assignments separate from salesman, or collection notes. Field check-ins (`BTR_CheckIn`) exist but are **not linked to payment outcomes** in any analytics discovered.

**Implication for M20 scope (approved):** M20 focuses on **collection exposure, receivable recovery risk, and cash recovery effectiveness** from piutang and pelunasan data — **not** collection activity tracking or CRM-style follow-up. Tagihan lifecycle metrics are administrative supporting information only, not headline KPIs.

**All open questions resolved.** See Section 13 for authoritative Product Owner decisions.

### Key findings

| Finding | Implication |
| ------- | ----------- |
| **No separate Collector entity** — collection ownership follows **invoicing salesman** (`BTR_Faktur.SalesPersonId`) and **Tagihan** (`BTR_Tagihan.SalesPersonId`) | M20 ownership model should reuse salesman attribution; do not invent a collector dimension |
| **Payment data exists** in `BTR_PiutangLunas` with `LunasDate`, `JenisLunas` (Cash, Cek/BG, UangMuka), linked to Tagihan | Collection **effectiveness** KPIs (cash received, payment mix) are **computable** via `IPenerimaanPelunasanSalesDal` and `IPelunasanInfoDal` — **Desktop only today** |
| **Open exposure KPIs mature** in Piutang, Customer, Salesman, and Executive snapshots | M20 must **differentiate** from M14/M17/M18 — avoid duplicating Total Piutang, aging pie, and generic overdue counts without collection-management framing |
| **FF1 `PiutangSalesWilayahDal`** exposes `WilayahName`, payment decomposition (`BayarTunai`, `BayarGiro`, `Retur`, `Potongan`, `MateraiAdmin`) per open Faktur | Regional collection risk and non-cash settlement patterns are **available at row level** — not aggregated in portal |
| **Tagihan workflow** tracks billing state (`StatusPiutang`: Tercatat / Ditagihkan / Lunas) and flags (`IsTandaTerima`, `IsTagihUlang`) | Billing pipeline indicators are **partially available** — not exposed as management KPIs |
| **PiutangTracker** is a per-Faktur **event ledger** (piutang created, tagihan issued, payment posted) — not a follow-up log | Useful for drill-down validation; **not** a collection workload dashboard source |
| **No aging deterioration trend** — snapshots are point-in-time | **Excluded from M20** — no historical aging trend analysis |
| **DSO** | **Excluded from M20** — Recovery vs Billing % is the mandatory effectiveness KPI |
| **M18 deferred** "collection payments lagging new billing" to M20 | **Approved** — Recovery vs Billing % at company level; LowRecoveryVsBilling signal per salesman |

### Approved product outcome

Deliver **Collection Dashboard** at `/dashboard/collection` (sidebar label: **Collection**) using **Attention-First layout with Recovery Summary**. Materialize KPIs in a **dedicated `BTRPD_Collection*` snapshot domain** (Piutang open balance + Pelunasan sources; refresh cadence **30 minutes**). Mandatory sections (in order):

1. Collection Attention Cards  
2. Recovery Summary  
3. Aging Risk Summary  
4. Collection Attention List  
5. Top Overdue Customers  
6. Top Overdue Salesmen  
7. Top Overdue Wilayah  
8. Navigation to supporting dashboards and reports  

**Mandatory recovery KPIs:** Cash Collected MTD · Recovery vs Billing % · Payment Mix (Cash / Giro / Adjustment) at **company level only**. **Rankings use overdue balance**, not total balance. **Wilayah** is a required dimension (`Customer.WilayahId`). **Attention list grain:** Customer × Signal and Salesman × Signal. **No Pelunasan Report** in M20. **Executive Dashboard** promotion after M20: Cash Collected MTD, Recovery vs Billing %, Overdue Concentration %.

### Milestone positioning (approved)

| Milestone | Question answered |
| --------- | ----------------- |
| M14 Piutang Dashboard | How much money is owed? |
| M17 Customer Analytics | Which customers require attention? |
| M18 Salesman Performance | Which salesmen require attention? |
| **M20 Collection Dashboard** | **Are receivables being converted into cash?** |

**Explicitly out of M20:** Collection CRM (follow-up, promise-to-pay, visit outcomes), DSO, aging deterioration trends, salesman sales-performance metrics, route/check-in/effective call (M25), retur analytics as standalone dimension, Pelunasan Report, event-driven refresh after pelunasan, Tagihan as headline KPIs.

---

## 2. Management Attention Discovery

This section identifies collection-related situations that typically require management intervention, mapped to **existing BTR capabilities**. Items marked **Portal today** are in current portal snapshots or reports. **Desktop only** exists in BTR Desktop but is not portal-exposed. **Not available** means no implemented logic was discovered.

### 2.1 Overdue exposure and aging severity

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **High total overdue exposure** | Past-due balances require collection action | Piutang aging buckets; `OverdueCustomer` KPI | **Portal today** (M14) — company level |
| **Severely aged receivables (> 90 days)** | Debt unlikely to self-correct; escalation risk | `DaysOver90` bucket; executive `AgingOver90Amount/%` | **Portal today** — company level |
| **Customer with any overdue balance** | Account needs follow-up | M17 `Overdue` attention signal | **Portal today** (M17) |
| **Customer with balance only in > 90 day bucket** | Chronic overdue — collection failure on account | Per-customer aging from `PiutangOpenBalanceDto` | **Derivable** — same bucket logic as `DashboardPiutangAggregator` |
| **Many overdue Faktur on one customer** | Multiple unpaid invoices — repeated collection failure | FF1 rows grouped by `CustomerCode` | **Partial** — row-level in Piutang Report |
| **Overdue exposure increasing** | Collection posture worsening | Snapshot `GeneratedAt` only — no retained history | **Not available** |
| **Aging profile shifting toward older buckets** | Portfolio quality deteriorating | Current snapshot buckets only | **Not available** without history or period comparison |

### 2.2 Collection concentration and workload

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Few customers hold majority of overdue** | Collection effort should prioritize top accounts | Top 10 customers by `KurangBayar`; executive Top Customer % | **Portal today** — by total balance, not overdue-only ranking |
| **Single customer dominates company piutang** | Default risk is concentrated | M17 `TopPiutangCustomerPercent`; executive `TopCustomerPercent` | **Portal today** |
| **Salesman with large overdue portfolio** | Rep's book needs collection intervention | M18 `HighOverdueExposure` signal | **Portal today** (M18) |
| **Wilayah with high overdue concentration** | Regional collection problem | FF1 `WilayahName` + `JatuhTempo` + `KurangBayar` | **Partial** — row-level; no wilayah aggregate KPI |
| **Large overdue exposure with few collectors responsible** | Workload imbalance across salesmen | Group open overdue by `SalesPersonName` | **Derivable** from `IPiutangOpenBalanceWithSalesmanDal` |
| **High count of open overdue Faktur** | Operational collection queue depth | Count FF1 rows where `JatuhTempo < today` and `KurangBayar > 1` | **Not computed** |

### 2.3 Cash recovery and collection effectiveness

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Low cash collections in period** | Recovery pace insufficient | `PenerimaanPelunasanSalesDal` — `TotalBayar` by `LunasDate` | **Desktop only** (FF2) |
| **Collections lagging new billing** | Receivable base growing faster than recovery | Compare `PenerimaanPelunasanSalesDal` vs `FakturView` omzet by salesman/month | **Not computed** — M18 deferred to M20 |
| **High giro/cek share vs cash** | Liquidity risk — payments not yet realized | `JenisLunas = 1` in `BTR_PiutangLunas`; FF1 `BayarGiro` | **Partial** — payment rows exist; no portal KPI |
| **High retur/potongan offset vs cash payment** | Receivable settled by non-cash adjustments | FF1 `Retur`, `Potongan`; `PiutangElement` | **Desktop only** (FF1 grid columns) |
| **DSO rising** | Average collection period lengthening | No formula in codebase | **Not available** — requires PO definition |
| **Payment received on severely overdue Faktur** | Late recovery — partial success | `PelunasanInfoDal` + `JatuhTempo` vs `LunasDate` | **Derivable** — not aggregated |
| **No payments recorded despite overdue balance** | Stale debt with no recovery activity | Open balance + absence of recent `LunasDate` per Faktur | **Derivable** |

### 2.4 Customer payment behavior and credit risk

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Customer exceeding Plafond** | Credit policy breach — collection urgency | M17 `PlafondBreach` signal | **Portal today** (M17) |
| **Suspended customer with open balance** | Policy violation with outstanding debt | M17 `SuspendedWithSales` (current-month sales); open balance on suspended master | **Partial** — sales signal only in M17 |
| **Dormant customer with open balance** | Legacy "zombie" debt — low recovery probability | M17 dormant rule + open `KurangBayar` | **Not computed** as combined signal |
| **Customer with high sales but rising overdue** | Active account with deteriorating payment behavior | M17 omzet + overdue per customer | **Derivable** — cross-signal not in attention list |
| **Chronic overdue customer still receiving new Faktur** | Credit control failure | Current-month Faktur + overdue balance | **Derivable** |

### 2.5 Billing pipeline and tagihan state

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Overdue Faktur not yet billed (Tagihan)** | Collection document not issued — process gap | `StatusPiutang = Tercatat` on open overdue rows | **Partial** — `StatusPiutang` on `BTR_Piutang`; not in FF1 DAL or portal |
| **Faktur stuck in Ditagihkan state** | Billed but unpaid — active collection cycle | `StatusPiutangEnum.Ditagihkan` | **Partial** — Desktop workflow state |
| **Tagih Ulang without payment** | Re-billing attempted — repeated failure | `BTR_TagihanFaktur.IsTagihUlang` | **Partial** — per-Faktur flag; PiutangTracker timeline |
| **Tanda Terima without subsequent payment** | Customer acknowledged bill but did not pay | `IsTandaTerima` + open `Sisa` | **Partial** — event in tracker |
| **Tagihan volume vs payment volume mismatch** | Billing activity not converting to cash | `BTR_Tagihan` dates vs `BTR_PiutangLunas.LunasDate` | **Not computed** |

### 2.6 Salesman and regional collection accountability

| Situation requiring attention | Business meaning | Existing capability | Availability |
| ----------------------------- | ---------------- | ------------------- | ------------ |
| **Salesman with high overdue but low collections** | Rep not recovering own book | M18 overdue exposure + FF2 payments by `SalesName` | **Partial** — sources exist separately |
| **Top performer by omzet with worst overdue** | Sales/collection conflict | M18 cross-rankings composable | **Not computed** |
| **Wilayah under-collecting vs peers** | Territory recovery underperformance | FF1 grouped by `WilayahName`; FF2 lacks wilayah | **Partial** |
| **Salesman piutang concentration** | Single rep owns disproportionate receivable risk | M18 `HighPiutangExposure`, `TopPiutangSalesmanPercent` | **Portal today** (M18) |

### 2.7 Workflow-derived attention points

From `docs/foundation/WORKFLOW.md` (Receivable Collection: Customer Receivable → Collection Visit → Customer Payment → Payment Recording):

| Workflow stage | When management cares | Portal support today |
| -------------- | --------------------- | -------------------- |
| Faktur → Piutang created | New debt entering collection pool | **Indirect** — open balance snapshots |
| Tagihan issued (FT2) | Billing step completed | **None** |
| Field collection visit | Visit occurred, outcome | **None** — check-in exists (RO1) but not collection-linked |
| Payment recorded (FT1) | Cash/recovery realized | **None** — `PelunasanInfo` / FF2 Desktop only |
| Tanda Terima / Tagih Ulang (FT3) | Acknowledgment or re-bill | **None** |
| Per-Faktur lifecycle review (FT5) | Audit trail | **None** |

---

## 3. Existing Dashboard Reuse Analysis

### 3.1 Management Attention Center (`/dashboard`) — collection-relevant metrics

| Metric | Source | Collection relevance | Reuse for M20 |
| ------ | ------ | -------------------- | ------------- |
| Overdue Customer (count) | Piutang snapshot → executive | Exposure headline | **Context only** — avoid duplicating as primary M20 KPI without reframing |
| Aging > 90 Day amount / % | Piutang aging buckets | Severity indicator | **Reuse logic** (`DaysOver90` bucket key) |
| Top Customer % | `#1 balance / TotalPiutang` | Concentration risk | **Reuse** — collection concentration signal |
| Critical Exposures — Top 5 Customers | Piutang Top N | Priority accounts | **Navigation link** — M20 may rank by **overdue** not total balance |
| Piutang card `RequiresAttention` | `OverdueCustomer > 0 \|\| over90 > 0` | Binary attention flag | **Pattern reuse** for M20 attention resolver |

**Assessment:** Executive dashboard provides **company-level exposure alerts**. M20 should add **recovery, workload, and effectiveness** dimensions the executive page omits.

### 3.2 Piutang Dashboard (`/dashboard/piutang`) — M14

| KPI / section | Collection relevance | M20 reuse rationale |
| ------------- | -------------------- | ------------------- |
| Total Piutang | Exposure baseline | **Do not duplicate** as headline — already domain dashboard |
| Total Customer (with open balance) | Breadth of debt | Supporting metric only |
| Overdue Customer | Collection attention count | **Reframe** in M20 as workload input, not primary differentiator |
| Aging pie (5 buckets) | Due-date distribution | **Reuse bucket definitions** from `DashboardPiutangAggregator` — same `JatuhTempo` rules |
| Top 10 Outstanding Customers | Collection priority by balance | M20 may add **Top 10 Overdue** or **Top 10 >90d** — different ranking |
| Snapshot source | `IPiutangOpenBalanceDal` | Authoritative open-balance input |

**Key distinction:** Piutang Dashboard answers *how much is owed and how old is it?* M20 should answer *what requires collection action, is recovery working, and where is risk concentrated from a collection lens?*

### 3.3 Customer Analytics (`/dashboard/customers`) — M17

| Signal / KPI | Collection relevance | M20 relationship |
| ------------ | -------------------- | ---------------- |
| Overdue (attention list) | Per-customer collection flag | **Overlap** — M20 should not replicate full customer attention list |
| Plafond breach | Credit/collection urgency | **Cross-reference** — candidate M20 attention signal |
| Dormant | Inactivity — different from overdue | M20 may combine **Dormant + open balance** as legacy debt |
| Top 10 Piutang | Balance ranking | Piutang Dashboard + M17 already cover |
| AgingOver90Amount (KPI) | Severe exposure | **Reusable metric** at company level |
| Segmentation by Wilayah | Regional view | M20 may add **wilayah collection exposure** — not in M17 segmentation purpose |

### 3.4 Salesman Performance (`/dashboard/salesmen`) — M18

| Signal / KPI | Collection relevance | M20 relationship |
| ------------ | -------------------- | ---------------- |
| High Overdue Exposure | Rep-level collection risk | **Overlap** — salesman overdue attribution in M18 |
| High Piutang Exposure | Receivable concentration by rep | Exposure, not recovery |
| Top 10 Piutang (by rep) | Ranking | M20 may add **Top 10 by overdue** or **collections received** |
| Collection Exposure attention cards | UI grouping | **Pattern reuse** — not same signals |

**M18 explicit deferral to M20:** "Collection payments lagging new billing" and "collection effectiveness / DSO."

### 3.5 Sales Dashboard — indirect collection context

| KPI | Collection relevance |
| --- | -------------------- |
| Total Omzet (month) | Numerator for billing pace vs collections |
| Total Customer | Not collection-specific |

### 3.6 Aggregator and bucket reuse (authoritative logic)

| Component | Location | Reuse |
| --------- | -------- | ----- |
| Aging bucket boundaries | `DashboardPiutangAggregator.ResolveAgingBucketKey` | **Copy constants** — inclusive 0, 1–30, 31–60, 61–90, >90 from `JatuhTempo` |
| Open balance filter | `KurangBayar > 1` / `Sisa > 1` | **Mandatory** — all dashboards |
| Customer key resolution | `CustomerCode` else `CustomerName` | **Mandatory** |
| Salesman piutang join | `IPiutangOpenBalanceWithSalesmanDal` | M20 salesman/wilayah workload |
| Attention list pattern | M17/M18/M19 `AttentionList` grain (entity × signal) | **UI/aggregator pattern** |
| Materialized snapshot worker | `RefreshDashboard*SnapshotWorker` | **Pattern** for dedicated `BTRPD_Collection*` domain |

---

## 4. Existing Report Reuse Analysis

### 4.1 Portal reports today

| Report | Route | Collection information | Drill-down role |
| ------ | ----- | ---------------------- | --------------- |
| **Piutang Report** | `/reports/piutang` | Open Faktur rows: Customer, Sales, Faktur, Tanggal, Jatuh Tempo, Kurang Bayar; footer Total Piutang, Total Customer; date field toggle (Jatuh Tempo / Piutang Date) | **Primary validation** for exposure KPIs; sort by Jatuh Tempo for collection queue |
| Sales Report | `/reports/sales` | Faktur list — billing pace context | Cross-check new debt creation |
| Inventory Report | — | None | Out of scope |
| Purchasing Report | — | None | Out of scope |

**Gap:** No portal report for **payments received** (FF2, FF4) or **Tagihan** activity.

### 4.2 Desktop finance reports (not in portal)

| Menu | Form | DAL | Collection information |
| ---- | ---- | --- | ---------------------- |
| FF1 | `PiutangSalesWilayahForm` | `IPiutangSalesWilayahDal` | Open balance grid with Sales, Wilayah, Jatuh Tempo, Kurang Bayar, BayarTunai, BayarGiro, Retur, Potongan, MateraiAdmin |
| FF2 | `PenerimaanPelunasanSalesForm` | `IPenerimaanPelunasanSalesDal` | **Collections summary** by date + salesman: cash, giro, retur, potongan, materai/admin, total bayar |
| FF4 | `PelunasanInfoForm` | `IPelunasanInfoDal` | Payment detail per line: customer, Faktur, payment date, cash, BG, remaining Sisa |
| FT1 | `LunasPiutang2Form` | `IPiutangLunasViewDal` | Operational payment entry — not aggregate report |
| FT2 | `TagihanForm` | Tagihan aggregate | Billing document creation per salesman |
| FT3 | `TandaTerimaTagihanForm` | `ITagihanFakturDal` | Tanda Terima / Tagih Ulang flags |
| FT5 | `PiutangTrackerForm` | `IPiutangTrackerDal` | Per-Faktur timeline: Catat Piutang, Tagihan, Tanda Terima, Tagih Ulang, Pelunasan |

### 4.3 KPI-to-report traceability matrix

| KPI candidate | Primary source DAL / aggregator | Validating report | Notes |
| ------------- | ------------------------------- | ----------------- | ----- |
| Total open piutang | `IPiutangOpenBalanceDal` | Piutang Report footer | Already M14 traceability |
| Overdue customer count | `DashboardPiutangAggregator` | Piutang Report — count distinct customers with `JatuhTempo < today` | Bucket rule must match aggregator |
| Aging bucket amounts | `DashboardPiutangAggregator` | Piutang Report — manual bucket sum on filtered rows | |
| Top N customer by balance | Piutang snapshot | Piutang Report — group by customer | |
| Top N customer by **overdue** balance | Derive from open balance rows | Piutang Report — filter overdue, then group | **New ranking** — not in M14 |
| Top N salesman by overdue exposure | `IPiutangOpenBalanceWithSalesmanDal` + bucket logic | Piutang Report — group by Sales | M18 has total piutang ranking |
| Wilayah overdue exposure | `IPiutangSalesWilayahDal` (all-open period) | FF1 Desktop export | Portal DAL needs all-open variant or reuse open-balance + wilayah join |
| Cash collected (period) | `IPenerimaanPelunasanSalesDal` | FF2 Desktop | **No portal report** |
| Payment detail | `IPelunasanInfoDal` | FF4 Desktop | **No portal report** |
| Payment mix (cash vs giro) | `BTR_PiutangLunas.JenisLunas` | FF4 / FF2 | |
| Retur/potongan offset on piutang | FF1 row columns | FF1 | Not in portal Piutang Report columns |
| Plafond breach count | `DashboardCustomerAggregator` | Customer master + Piutang Report | M17 |
| Legacy debt (dormant + open) | Customer last Faktur + open balance | Sales Report + Piutang Report | Cross-report |
| Billing vs payment gap | `BTR_Tagihan` + `BTR_PiutangLunas` | FT2 + FF2 | **Not implemented** |
| Per-Faktur collection history | `IPiutangTrackerDal` | FT5 Desktop | Single-Faktur drill-down only |
| Collections vs omzet (salesman) | `IPenerimaanPelunasanSalesDal` + `IFakturViewDal` | FF2 + Sales Report | M20 effectiveness candidate |

---

## 5. Collection Workflow Analysis

Investigation of BTR Desktop finance screens and application workers. **No new collection processes invented** — this reflects implemented behavior.

### 5.1 End-to-end receivable lifecycle (as implemented)

```text
Faktur created
    → Piutang recorded (CreatePiutangWorker — Total = GrandTotal)
    → StatusPiutang = Tercatat
    → [Optional] Tagihan document created (FT2) — groups Faktur per SalesPerson
    → StatusPiutang = Ditagihkan
    → [Optional] Tanda Terima / Tagih Ulang (FT3) — flags on TagihanFaktur
    → Payment recorded (FT1 Lunas Piutang) — BTR_PiutangLunas rows
    → PiutangBuilder.ReCalc — Sisa = Total + Potongan - Terbayar
    → StatusPiutang = Lunas (or Tercatat if partial)
```

### 5.2 Desktop screens and operational roles

| Screen | Code | Actor | Purpose |
| ------ | ---- | ----- | ------- |
| **Tagihan** | FT2 | Finance / Sales admin | Create billing document listing open Faktur for a **selected salesman**; print Tagihan; enforce Faktur belongs to same salesman |
| **Tanda Terima Tagihan** | FT3 | Finance | Mark invoices as acknowledged (`IsTandaTerima`) or re-bill (`IsTagihUlang`) |
| **Lunas Piutang** | FT1 | Finance | Record payment against Faktur; link to Tagihan; cash/giro/uang muka; retur/potongan/materai/admin adjustments |
| **Pelunasan Info** | FF4 | Finance / Management | List payments in date range with detail |
| **Penerimaan Pelunasan Sales** | FF2 | Management | Monitor collections received by date and salesman |
| **Piutang Sales Wilayah** | FF1 | Management | Review open receivables grouped by salesman and wilayah |
| **Piutang Tracker** | FT5 | Finance / Audit | Per-Faktur event timeline |

### 5.3 Collection assignment and ownership (workflow)

| Question | Implemented behavior |
| -------- | -------------------- |
| Who is assigned to collect? | **Salesman on Faktur** (`BTR_Faktur.SalesPersonId`). Tagihan is created **per salesman** — `BTR_Tagihan.SalesPersonId` must match Faktur salesman. |
| Is there a separate Collector role? | **No** entity or master table discovered. |
| Route-based collection queue? | `LunasPiutang2Form` and `TagihanForm` use `ISalesRuteDal` / `ISalesRuteBuilder` for **route-day customer lists** — operational UI, not persisted collection assignments. |
| Customer ownership for collection? | **Transactional** — invoicing salesman on each Faktur; customer master has **no** `SalesPersonId`. |
| Wilayah role? | Customer `WilayahId` on master; salesman `WilayahId` on `BTR_SalesPerson` — geographic dimension for reporting (FF1), not collection assignment rules. |

### 5.4 Payment recording workflow (FT1)

- Search Faktur by code; load open piutang (`Sisa > 1`).
- Select Tagihan document (open tagihan for customer; excludes `IsTagihUlang` in some paths).
- Enter payment: `JenisLunas` — Cash (0), Cek/BG (1), UangMuka (2).
- Optional adjustments via PiutangElement: Retur, Potongan, Materai, Admin.
- On save: writes `BTR_PiutangLunas`, recalculates `Sisa`, updates `StatusPiutang`.

### 5.5 What is NOT recorded

| Expected CRM/collection activity | BTR status |
| -------------------------------- | ---------- |
| Promise-to-pay date | **Not found** |
| Collection call / visit log | **Not found** (check-in is separate from payment) |
| Collector notes or follow-up outcomes | **Not found** (`Keterangan` on TagihanFaktur is free text per line — not aggregated) |
| Collection task assignment | **Not found** |
| Dispute reason | **Not found** |
| Payment plan / installment schedule | **Not found** |

**Conclusion:** M20 analytics must be built from **financial state and events** (balances, due dates, tagihan flags, payment postings) — not from collection activity tracking.

---

## 6. Collection Risk Analysis

Risks **measurable today** from existing data. Thresholds are **not defined** — business meaning only.

| Risk | Business meaning | Measurable from | Portal today |
| ---- | ---------------- | --------------- | ------------ |
| **Overdue balance risk** | Cash not received by due date | `JatuhTempo` vs today on open rows | Yes (company); per-entity derivable |
| **Severe aging risk** | Debt > 90 days past due — recovery doubtful | Aging bucket `DaysOver90` | Yes (company amount) |
| **Concentration risk** | Default of one customer/rep/wilayah hurts materially | Top 1 % of total piutang or overdue | Partial (customer top 1 in executive) |
| **Credit limit risk** | Customer owes more than approved Plafond | Open balance vs `Customer.Plafond` | M17 signal |
| **Legacy debt risk** | Inactive customer still owes | Last Faktur date + open balance | Not computed |
| **Non-cash settlement risk** | Receivable cleared by retur/potongan not cash | FF1 payment decomposition | Desktop only |
| **Giro liquidity risk** | Payments pending in Cek/BG | `JenisLunas = 1` totals | Desktop only |
| **Billing without recovery** | Tagihan issued but no pelunasan | Tagihan dates vs LunasDate on same Faktur | PiutangTracker per Faktur; not aggregate |
| **Salesman portfolio risk** | One rep accumulates overdue | Group overdue by invoicing salesman | M18 overdue exposure count |
| **Regional risk** | Wilayah concentration of overdue | Group FF1 by `WilayahName` | Not computed |
| **Recovery pace risk** | Collections not keeping up with sales | Month pelunasan vs month Faktur omzet | Not computed |
| **Policy risk** | Suspended customer with debt or new sales | `IsSuspend` + balance/sales | Partial (M17 suspended+sales) |
| **Data integrity risk** | `Sisa` out of sync with payments | `PiutangBuilder` vs persisted `Sisa` | Documented in investigation — monitor via reconciliation |

---

## 7. Existing Asset Discovery

### 7.1 DALs — open exposure (receivable state)

| Asset | Interface | Purpose | Portal usage |
| ----- | --------- | ------- | ------------ |
| `PiutangOpenBalanceDal` | `IPiutangOpenBalanceDal` | All-open rows: customer, JatuhTempo, KurangBayar | M14, M17 snapshots |
| `PiutangOpenBalanceWithSalesmanDal` | `IPiutangOpenBalanceWithSalesmanDal` | Above + SalesPersonId/Name | M18 snapshot |
| `PiutangSalesWilayahDal` | `IPiutangSalesWilayahDal` | Period-filtered FF1 rows with wilayah, payment decomposition | M10 Piutang Report, M14 live fallback |
| `PiutangReportDal` | `IPiutangReportDal` | Portal report wrapper on FF1 | M10 report API |

### 7.2 DALs — payments and collections (recovery state)

| Asset | Interface | Purpose | Portal usage |
| ----- | --------- | ------- | ------------ |
| `PenerimaanPelunasanSalesDal` | `IPenerimaanPelunasanSalesDal` | Collections by LunasDate + SalesName | **None** |
| `PelunasanInfoDal` | `IPelunasanInfoDal` | Payment line detail in period | **None** |
| `PIutangLunasViewDal` | `IPiutangLunasViewDal` | Piutang list for pelunasan entry UI | **None** |
| `PiutangTrackerDal` | `IPiutangTrackerDal` | Per-Faktur event union | **None** |

### 7.3 DALs — tagihan (billing state)

| Asset | Interface | Purpose | Portal usage |
| ----- | --------- | ------- | ------------ |
| `TagihanDal` / `TagihanFakturDal` | `ITagihanDal`, `ITagihanFakturDal` | CRUD and list Tagihan documents | **None** |
| `TandaTerimaTagihanViewDto` | via `ITagihanFakturDal` | Tanda Terima listing | **None** |

### 7.4 Aggregators and builders (portal)

| Asset | Purpose | M20 reuse |
| ----- | ------- | --------- |
| `DashboardPiutangAggregator` | Aging, overdue count, top customers | Bucket logic, customer grouping |
| `DashboardCustomerAggregator` | Customer attention, plafond, dormant | Cross-signals; avoid duplicating list |
| `DashboardSalesmanAggregator` | Salesman overdue/piutang exposure | Salesman overdue attribution |
| `DashboardExecutiveComposer` | Piutang attention promotion | >90d %, top customer % patterns |
| `PiutangBuilder` / `ReCalc` | Write-path balance rules | Understand Sisa authority |

### 7.5 Snapshot tables (reporting context)

| Table | Content |
| ----- | ------- |
| `BTRPD_PiutangKpi` | TotalPiutang, TotalCustomer, OverdueCustomer |
| `BTRPD_PiutangAging` | Five bucket amounts |
| `BTRPD_PiutangTopCustomer` | Top 10 by balance |
| `BTRPD_Customer*` | Customer attention, top piutang |
| `BTRPD_Salesman*` | Salesman overdue/piutang exposure |

**No `BTRPD_Collection*` tables exist.**

### 7.6 Domain tables (finance)

| Table | Collection relevance |
| ----- | -------------------- |
| `BTR_Piutang` | Header: Total, Potongan, Terbayar, Sisa, DueDate, StatusPiutang |
| `BTR_PiutangLunas` | Payment lines: LunasDate, Nilai, JenisLunas, TagihanId |
| `BTR_PiutangElement` | Retur, Potongan, Materai, Admin adjustments |
| `BTR_Tagihan` | Billing document header: TagihanDate, SalesPersonId |
| `BTR_TagihanFaktur` | Faktur lines on bill: NilaiTagih, IsTandaTerima, IsTagihUlang |
| `BTR_Faktur` | SalesPersonId, CustomerId, FakturDate, DueDate |
| `BTR_Customer` | Plafond, IsSuspend, WilayahId |

### 7.7 Desktop analytics not exposed

| Capability | Form | M20 relevance |
| ---------- | ---- | ------------- |
| Collections by salesman/day | FF2 | **High** — recovery monitoring |
| Payment detail | FF4 | Drill-down validation |
| Open piutang with payment breakdown | FF1 | Non-cash settlement analysis |
| Per-Faktur lifecycle | FT5 | Exception investigation |

---

## 8. Exception-Based Management Analysis

Focus: collection situations deserving **management attention** rather than balance reporting. Threshold values are **candidates only** — except where inherited from M17/M14 or specified in Section 13.

**Approved for M20 Attention List (Section 13.3):** ChronicOverdue · LegacyDebt · PlafondBreachOverdue · HighOverdueWorkload · LowRecoveryVsBilling · WilayahHotspot — plus reuse of M17 Overdue and M18 HighOverdueExposure where relevant.

### 8.1 Warning condition candidates

| ID | Condition candidate | Business meaning | Data source | Computed today? |
| -- | ------------------- | ---------------- | ----------- | --------------- |
| W-K01 | Any overdue open balance (company) | Collection action required somewhere | Piutang snapshot | **Yes** |
| W-K02 | Customer with overdue balance | Account-level follow-up | Open rows per customer | **M17 Overdue** |
| W-K03 | Customer with > 90d overdue amount > 0 | Chronic/severe debt on account | Per-customer bucket | **Derivable** |
| W-K04 | Salesman with any overdue on book | Rep-level collection exposure | M18 HighOverdueExposure | **Yes** (M18) |
| W-K05 | Wilayah overdue amount in top concentration | Regional hotspot | FF1 grouped by Wilayah | **No** |
| W-K06 | Plafond breach with overdue | Credit + collection urgency | M17 PlafondBreach + overdue | **Partial** |
| W-K07 | Dormant customer with open balance | Low-velocity recovery target | Last Faktur + Sisa | **No** |
| W-K08 | Collections this month below TBD vs prior month | Recovery slowing | PenerimaanPelunasanSales | **No** |
| W-K09 | Billing (omzet) exceeds collections for salesman | Net receivable growth on rep | Faktur + Pelunasan by rep | **No** |
| W-K10 | High giro share of period collections | Liquidity concern | JenisLunas mix | **No** |
| W-K11 | High retur/potongan share of settlements | Non-cash clearance pattern | FF1 decomposition | **No** |
| W-K12 | Overdue Faktur in Ditagihkan state long duration | Billed but unpaid — stuck cycle | StatusPiutang + dates | **No** |
| W-K13 | Tagih Ulang flag on open overdue Faktur | Repeated billing failure | TagihanFaktur | **No** aggregate |
| W-K14 | Top 1 customer overdue % of total overdue | Overdue concentration (not total piutang) | Rank overdue by customer | **No** |

### 8.2 Critical condition candidates

| ID | Condition candidate | Business meaning | Data source |
| -- | ------------------- | ---------------- | ----------- |
| C-K01 | > 90d amount > TBD % of total piutang | Portfolio quality crisis | Aging buckets |
| C-K02 | Single customer overdue > TBD % of total overdue | One account blocks recovery |
| C-K03 | Single salesman overdue > TBD % of company overdue | Rep portfolio crisis |
| C-K04 | Zero collections in period with rising open balance | Complete recovery failure |
| C-K05 | Dormant + overdue > TBD absolute | Likely write-off candidate |
| C-K06 | Suspended customer with overdue > TBD | Policy + financial exposure |

### 8.3 Aging indicators (reuse M14 definitions)

| Bucket | Rule | Collection meaning |
| ------ | ---- | ------------------ |
| Current | `DaysOverdue ≤ 0` | Not yet due — preventive |
| 1–30 Days | `1–30` | Early overdue — first action window |
| 31–60 Days | `31–60` | Escalation zone |
| 61–90 Days | `61–90` | Serious delay |
| > 90 Days | `> 90` | Critical / chronic |

**M20 addition candidates:** Rankings and attention flags **by overdue amount per bucket**, not only company-wide pie.

### 8.4 Collection effectiveness indicators (candidates — require PO formulas)

| Indicator | Numerator / denominator (candidates) | Data available |
| --------- | ------------------------------------ | -------------- |
| **Collection rate (period)** | Sum pelunasan in month / sum open balance at start | Pelunasan yes; start balance needs snapshot history or approximation |
| **Cash recovery (period)** | Sum cash pelunasan / sum total pelunasan | Yes |
| **DSO** | Avg days from FakturDate or JatuhTempo to LunasDate | Row-level dates yes; formula not implemented |
| **Overdue ratio** | Overdue amount / total piutang | Yes from buckets |
| **Recovery vs billing** | Month pelunasan / month Faktur GrandTotal | Yes from separate DALs |

### 8.5 Ranking indicators

| Ranking | Exists | M20 collection use |
| ------- | ------ | ------------------ |
| Top 10 customer by total balance | M14 | Baseline — may duplicate |
| Top 10 customer by **overdue** balance | No | **Collection priority** |
| Top 10 customer by **>90d** balance | No | **Chronic priority** |
| Top 10 salesman by overdue | No (M18 has exposure count) | Workload routing |
| Top 10 wilayah by overdue | No | Regional intervention |
| Top 10 collectors of cash (period) | No | **Recovery leaders** — by SalesName from FF2 |
| Bottom collections vs omzet (salesman) | No | **Under-recovery** |

---

## 9. Existing Desktop Capability Analysis

### 9.1 Collection-oriented screens summary

| Category | Screens | Portal exposure potential |
| -------- | ------- | ------------------------- |
| **Exposure monitoring** | FF1 Piutang Sales Wilayah | Partial — M10 report is subset of FF1 columns |
| **Recovery monitoring** | FF2 Penerimaan Pelunasan Sales, FF4 Pelunasan Info | **High value for M20** — not in portal |
| **Operational collection** | FT1 Lunas Piutang, FT2 Tagihan, FT3 Tanda Terima | Read-only analytics only — transactional in Desktop |
| **Audit trail** | FT5 Piutang Tracker | Per-Faktur drill-down — not dashboard aggregate |
| **Field visits** | RO1 Check-In Info | **M25** — not collection effectiveness |

### 9.2 FF2 — Penerimaan Pelunasan Sales (primary recovery monitor)

- Groups `BTR_PiutangLunas` by `LunasDate` (day) and `SalesPersonName`.
- Sums: `BayarTunai`, `BayarGiro`, `Retur`, `Potongan`, `MateraiAdmin`, `TotalBayar`.
- User-selected period — portal would need **fixed period policy** (e.g. current month) per V1 report conventions.

### 9.3 FF1 — Piutang Sales Wilayah (richest open-receivable grid)

- Includes wilayah and cumulative payment/adjustment columns per **open** Faktur.
- Desktop groups Sales → Wilayah; Excel export hierarchy.
- Portal `PiutangReportDal` uses same DAL but **omits** Retur, BayarTunai, BayarGiro, Wilayah from report row (verify `PiutangReportRow` mapping).

### 9.4 FT1 — Lunas Piutang (operational workflow reference)

- Route-day integration via `ISalesRuteBuilder` — shows how finance uses **sales route** to find customers for payment entry.
- **Not** a metric store — payments land in `BTR_PiutangLunas`.

### 9.5 Reusable business knowledge before new analytics

1. **Open balance authority:** `BTR_Piutang.Sisa` with `> 1` threshold.
2. **Aging anchor:** `DueDate` / `JatuhTempo` — not `PiutangDate` for collection aging (portal dashboard uses JatuhTempo; report allows toggle).
3. **Salesman attribution:** invoicing `SalesPersonId` on Faktur.
4. **Payment types:** `JenisLunasEnum` — Cash, CekBg, UangMuka.
5. **Adjustment types:** PiutangElement — Retur, Potongan, Materai, Admin.
6. **Tagihan lifecycle flags:** IsTandaTerima, IsTagihUlang — binary events, not notes.
7. **No collection CRM data** — scope boundary for M20.

---

## 10. Ownership Analysis

### 10.1 Entity relationships (collection perspective)

```text
Customer (1) ──< Faktur (N) ── (1) Piutang
                    │
                    └── SalesPerson (invoicing salesman)

Tagihan (1) ──< TagihanFaktur (N) ──> Faktur
    │
    └── SalesPersonId (billing salesman — must match Faktur salesman)

Piutang (1) ──< PiutangLunas (N)  [payments]
Piutang (1) ──< PiutangElement (N) [adjustments]

Customer ── Wilayah (master geographic)
SalesPerson ── Wilayah (territory assignment)
```

### 10.2 Ownership model candidates

| Model | Definition | Authoritative in BTR? | Reliability |
| ----- | ---------- | --------------------- | ----------- |
| **Invoicing salesman** | `BTR_Faktur.SalesPersonId` on each open Faktur | **Yes** — used in FF1, M18, Piutang Report `Sales` column | **High** for Faktur-level collection accountability |
| **Tagihan salesman** | `BTR_Tagihan.SalesPersonId` | **Yes** — enforced in TagihanForm validation | **High** for billing actions; aligns with Faktur salesman |
| **Customer master salesman** | — | **Does not exist** | N/A |
| **Collector** | Separate role | **Does not exist** | N/A |
| **Route ownership** | `BTR_SalesRute` per SalesPersonId | **Yes** for field planning | **Medium** — operational queue, not financial attribution |
| **Wilayah** | Customer.WilayahId | **Yes** for geographic segmentation | **High** for regional reporting; not assignment |

### 10.3 Approved ownership for M20

| Metric family | Attribution | PO decision |
| ------------- | ----------- | ----------- |
| Open overdue exposure | **Invoicing salesman** | Q19 |
| Collections received | **Invoicing salesman** on paid Faktur | Q19 — `PenerimaanPelunasanSalesDal` pattern |
| Wilayah exposure / rankings | **Customer.WilayahId** | Q20 |
| Customer-level attention | **Customer** | Reuse M17 patterns where relevant (Q13) |
| Tagihan pipeline | **Not headline** — optional supporting only | Q17, Tagihan PO note |

**Avoid:** Introducing a new collector assignment rule not present in Desktop.

---

## 11. Dashboard Layout — Approved

**Product Owner decision:** Attention-First layout with Recovery Summary section (Q4). Route: `/dashboard/collection`. Sidebar label: **Collection** (Q27).

### 11.1 Approved page structure

**Route:** `/dashboard/collection`  
**Title:** Collection (sidebar) / Collection Dashboard (page)  
**Audience:** All authenticated users — same visibility model as M16/M17/M18 (no role-based routing)

### 11.2 Approved wireframe (fixed section order)

```text
Collection Dashboard                                    [/dashboard/collection]

1. Collection Attention Cards
┌──────────────────────────────────────────────────────────────────────────┐
│  Overdue Exposure · >90d Exposure · Overdue Concentration %              │
│  Cash Collected MTD · Recovery vs Billing % · Legacy Debt Count          │
└──────────────────────────────────────────────────────────────────────────┘

2. Recovery Summary
┌──────────────────────────────────────────────────────────────────────────┐
│  Cash Collected MTD (current month)                                       │
│  Recovery vs Billing % = Month Collections ÷ Month Faktur Omzet × 100   │
│  Payment Mix (company): Cash | Giro | Adjustment                          │
└──────────────────────────────────────────────────────────────────────────┘

3. Aging Risk Summary
┌──────────────────────────────────────────────────────────────────────────┐
│  Overdue-only aging: 1–30 | 31–60 | 61–90 | >90 (excludes Current)      │
│  Reuses M14 >90 Days bucket definition — NOT full Piutang Dashboard pie   │
└──────────────────────────────────────────────────────────────────────────┘

4. Collection Attention List
┌──────────────────────────────────────────────────────────────────────────┐
│  Customer × Signal · Salesman × Signal                                    │
│  Mandatory: ChronicOverdue · LegacyDebt · PlafondBreachOverdue            │
│             HighOverdueWorkload · LowRecoveryVsBilling · WilayahHotspot   │
│  Reuse M17/M18 signals where relevant (Overdue, HighOverdueExposure)      │
└──────────────────────────────────────────────────────────────────────────┘

5. Top Overdue Customers          (rank by overdue balance, not total)
6. Top Overdue Salesmen           (rank by overdue balance; invoicing salesman)
7. Top Overdue Wilayah             (Customer.WilayahId; rank by overdue balance)

8. Navigation
┌──────────────────────────────────────────────────────────────────────────┐
│  → Piutang Dashboard · Customer Analytics · Salesman Performance          │
│  → Piutang Report (drill-down; no Pelunasan Report in M20)                │
└──────────────────────────────────────────────────────────────────────────┘
```

**Design constraint:** M20 **complements, does not duplicate** M14 — no Total Piutang headline, no full Current+overdue aging pie as primary view.

### 11.3 Drill-down conventions

| UI element | Target |
| ---------- | ------ |
| Customer row | `/reports/piutang?q={CustomerName}` |
| Salesman row | `/reports/piutang?q={SalesName}` |
| Wilayah row | Piutang Report — search limitation noted; architect may expose wilayah filter or cross-link only |
| Recovery KPIs | Dashboard only — no FF2/FF4 portal report (Q10) |

### 11.4 Executive Dashboard promotion (post-M20)

After M20 implementation, promote to Executive Dashboard (Q25):

| KPI | Business meaning |
| --- | ---------------- |
| Cash Collected MTD | Cash recovery pace in current month |
| Recovery vs Billing % | Collections keeping up with new billing |
| Overdue Concentration % | Share of overdue exposure in top account(s) — architect to align denominator with Overdue Concentration card on Collection dashboard |

---

## 12. Gap Analysis

### 12.1 Information already available (portal or authoritative DAL)

| Information | Where |
| ----------- | ----- |
| Total open piutang, overdue customer count | M14 snapshot |
| Five-bucket aging by JatuhTempo | M14 aggregator |
| Top 10 customers by total balance | M14 snapshot |
| Per-customer overdue, plafond breach, dormant (sales) | M17 snapshot |
| Per-salesman overdue exposure, top piutang | M18 snapshot |
| Executive >90d %, top customer % | M16 composer |
| Open Faktur list with Jatuh Tempo, Sales, Customer | Piutang Report |
| Payment rows with dates, amounts, types | `BTR_PiutangLunas` (DB) |
| Collections by day and salesman | `PenerimaanPelunasanSalesDal` |
| Payment line detail | `PelunasanInfoDal` |
| Wilayah on open piutang rows | `PiutangSalesWilayahDal` |
| Tagihan and lifecycle flags | `BTR_Tagihan*`, `PiutangTrackerDal` |

### 12.2 Information partially available (M20 will materialize)

| Information | Gap | M20 disposition |
| ----------- | --- | --------------- |
| **Overdue-only rankings** | Rankings today by total balance | **In scope** — Top Overdue Customers/Salesmen/Wilayah (Q14) |
| **Wilayah collection aggregates** | No portal aggregator | **In scope** — required dimension (Q15) |
| **Payment mix / non-cash settlement** | Desktop FF2 only | **In scope** — company Payment Mix (Q9) |
| **Recovery vs billing** | DALs exist separately | **In scope** — mandatory KPI (Q8) |
| **Legacy debt (dormant + balance)** | M17 dormant separate from balance | **In scope** — LegacyDebt signal |
| **Tagihan pipeline KPIs** | Not in reporting DALs | **Optional supporting only** (Q17) |
| **Piutang Report wilayah column** | Not in portal report row | Drill-down gap — architect decision |
| **Chronic overdue per customer** | Company >90d only in M17 KPI | **In scope** — ChronicOverdue signal |
| **LowRecoveryVsBilling per salesman** | Not computed | **In scope** — attention signal |

### 12.3 Information excluded from M20 scope

| Information | Disposition |
| ----------- | ----------- |
| **Collection activity / follow-up log** | **Excluded** — not recorded in BTR (Q2) |
| **Promise-to-pay** | **Excluded** (Q2) |
| **DSO** | **Excluded** (Q7) |
| **Historical aging deterioration trend** | **Excluded** (Q22) |
| **Visit-to-payment conversion** | **Excluded** — M25 (Q29) |
| **Portal Pelunasan / Collections report** | **Excluded** — dashboard only (Q10) |
| **Tagihan pipeline headline KPIs** | **Optional supporting only** — not dashboard drivers (Q17, Tagihan PO note) |
| **Retur analytics** | **Excluded** as standalone dimension (Q30) |
| **Salesman sales performance** | **Excluded** (Q28) |
| **Event-driven refresh after pelunasan** | **Excluded** (Q24) — use 30-minute scheduled refresh (Q23) |

---

## 13. Final Product Decisions (Authoritative)

**Status:** All open questions resolved by Product Owner — 2026-06-08.  
**Audience:** Architect — use this section as scope input. Do not re-decide business rules listed here.

### 13.1 Scope and philosophy

| # | Decision |
| - | -------- |
| Q1 | **Primary question:** *Which receivables require collection attention and is recovery working?* |
| Q2 | **Exclude** collection CRM: follow-up tracking, promise-to-pay, visit outcomes, collection activity logging |
| Q3 | M20 **complements, does not duplicate** M14 Piutang Dashboard |
| Q4 | **Attention-First layout with Recovery Summary** section |

### 13.2 Recovery and effectiveness metrics

| # | Decision |
| - | -------- |
| Q5 | **Cash Collections mandatory** |
| Q6 | **Default period:** current calendar month |
| Q7 | **DSO excluded** from M20 |
| Q8 | **Recovery vs Billing mandatory** KPI |
| Q9 | **Payment Mix** (Cash vs Giro vs Adjustment) — **company level only** |
| Q10 | **Dashboard only** — no Pelunasan Report in M20 |

**Recovery vs Billing % (authoritative formula):**

```text
Recovery vs Billing % = Current Month Collections ÷ Current Month Faktur Omzet × 100
```

| Term | Definition | Source |
| ---- | ---------- | ------ |
| Current Month Collections | Sum of collections received in current calendar month | `IPenerimaanPelunasanSalesDal` / `BTR_PiutangLunas` — align with FF2 `TotalBayar` semantics (Cash + Giro); architect to confirm adjustment handling in Payment Mix vs numerator |
| Current Month Faktur Omzet | Sum of non-void Faktur `GrandTotal` in current calendar month | `IFakturViewDal` — same as Sales Dashboard Total Omzet |

**Cash Collected MTD:** Company-level cash collections in current calendar month (mandatory attention card and executive promotion).

**Payment Mix (company only):** Cash · Giro · Adjustment components for current month — sourced from pelunasan/piutang element decomposition (FF2 pattern).

### 13.3 Exposure and attention signals

| # | Decision |
| - | -------- |
| Q11 | **Mandatory attention signals:** ChronicOverdue · LegacyDebt · PlafondBreachOverdue · HighOverdueWorkload · LowRecoveryVsBilling · WilayahHotspot |
| Q12 | **Attention list grain:** Customer × Signal **and** Salesman × Signal |
| Q13 | **Reuse** M17 and M18 attention signals where relevant (e.g. Overdue, HighOverdueExposure) |
| Q14 | **Rankings use Overdue Balance**, not Total Balance |
| Q15 | **Wilayah required** dimension |
| Q16 | Reuse existing **> 90 Days** exposure definition from M14 (`DaysOver90` bucket) |

**Signal business meanings (for architect — thresholds TBD unless inherited from M17):**

| Signal | Entity | Business meaning |
| ------ | ------ | ---------------- |
| **ChronicOverdue** | Customer | Customer has open balance in M14 `DaysOver90` bucket |
| **LegacyDebt** | Customer | Dormant customer (M17 90-day rule) with open balance |
| **PlafondBreachOverdue** | Customer | Plafond breach (M17 rule) **and** overdue balance |
| **HighOverdueWorkload** | Salesman | Rep with material overdue exposure on invoiced book — align with M18 HighOverdueExposure where applicable |
| **LowRecoveryVsBilling** | Salesman | Rep's month collections lag month Faktur omzet — architect to define comparison rule consistent with company Recovery vs Billing % |
| **WilayahHotspot** | Wilayah | Wilayah with disproportionate share of company overdue — architect to define concentration rule |
| **Overdue** (reuse M17) | Customer | Any non-Current aging bucket balance |
| **HighOverdueExposure** (reuse M18) | Salesman | Any overdue on rep's invoiced open Faktur |

### 13.4 Tagihan and collection administration

| # | Decision |
| - | -------- |
| Q17 | **Tagihan pipeline optional supporting indicators only** — not headline KPIs |
| Q18 | **Tanda Terima without payment** — supporting information only, not primary management KPI |

**PO note — Tagihan:** Tagihan is an administrative document. Operational collection effectiveness is measured through outstanding piutang, overdue exposure, aging, pelunasan, and Recovery vs Billing. Tagihan lifecycle metrics (Tagihan, Tanda Terima, Tagih Ulang) are not actively managed by operations and must not drive dashboard design. May surface as supporting information if inexpensive.

### 13.5 Ownership and dimensions

| # | Decision |
| - | -------- |
| Q19 | Collection ownership follows **Invoicing Salesman** |
| Q20 | Wilayah uses **Customer.WilayahId** |

### 13.6 Data architecture (business-level)

| # | Decision |
| - | -------- |
| Q21 | **Dedicated snapshot domain:** `BTRPD_Collection*` |
| Q22 | **Aging deterioration trend excluded** |
| Q23 | **Refresh cadence:** 30 minutes |
| Q24 | **Event-driven refresh after pelunasan excluded** |

### 13.7 Executive dashboard and navigation

| # | Decision |
| - | -------- |
| Q25 | **Promote** after M20: Cash Collected MTD · Recovery vs Billing % · Overdue Concentration % |
| Q26 | **Route:** `/dashboard/collection` |
| Q27 | **Sidebar label:** Collection |

### 13.8 Explicit exclusions

| # | Decision |
| - | -------- |
| Q28 | Exclude salesman **sales-performance** metrics (target, omzet rankings) |
| Q29 | Exclude route, check-in, effective call, sales productivity — **M25** |
| Q30 | Exclude **retur analytics** as standalone dimension |

---

## 14. Collection KPI Definitions (Approved)

All monetary values in IDR. Collection metrics use **current calendar month** unless noted. Exposure metrics use **all-time open balance** with `KurangBayar > 1`.

| KPI | Period / scope | Formula / rule | Business meaning |
| --- | -------------- | ---------------- | ---------------- |
| **Cash Collected MTD** | Current month | Sum collections in month (align FF2 TotalBayar / cash+giro policy per architect) | How much cash recovery occurred this month |
| **Recovery vs Billing %** | Current month | Month Collections ÷ Month Faktur Omzet × 100 | Are collections keeping pace with new billing |
| **Payment Mix — Cash** | Current month, company | Share of cash in month collections | Liquidity composition |
| **Payment Mix — Giro** | Current month, company | Share of giro/cek in month collections | Non-immediate liquidity |
| **Payment Mix — Adjustment** | Current month, company | Share of retur/potongan/materai/admin in settlements | Non-cash clearance of receivables |
| **Overdue Exposure** | All-time open | Sum `KurangBayar` where `JatuhTempo < today` | Total past-due balance requiring collection |
| **>90d Exposure** | All-time open | Sum in M14 `DaysOver90` bucket | Severely aged overdue |
| **Overdue Concentration %** | All-time open | Top-1 overdue customer ÷ total overdue × 100 (or PO-aligned rule) | Dependency on worst overdue account |
| **Legacy Debt Count** | All-time open | Count customers: M17 dormant **and** open balance | Zombie receivables |
| **Aging Risk Summary** | All-time open, overdue only | Buckets 1–30, 31–60, 61–90, >90 — **exclude Current** | Where overdue sits by severity |
| **Top Overdue Customer** | All-time open | Top 10 by sum overdue balance per customer | Collection priority ranking |
| **Top Overdue Salesman** | All-time open | Top 10 by sum overdue balance per invoicing salesman | Rep workload / risk ranking |
| **Top Overdue Wilayah** | All-time open | Top 10 by sum overdue balance per Customer.WilayahId | Regional hotspot ranking |

**Overdue balance per row:** `KurangBayar` where `ResolveAgingBucketKey(JatuhTempo, today) != "Current"` — same aging anchor as M14.

**Traceability:** Overdue exposure totals must reconcile with Piutang Report rows filtered to past-due. Month collections must reconcile with FF2 Desktop for same period (validation sample).

---

## 15. Relationship to Other Milestones

| Milestone | Relationship to M20 |
| --------- | --------------------- |
| **M14 Piutang Dashboard** | *How much money is owed?* — M20 **complements**; does not duplicate Total Piutang / full aging pie |
| **M17 Customer Analytics** | *Which customers require attention?* — M20 reuses Overdue/plafond/dormant logic; adds collection recovery lens |
| **M18 Salesman Performance** | *Which salesmen require attention?* — M20 reuses HighOverdueExposure; adds LowRecoveryVsBilling |
| **M19 Inventory Risk** | No overlap |
| **M25 Sales Force Effectiveness** | Route, check-in, effective call — **out of M20** |
| **M16 Executive** | Promote Cash Collected MTD, Recovery vs Billing %, Overdue Concentration % **after** M20 ships |

---

## 16. Architect Handoff Checklist

Product Owner decisions recorded — ready for implementation planning.

- [x] M20 scope boundary: exposure + recovery; no CRM; Tagihan not headline
- [x] Attention signals and list grain (customer + salesman × signal)
- [x] Period semantics: current calendar month for recovery; all-time open for exposure
- [x] Recovery vs Billing % formula authoritative
- [x] DSO excluded
- [x] Dedicated `BTRPD_Collection*` snapshot; 30-minute refresh
- [x] Differentiation from M14 (no duplicate headlines)
- [x] Drill-down: Piutang Report only; no Pelunasan Report
- [x] Wilayah required; rankings by overdue balance
- [x] Executive promotion KPIs identified (post-M20)
- [x] Exclusions: M18 sales perf, M25 field activity, retur analytics, aging trends, event-driven refresh

---

## Appendix A — Desktop Menu Reference (Finance / Collection)

| Code | Form | Function |
| ---- | ---- | -------- |
| FF1 | Piutang Sales Wilayah | Open receivable analysis |
| FF2 | Penerimaan Pelunasan Sales | Collections received by date/salesman |
| FF4 | Pelunasan Info | Payment detail report |
| FT1 | Lunas Piutang | Payment entry |
| FT2 | Tagihan | Billing document creation |
| FT3 | Tanda Terima Tagihan | Acknowledgment / re-bill flags |
| FT5 | Piutang Tracker | Per-Faktur lifecycle timeline |

## Appendix B — Key business rules (do not reimplement in portal SQL)

| Rule | Source |
| ---- | ------ |
| Open balance: `Sisa > 1` / `KurangBayar > 1` | M5/M10/M14 domain docs |
| Aging from `JatuhTempo` vs today | `DashboardPiutangAggregator` |
| Customer key: Code else Name | All aggregators |
| Salesman on open piutang: Faktur `SalesPersonId` | M18, FF1 |
| Payment types: `JenisLunasEnum` | `btr.domain` |
| Piutang balance: `Sisa = Total + Potongan - Terbayar` | `PiutangBuilder.ReCalc` |
| Dormant: 90 days, prior history, exclude active month | M17 |
| Plafond breach: balance > Plafond when Plafond > 0 | M17 |
| Recovery vs Billing %: Month Collections ÷ Month Faktur Omzet × 100 | PO M20 |
| Rankings by overdue balance (not total KurangBayar) | PO M20 |
| Wilayah: Customer.WilayahId | PO M20 |
| Collection snapshot refresh: 30 minutes | PO M20 |
| Portal read-only | All portal features |

---

*End of analysis — M20 Collection Dashboard*
