# Feasibility Assessment — Principal Growth Planning

| Field | Value |
| ----- | ----- |
| Document | `FEASIBILITY-ASSESSMENT-principal-growth-planning.md` |
| Status | Reassessment — analysis only; no implementation |
| Date | 2026-09-05 |
| Supersedes (planning aggregate only) | [sales-target-planning-feasibility.md](./sales-target-planning-feasibility.md) |
| Related | [sales-person-principal-target/feature.md](../../features/sales-person-principal-target/feature.md), [dashboard-17-pu01-purchasing.md](../../features/btr-portal/dashboard-17-pu01-purchasing.md), [dashboard-02-sa02-sales-forecast.md](../../features/btr-portal/dashboard-02-sa02-sales-forecast.md), Supplier Entity Analytics (M32.10) |

---

## 1. Executive Summary

The revised hierarchy — **Company Goal → Principal Growth Planning → (optional) Salesman Allocation → Execution** — is a **stronger business fit** than salesman-first planning.

It matches:

- how Owners discuss Principals, programs, and commitments;
- shared customer ownership across Principal assignments;
- the existing target storage model (`SalesPerson × Principal × Month`).

It does **not** mean Principal planning is analytically cheaper than salesman planning. Portal Principal surfaces today are **purchase/inventory-first**. Company **sales-out by Principal** is computable (`FakturItem → Brg.SupplierId`) and partially present as `SalesOutAmount`, but **sales growth history, pace forecast, customer dormancy, and collection quality are not first-class Principal KPIs**.

**Final verdict: GO WITH REDUCTIONS**

Ship a Principal Growth Planning Workspace focused on company goal balancing, Principal sales baselines, Safe/Normal/Stretch, and portfolio gap. Defer Growth Opportunity decomposition, Principal collection quality, hard lock, and salesman allocation UI to later phases.

---

## 2. Review of Previous Feasibility Findings

Source: [sales-target-planning-feasibility.md](./sales-target-planning-feasibility.md) (Salesman-primary planning).

### 2.1 Findings that remain valid

| Finding | Why it still holds |
| ------- | ------------------ |
| **Planning Workspace > data-entry screen** | Value still comes from context → recommendation → Owner decision, not from prettier grids. |
| **Company Goal + assigned sum + gap** | Portfolio balancing is aggregate-agnostic; only the row grain changes. |
| **Safe / Normal / Stretch scenarios** | Deterministic history-based bands remain the right recommendation pattern. |
| **Explainable reasons (1–3 bullets)** | Same Inventory/Collection Optimization trust model. |
| **Advisory difficulty / risk class** | Still useful as `Target ÷ Expected` bands; Expected becomes Principal expected sales. |
| **Deterministic formulas, not AI/ML** | Unchanged Portal analytics philosophy. |
| **Portal read-only vs planning writes** | Architecture constraint unchanged; SM6 remains write home unless an exception is approved. |
| **Hard lock / reopen deferred from MVP** | Mid-month revision culture and SM6 last-write-wins still argue against hard lock in v1. |
| **Hybrid table + detail > pure cards** | Even more important now — Principals outnumber salesmen (~200–250 vs ~50–80). |
| **No silent auto-save of recommendations** | Unchanged adoption risk. |
| **Planning is Sales Optimization maturity** | Still the natural step after SA02 company forecast. |

### 2.2 Findings that become invalid or inverted

| Previous finding | Status | Justification |
| ---------------- | ------ | ------------- |
| Primary planning row = **Salesman** | **Invalid** | Shared Principal assignment means customer growth and Principal commitments are not cleanly owned by one salesman. Owner attention is Principal-led. |
| Salesman history (`BTRPD_SalesmanRepHistory`) as main recommendation input | **Invalid as primary** | Still useful for optional allocation; Principal sales-out history must become the primary baseline store. |
| Customer / Active / Dormant / Piutang as **readily available** planning card fields | **Invalid at Principal grain** | Those metrics are Available for salesman/customer; at Principal they are minor-work, new analytics, or not feasible (collection). |
| Visit completion as Phase 2 card signal | **Invalid for Principal workspace** | Visits are salesman/day-owned; not a Principal planning signal. |
| Territory potential (already deferred) | Remains invalid | Still undefined; also not Principal-keyed. |
| MVP: plan **rep total**, allocate to Principals by mix | **Inverted** | Correct direction is plan **Principal total**, optionally allocate to assigned salesmen. |
| “Principal-level scenario cards = out of MVP” | **Inverted** | Principal scenarios are now the core MVP surface. |
| Card scalability guidance based on ≤15 / 15–40 / >40 **salesmen** | **Must be recalibrated** | ~3× more Principal entities; table-first is mandatory from day one. |
| SF01 attention/coaching as primary planning companion | **Secondary** | SF01 remains execution/coaching; PU01 + sales-out Principal analytics become planning companions. |
| Assumption that salesman planning better matches Owner workflow | **Invalid** | New business observation (Principal conversations dominate) reverses this. |

