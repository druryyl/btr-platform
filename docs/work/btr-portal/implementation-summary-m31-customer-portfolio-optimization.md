# M31 Customer Portfolio Optimization Dashboard — Implementation Summary

| Field | Value |
| --- | --- |
| Milestone | M31 — Customer Portfolio Optimization Dashboard |
| Plan | [implementation-plan-m31-customer-portfolio-optimization.md](./implementation-plan-m31-customer-portfolio-optimization.md) |
| Analysis | [portal-analysis-m31-customer-portfolio-optimization.md](./portal-analysis-m31-customer-portfolio-optimization.md) |
| Status | **Complete** |
| Date | 2026-06-22 |

## Delivered

### Backend — portfolio composition aggregation

- `CustomerPortfolioOptimizationPolicy` — lifecycle/tier/action constants, attention filter, portfolio priority score, action owners.
- `CustomerPortfolioLifecycleResolver` — computed lifecycle (Never Purchased → Dormant → New → Declining → Growing → Mature).
- `CustomerPortfolioTierResolver` — Strategic / High / Medium / Low Value from omzet rank, piutang rank, frequency, risk (never Klasifikasi).
- `CustomerPortfolioActionBuilder` — single primary action with Collect→M30 link, reason text, rule ids.
- `CustomerPortfolioExecutiveSummaryBuilder` — plain-language portfolio brief.
- `DashboardCustomerPortfolioAggregator` — composes M17 + M29 + M30 in-memory; M18 salesman cross-read; optional wilayah breakdown.
- Extended `DashboardCollectionOptimizationAggregateResult.ContextsByKey` for M31 Collect resolution.
- Extended `RefreshDashboardCustomerSnapshotWorker` — first faktur, purchase frequency, last faktur+salesman, M18 snapshot loads; M31 step after M30.
- Extended `DashboardCustomerSnapshotDal.ReplaceCurrent` — persists M17 + M29 + M30 + M31 atomically.
- New DALs: `ICustomerFirstFakturDal`, `ICustomerPurchaseFrequencyDal`.

### Database

- `BTRPD_CustomerPortfolioKpi` — headline KPIs + executive summary + value disclaimer.
- `BTRPD_CustomerPortfolioLifecycleDist`, `TierDist`, `ActionDist` — distributions.
- `BTRPD_CustomerPortfolioPriority` — top 50 attention queue.
- `BTRPD_CustomerPortfolioCustomer` — all customers (report + All Customers view).
- `BTRPD_CustomerPortfolioConcentration` — top 10 omzet/piutang (copied from M17).
- `BTRPD_CustomerPortfolioWilayah` — top 15 wilayah breakdown.
- `Upgrade_M31_CustomerPortfolioOptimization.sql` + entries in `Create_BTRPD_PortalDashboard_Tables.sql`.

### API

- `GET /api/dashboard/customer-portfolio` — portfolio dashboard read path.
- `GET /api/reports/customers?customerCode=` — Customer Report from M31 snapshot.
- Extended `GET /api/dashboard/executive` — `Portfolio` summary cards (healthy %, at risk, strategic at risk).
- Extended `InvestigationRegistry` — portfolio action signals + Customer Report route.

### Portal Web

- Route `/dashboard/customer-portfolio` — sidebar **Customer Portfolio** (after Collection Optimization).
- Route `/reports/customers` — Customer Report with investigation pre-filter.
- Components: summary, KPI grid, lifecycle/tier charts, filter bar, priority table, action segments, concentration tables.
- `ExecutivePortfolioSummarySection` on Management Attention Center (M16).
- Service: `customerPortfolioSignals.ts` — lifecycle/tier/action labels, Collect→M30 link builder.

### Configuration

- `DashboardSnapshot:CustomerPortfolio*` thresholds in `DashboardSnapshotOptions`.

## Tests

| File | Coverage |
| ---- | -------- |
| `CustomerPortfolioLifecycleResolverTest.cs` | Lifecycle precedence, M17 dormant, M29 decline |
| `CustomerPortfolioTierResolverTest.cs` | Strategic rank, frequency tier, low default |
| `CustomerPortfolioOptimizationPolicyTest.cs` | Attention matrix, priority score, Collect precedence |
| `CustomerPortfolioActionBuilderTest.cs` | Reason text, M30 link, rule ids |
| `DashboardCustomerPortfolioAggregatorTest.cs` | KPI/distributions, attention count reconciliation (CPO-52) |
| `customerPortfolioSignals.spec.ts` | Label maps, M30 link, attention filter |

## Knowledge sync

- Updated [feature.md](../features/customer-portfolio-optimization/feature.md) — status **Current**
- Updated [btr-portal-domain.md](../features/btr-portal/btr-portal-domain.md) — M31 milestone
- Updated [btr-portal-operational.md](../features/btr-portal/btr-portal-operational.md) — routes, usage, Customer Report

## Out of scope (unchanged)

- M17/M29/M30 API response shapes and rule engines
- M30 collection queue duplication on M31 surface
- Alert Center integration
- Field activity (M18.5)
- Profitability / retur ratio / Desktop write-back
- Historical portfolio snapshot retention

## Acceptance criteria

| Criterion | Status |
| --------- | ------ |
| One primary action per attention row | Done |
| Default Attention view; All Customers toggle | Done |
| M29/M17 signals composed from same refresh | Done |
| Collect links to M30 only | Done |
| Omzet value labeled as proxy | Done |
| Lifecycle + tier visible; Klasifikasi filter only | Done |
| Salesman summary on rows (M18 cross-read) | Done |
| M16 summary cards only | Done |
| Investigation chain to Customer Report | Done |
| Read-only | Done |
