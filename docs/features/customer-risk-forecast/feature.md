# Customer Risk Forecast Dashboard

**Feature:** M29 — Customer Risk Forecast Dashboard  
**Status:** Current  
**Route:** `/dashboard/customer-risk-forecast`  
**API:** `GET /api/dashboard/customer-risk-forecast`

---

## Purpose

Management needs to anticipate **customer-level collection and relationship risk** within a **30-day horizon** before problems become overdue or dormant. The Customer Risk Forecast Dashboard answers:

> *Which customers are likely to become overdue, exceed credit limits, stop purchasing, or require early collection planning within the next 30 days — and why?*

This is a **read-only**, **deterministic**, **explainable** forecast layer — not AI/ML.

---

## Relationship to Other Dashboards

| Dashboard | Question | M29 relationship |
| --------- | -------- | ---------------- |
| Piutang (M14) | Who owes money today? | Reuses aging via `PiutangAgingBucketResolver`; does not duplicate |
| Customer Analytics (M17) | Who needs attention today? | Extends with forward signals (approaching dormant, projected plafond) |
| Collection (M20) | Recovery performance today? | Collection risk thresholds reused at customer grain |
| Cash Flow Forecast (M27) | Company month-end cash? | Complementary — M29 is customer portfolio risk |

**Traceability:** `TotalPiutang` (CRF-KPI-07) must equal the sum of open balances from the same piutang load in the Customer snapshot refresh (CRF-KPI-50).

---

## Data Scope (V1)

| Rule | Description |
| ---- | ----------- |
| CRF-01 | 30 calendar day forecast horizon from business date |
| CRF-02 | Business date from `IBusinessDateProvider.Today` |
| CRF-03 | Open balance rows: `KurangBayar > 1` |
| CRF-04 | Customer key: `DashboardCustomerKeyResolver` code-first |
| CRF-05 | No AI/ML — integer scores and threshold rules only |
| CRF-06 | Read-only — no Desktop write-back |
| CRF-07 | UangMuka excluded from pelunasan aggregates |
| CRF-08 | Prior-month omzet floor (default Rp 1M) for decline rules |
| CRF-09 | Min 2 settled Fakturs for average payment lag |
| CRF-10 | Max materialized rows: 20 customers, 25 attention, 15 recommendations |

**Out of scope (V1):** DSO headline KPI, per-salesman/wilayah forecast pages, Alert Center, Executive integration, historical snapshot retention.

---

## Risk Categories

Five-band model derived from signal severity counts:

| Category | Entry conditions |
| -------- | ---------------- |
| Healthy | No forecast signals; or zero balance with current purchasing |
| Watch | 1 moderate or 2 weak signals; no strong |
| Attention | 1 strong or ≥2 moderate; not High Risk |
| High Risk | ≥2 strong, or 1 strong + ≥2 moderate, or chronic + forward signal |
| Critical | ≥3 strong, or chronic + plafond + decline/inactivity combo |

---

## Forecast Signal Families

1. **Payment delay** — likely late payer, escalating overdue, no recent payment, due-soon slow payer
2. **Credit limit** — projected plafond breach, approaching 90%, breached worsening
3. **Inactivity** — approaching dormant (60–79d), imminent dormant (80–89d), legacy forward
4. **Purchase decline** — moderate (<70%), severe (<50%), stopped after history
5. **Collection risk** — due concentration, chronic trajectory, legacy + overdue forward

---

## Projected Plafond (V1)

Conservative upper-bound formula:

```
ProjectedBilling = (MTD Omzet ÷ DaysElapsed) × HorizonDays
ProjectedOpenBalance = CurrentOpenBalance + ProjectedBilling
```

UI displays this as indicative — not an automatic credit decision.

---

## Snapshot Refresh

- **Worker domain:** Customer (~30 minutes)
- **Co-located with:** M17 Customer Analytics snapshot
- **Tables:** `BTRPD_CustomerRiskForecast*` (7 tables, `SnapshotKey = CURRENT`)
- **Manual refresh:** `POST /api/admin/dashboard/refresh?domain=Customer`

---

## Recommendation Types

Rule-triggered, one primary recommendation per customer: Management Review, Suspend Credit Review, Early Collection, Review Credit, Legacy Debt Review, Call Customer, Schedule Visit, Sales Recovery, Increase Monitoring, Continue Monitoring.

---

## Document Maintenance

Authoritative business rules: [portal-analysis-m29-customer-risk-forecast.md](../../work/btr-portal/portal-analysis-m29-customer-risk-forecast.md)