### 2.3 What the previous study got right about Principals

The original study already noted that SM6 truth is Principal-scoped and that allocating into Principal lines was required. The error was treating Principal as a **persistence detail** rather than the **decision aggregate**.

---

## 3. Business Fit Reassessment

### 3.1 Does Principal-first reflect distributor management?

**Yes — better than salesman-first.**

| Business fact | Planning implication |
| ------------- | -------------------- |
| Salesmen sell only assigned Principals | Growth commitments are Principal-bounded. |
| One customer ↔ many salesmen via Principals | Customer growth cannot be planned as a single-rep target without double-counting risk. |
| Owner talks Principals, programs, opportunities | Planning UI should speak Principal language. |
| Targets already stored as SalesPerson × Principal | System of record already assumes Principal dimension. |

Salesman planning remains important for **coaching and execution**, not as the primary **commercial growth commitment** surface.

### 3.2 Workflow alignment with Owner thinking

Proposed flow:

```text
Company Revenue Goal
  → Principal targets / growth
    → Optional salesman allocation
      → Execution (Faktur / field)
        → Evaluation
```

This matches observed Owner discussions more closely than starting from individual salesman cards.

Caveat: operations staff who maintain SM6 still think in **Salesman → Principal grid**. The workspace must eventually bridge Owner Principal decisions into SM6 rows (allocation), or staff will re-split numbers manually.

### 3.3 Value vs salesman planning

| Dimension | Principal-first | Salesman-first |
| --------- | --------------- | -------------- |
| Owner decision quality | **Higher** | Medium |
| Alignment with shared customers | **Higher** | Weak |
| Alignment with SM6 storage grain | **Higher** (decide at Principal, split to reps) | Medium (decide at rep, split to Principals) |
| Near-term analytics readiness | **Lower** (sales-out history/forecast thinner) | Higher (SF01 + RepHistory mature) |
| Execution coaching | Secondary | **Primary** |
| Scalability of entity list | Harder (~200–250) | Easier (~50–80) |

**Business conclusion:** Principal-first provides **more strategic value**. Salesman-first provided **faster analytics reuse**. Choosing Principal-first correctly prioritizes business fit over implementation convenience.

---

## 4. Analytics Baseline Review

Classification key: **Available** | **Available with Minor Work** | **Requires New Analytics** | **Not Feasible**

### 4.1 Principal-level metrics

| Metric | Classification | Evidence / notes |
| ------ | -------------- | ---------------- |
| Monthly sales (company omzet by Principal) | **Available with Minor Work** | Proven join `FakturItem → Brg.SupplierId`. Portal already computes `SalesOutAmount` in purchasing management portfolio; Desktop `OmzetSupplierViewDal`; can sum `FakturPrincipalOmzet`. Needs promotion to first-class sales KPI + history store. |
| Growth trend (sales-out MoM / 3-mo) | **Requires New Analytics** | Supplier Entity Analytics “Growth” today is **purchase MoM** (`PU-KPI-001`) — wrong semantic for sales growth planning. Sales-out trend needs new history. |
| Forecast (Principal month-end pace) | **Requires New Analytics** | SA02 / `SalesForecastPolicy` is company-only. Same pace formula can apply per Principal once MTD sales-out exists. |
| Customer Count (ever / distinct buyers) | **Available with Minor Work** | Distinct customers from `SupplierMtdItemRollup` / relationship aggregator patterns; Top customers exist in L4, not full counts. |
| Active Customer Count (invoiced in period for that Principal) | **Available with Minor Work** | Same rollup; define as customers with Principal-attributed omzet in period. |
| Dormant Customer Count (Principal-scoped) | **Requires New Analytics** | Dormant today is customer/salesman portfolio concept (CU04 / SF01). Needs Principal-scoped inactivity rule (e.g. bought Principal before, not in N days). |
| Collection Quality / piutang by Principal | **Not Feasible** (current model) | Piutang is Faktur-header scoped; multi-Principal invoices have **no allocation rule**. Explicitly excluded from M14 Principal concentration. Do not fake in MVP. |
| Portfolio Health | **Split** | **SKU/catalog health: Available** (`ActiveSkuCount`, `CatalogPenetration`, inventory / at-risk via Supplier EA + PU01). **Customer portfolio health score: Requires New Analytics** (CU04 is customer-scoped). |

### 4.2 Supporting Principal analytics (context, not sales target basis)

| Metric | Classification | Role in planning |
| ------ | -------------- | ---------------- |
| MTD purchase / dependency / posting | **Available** (PU01) | Supply constraint / dependency risk — not sales target baseline |
| Inventory value / at-risk by Supplier | **Available** | Capacity / overstock caution |
| Salesman × Principal achievement (SF-KPI-011) | **Available** | Useful for **allocation** phase, not company Principal goal |
| Assigned salesman list per Principal | **Available with Minor Work** | From `BTR_SalesPersonSupplier` |
| Company Principal target (sum of SM6 rows) | **Available with Minor Work** | `SUM(TargetAmount)` by `SupplierId` for period |

### 4.3 Critical semantic warning

**Do not use purchase growth as a proxy for sales growth in this workspace.**

PU01 and Supplier Entity Analytics optimize buying. Principal Growth Planning must optimize **selling (sales-out)**. Mixing them will destroy trust.

---

## 5. Recommendation Engine Feasibility

### 5.1 Can Safe / Normal / Stretch be generated per Principal?

**Yes**, once Principal sales-out history exists — same complexity class as the salesman model in the prior study.

#### Suggested MVP formulas

```text
Baseline     = Average(PrincipalSalesOut for last 3 completed months)
TrendFactor  = LastMonth / PriorMonth   (clamped, e.g. 0.85–1.15)
Safe         = Baseline × 0.95
Normal       = Baseline × TrendFactor   (or × 1.00 if trend weak / thin history)
Stretch      = Baseline × max(TrendFactor, 1.05) × StretchMultiplier (e.g. 1.08–1.12)
```

Company-goal-aware variant (recommended when goal exists):

```text
NaturalSum   = SUM(Normal across Principals)
ScaleFactor  = CompanyGoal / NaturalSum   (optional soft scaling)
Proposed     = Normal × ScaleFactor       (Owner still confirms)
```

### 5.2 Assessment

| Dimension | Assessment |
| --------- | ---------- |
| Data requirements | Need Principal × Month sales-out history (new snapshot/history table or reuse/promote OmzetSupplier monthly rollups). MTD current month available via existing rollup. |
| Formula complexity | **Low–Medium** — deterministic, explainable |
| Reliability | **Medium** — good for ranking Principals and framing debate; weak for brand-new Principals / sparse history |
| Implementation effort | **Medium** — higher than salesman MVP because history store is not as mature as `BTRPD_SalesmanRepHistory` |
| Risk of wrong Stretch | Same mitigation: clamps, Custom always available, transparent reasons |

### 5.3 Difficulty / risk classification

Still feasible as advisory:

```text
Expected   = pace forecast OR Normal baseline
Coverage   = SelectedTarget / Expected
Low / Medium / High bands (e.g. ≤1.05 / ≤1.20 / >1.20)
```

**Do not** use collection quality in MVP risk (not feasible). Optional Phase 2 dampeners: declining active buyers, weak catalog penetration, inventory stock-out risk.

---

## 6. Growth Opportunity Analysis Feasibility

Concept: explain `Growth Needed = Target − Forecast` via opportunity sources.

| Opportunity source | Existing support | Required new analytics | Confidence | MVP? |
| ------------------ | ---------------- | ---------------------- | ---------- | ---- |
| **Dormant Customer Recovery** | Customer dormant exists; not Principal-scoped | Principal-scoped dormant set + prior Principal omzet estimate | Medium after new analytics | **Defer** |
| **Existing Customer Expansion** | Top customers by Principal omzet (L4) | Peer / historical max vs current per customer×Principal | Medium | **Defer** (light hint possible later) |
| **New Customer Activation** | Weak | Customers active on other Principals but not this one; or never-bought | Low–Medium | **Defer** |
| **Cross-Sell Opportunity** | Catalog penetration (SKU) exists | Customer×Principal purchase matrix (“buys A not B”) | Medium–High value, **High** build cost | **Defer** |

### 6.1 Verdict on opportunity analysis

**Conceptual value is high; MVP feasibility is low.**

Estimating rupiah opportunity without double-counting (same customer in dormant + expansion + cross-sell) needs careful policy design. Treat as **Phase 2+ Decision Support**, not as a dependency of Principal target setting.

MVP may show a simple:

```text
Growth Needed = Selected Target − Current Forecast (or Baseline)
```

without decomposing sources.

---

## 7. UX Reassessment

### 7.1 Is Principal Planning Workspace superior?

| Criterion | Principal workspace | Salesman workspace |
| --------- | ------------------- | ------------------ |
| Management alignment | **Superior** | Weaker for Owner commercial planning |
| Decision quality on growth commitments | **Superior** | Risks mis-attributing shared customers |
| Usability for Owner monthly ritual | **Superior** if table-first | Familiar to supervisors, less to Owner |
| Scalability | **Harder** (~200–250 rows) | Easier |
| Path to SM6 persistence | Needs allocation step | More direct for supervisors |

**Overall:** Principal workspace is the **correct primary product**. Salesman allocation should be a **secondary drawer/step**, not the home screen.

### 7.2 Recommended screen structure

```text
┌──────────────────────────────────────────────────────────────────────────┐
│ Principal Growth Planning — Period: [Oct 2026 ▼]                          │
├──────────────────────────────────────────────────────────────────────────┤
│ Company Goal [ 2.50 B ]   Forecast 2.20 B   Assigned 2.41 B   Gap 90 M  │
│ Expected Growth +13.6%     Completeness 18/22 active Principals          │
├──────────────────────────────────────────────────────────────────────────┤
│ Filters: Search | Active only | Gap contributors | Missing target        │
│ Bulk: Apply Normal to missing | Scale to Goal (preview)                  │
├──────────────────────────────────────────────────────────────────────────┤
│ Principal │ Last Mo │ 3-mo Avg │ Forecast │ Safe │ Normal │ Stretch │    │
│           │         │          │          │      │ Select │ Risk    │ …  │
│ Wings     │ 820 M   │ 790 M    │ 820 M    │ 750  │ 900*   │ 960     │ Med│
│ Unilever  │ ...                                                         │
├──────────────────────────────────────────────────────────────────────────┤
│ Detail drawer (row expand):                                              │
│  - History sparkline / best month                                        │
│  - Active customers (MTD) · Catalog penetration · Inventory caution      │
│  - Reasons for recommendation                                            │
│  - Optional: Assigned salesmen + allocation preview                      │
└──────────────────────────────────────────────────────────────────────────┘
```

### 7.3 UX rules

1. **Table-first mandatory** — do not ship card-grid as default.
2. Focus on **commercially active Principals** (recent sales-out or assigned targets), not all 250 suppliers by default.
3. Keep purchase metrics as **secondary caution**, visually separated from sales-out baselines.
4. Never label purchase MoM as “Growth” on this screen.
5. Salesman allocation = optional Phase 1.5 / Phase 2 panel, not step blocking finalize in MVP advisor mode.

---

## 8. Architecture Impact Reassessment

### 8.1 What changes vs the original feasibility study

| Area | Original (Salesman-first) | Principal-first reassessment |
| ---- | ------------------------- | ---------------------------- |
| Primary history store | Reuse `BTRPD_SalesmanRepHistory` | **New/promoted Principal sales-out history** required |
| Forecast reuse | Apply SA02 policy per salesman | Apply SA02 policy per Principal |
| Card metrics | SF01 portfolio + piutang ready | Piutang **removed**; customer counts **rebuild**; catalog/inventory **borrow from Supplier EA** |
| Persistence mapping | Rep total → allocate to Principals | **Principal total → allocate to salesmen** (inverse) |
| Companion dashboards | SA01/SA02/SF01 | SA01/SA02 + **PU01/Supplier EA (context only)** + SM6 |
| Entity scale | ~50–80 | ~200–250 → stronger need for filters/active subset |
| Opportunity analysis | Not central | Explicitly evaluated — **defer** |
| Portal write constraint | Same | Same |
| Lock/versioning | Deferred | Still deferred |

### 8.2 Impact classes

| Change | Impact | Class |
| ------ | ------ | ----- |
| Principal × Month sales-out history snapshot | New analytics store + worker step | **Medium–High** |
| Planning read API (company + Principal rows + scenarios) | New query surface | **Medium** |
| Recommendation policy (Principal) | New policy service | **Medium** |
| Company plan header (goal/status) | New table (same as before) | **Medium** |
| UI Workspace | New Portal view | **Medium** |
| Salesman allocation writer into SM6 rows | New allocation policy | **Medium** (Phase 2 if MVP is advisor-only) |
| Portal business write exception | Architecture policy change | **High** (optional) |
| Principal piutang allocation model | Domain invention | **High** — out of scope |
| Growth opportunity engines | Multiple new analytics | **High** — Phase 2+ |
| Reporting / planned vs actual by Principal | Extend achievement views | **Medium** (after targets exist) |

### 8.3 Database / API / projection (summary)

| Layer | MVP need |
| ----- | -------- |
| DB | Principal sales-out monthly history (or extend existing snapshot); optional `SalesTargetPlan` header |
| API | `GET` Principal growth planning context; writes only if exception approved |
| Worker/projection | Prefer extend purchasing/sales snapshot refresh to persist Principal sales-out history + optional forecast columns |
| Reporting | Later: Principal planned vs actual; reuse OmzetSupplier / FakturPrincipal patterns |

### 8.4 Target persistence model (unchanged system of record)

```text
Owner decides: Principal Target (company)
     ↓ allocate across assigned salesmen
BTR_SalesPersonPrincipalTarget (SalesPerson × Principal × Month)
     ↓ sum
Company / Principal assigned totals
```

No need for a separate company-Principal target table **if** allocation always writes SM6 rows. A plan header for Company Goal + finalize status is still useful.

---

## 9. MVP Recommendation

### 9.1 MVP — Principal Growth Planning Advisor

Smallest valuable product:

1. Period = next month (selectable).
2. Company Goal input + Forecast (SA02) + Assigned Principal targets + Gap.
3. Principal planning table for **active Principals**:
   - Last month sales-out, 3-month average, best month (once history exists; bootstrap from available months).
   - Current-month pace forecast (new, formula reuse).
   - Safe / Normal / Stretch + Custom.
   - Advisory difficulty.
   - 1–3 explanation bullets from sales-out history only.
4. Supporting context (optional columns / drawer): Active customer count (MTD), catalog penetration, inventory caution.
5. Persistence: advisor + SM6 handoff **or** approved narrow write that allocates Principal decision to assigned salesmen.
6. Explicit exclusions: opportunity decomposition, collection quality, hard lock, visit metrics, full salesman negotiation UI.

### 9.2 MVP success criteria

- Owner can set company goal and see Principal assignment gap.
- Top Principals by sales-out have transparent Normal recommendations.
- Recommendations never silently use purchase growth as sales growth.
- At least one monthly planning cycle completed without spreadsheet for Principal targets.
- Path exists to land numbers in SM6 (manual or allocated write).

### 9.3 Phase 2

- Soft finalize + plan snapshot.
- Salesman allocation panel (split Principal target by prior mix / capacity).
- Principal-scoped dormant / expansion opportunity hints (conservative, non-additive).
- Stronger forecast confidence and difficulty model.
- Alert: incomplete Principal plan / unresolved company gap.
- Planned vs actual Principal evaluation view.

### 9.4 Future vision

- Hard lock + reopen governance.
- Full Growth Opportunity Analysis with anti-double-count rules.
- Cross-sell matrices and program/commitment tracking.
- Cross-domain constraints (inventory supply vs Principal sales stretch).
- Optional Principal collection quality **only after** an explicit Faktur allocation policy exists.
- Unified Planning & Execution platform with Principal as the commercial spine and Salesman as the execution spine.

---

## 10. Roadmap Recommendation

```text
Phase 1 — Principal Growth Advisor
  Company goal · sales-out baselines · Safe/Normal/Stretch · gap
        ↓
Phase 2 — Commit & Allocate
  Soft finalize · SM6/Portal allocation to salesmen · evaluate plan vs actual
        ↓
Phase 3 — Opportunity & Governance
  Opportunity decomposition · lock/reopen · cross-domain constraints
```

| Phase | Primary outcome |
| ----- | --------------- |
| 1 | Owner plans growth in Principal language |
| 2 | Decisions become executable targets in SM6 |
| 3 | Planning becomes governed Decision Support |

**Do not** restart with salesman-first MVP if Principal-first is accepted — that would rebuild the wrong habit. Keep SF01 as execution coaching, not as the planning home.

---

## 11. Risks & Constraints

| # | Risk / constraint | Severity | Mitigation |
| - | ----------------- | -------- | ---------- |
| R1 | Confusing **purchase** metrics with **sales-out** growth | **High** | Separate labels; ban purchase MoM from recommendation formula |
| R2 | Principal sales history less mature than salesman RepHistory | **High** | MVP includes history projection work; bootstrap carefully |
| R3 | Collection quality requested but **not feasible** | **High** (scope risk) | Explicitly exclude; do not approximate from inventory |
| R4 | Opportunity analysis over-promised | **Medium** | Defer; show Growth Needed only |
| R5 | ~200–250 Principals overwhelm UX | **Medium** | Active-Principal filter; table-first; bulk apply |
| R6 | Allocation to salesmen still required for SM6 | **Medium** | Phase 2 panel; MVP advisor handoff acceptable |
| R7 | Portal read-only vs Owner wanting in-Portal save | **High** (same as prior) | Decide write exception early |
| R8 | Historical attribution uses **current** `Brg.SupplierId` | **Medium** | Document limitation; acceptable for planning |
| R9 | Dual conversations (Owner Principals vs Supervisor salesmen) | **Medium** | Clear dual surfaces: Planning vs SF01 coaching |
| R10 | Scaling recommendations to company goal blindly | **Medium** | Preview Scale-to-Goal; never auto-commit |

---

## 12. Final Verdict

### Verdict: **GO WITH REDUCTIONS**

### Reasoning

1. **Business aggregate is corrected.** Principal-first matches Owner behavior, shared customer ownership, and SM6’s Principal dimension better than salesman-first.
2. **Core planning mechanics from the prior study still apply** — company goal balancing, scenarios, explainability, hybrid UX, deferred lock, deterministic recommendations.
3. **Analytics are sufficient for a reduced MVP**, but only after promoting Principal **sales-out** history/forecast. They are **not** as ready as the salesman path was.
4. **Several proposed Principal card fields must be cut** — especially collection quality and Growth Opportunity decomposition — or the project becomes CONDITIONAL on unresolved domain models.
5. **Salesman planning is demoted, not deleted** — it remains the execution/allocation layer after Principal decisions.

### Reductions required for GO

- No Principal collection/piutang quality in MVP.
- No Growth Opportunity source split in MVP.
- No pure card UX; table-first only.
- No hard lock in MVP.
- Recommendations based on **sales-out history only** (not purchase).
- Salesman allocation secondary (Phase 2 unless write path is in MVP).

### When verdict would change

| Trigger | New verdict |
| ------- | ----------- |
| Product requires Principal piutang + opportunity rupiah breakdown in v1 | **CONDITIONAL GO** — needs allocation policy + multi-engine analytics first |
| Product refuses to fund Principal sales-out history and tries to reuse purchase KPIs | **NO GO** — would produce misleading “growth” planning |
| Product wants Principal planning but also salesman-first as equal primary in same MVP | **CONDITIONAL GO** — split releases; do not dual-primary one screen |
| Write path + allocation + lock all mandatory in v1 | **CONDITIONAL GO** — architecture exception + higher delivery risk |

---

## Appendix A — Previous vs revised planning hierarchy

```text
Previous assumption                 Revised assumption
─────────────────────               ──────────────────
Company Goal                        Company Goal
    ↓                                   ↓
Salesman Target Planning            Principal Growth Planning
    ↓                                   ↓
(allocate to Principals)            (optional) Salesman Allocation
    ↓                                   ↓
Execution                           Execution
```

## Appendix B — Metric classification quick reference

| Metric | Classification |
| ------ | -------------- |
| Principal monthly sales-out | Available with Minor Work |
| Principal sales growth trend | Requires New Analytics |
| Principal sales forecast | Requires New Analytics |
| Customer / Active count (Principal) | Available with Minor Work |
| Dormant count (Principal) | Requires New Analytics |
| Collection quality (Principal) | Not Feasible |
| SKU / catalog portfolio health | Available |
| Customer portfolio health score | Requires New Analytics |
| Safe / Normal / Stretch engine | Requires New Analytics (feasible) |
| Growth opportunity decomposition | Requires New Analytics — defer |
| Company goal + gap | Requires New Analytics (trivial once goal exists) |
| Portal target write | Not Feasible under current architecture |
| SM6 Principal×Salesman write | Available |

## Appendix C — Suggested next artifacts

If Product Owner accepts **GO WITH REDUCTIONS**:

1. Analyst: `docs/features/btr-portal/principal-growth-planning/feature.md` (MVP scope only)
2. Architect: `docs/work/btr-portal/principal-growth-planning/implementation-plan.md`
3. Decision record: **sales-out history projection design** + **Portal write vs SM6 allocation**
4. Mark salesman-first MVP path in the prior study as **superseded for primary aggregate**, retained as execution-layer reference
