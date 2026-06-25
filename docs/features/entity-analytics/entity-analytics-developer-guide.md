# Entity Analytics — Developer Guide



**Status:** Permanent knowledge (M32.11 complete)  

**Audience:** Implementers adding entity types or history layers  

**Architecture:** [entity-analytics-architecture.md](../../work/btr-portal/entity-analytics/entity-analytics-architecture.md)  

**Roadmap SSOT:** [entity-analytics-roadmap-authoritative.md](../../work/btr-portal/entity-analytics/entity-analytics-roadmap-authoritative.md)



---



## Overview



Entity Analytics is a **platform layer** that materializes per-entity KPI snapshots (L0–L5) and composes a unified **Performance Profile** API.



**Delivery strategy:** Capability layers are proven on **Customer** first (M32.1–M32.8). Additional entity types (Salesman, Supplier, Item) adopt the pipeline via registration-only entity packs (M32.9+).



**Current status (M32.11):** Customer, **Salesman**, **Supplier**, and **Item** are enabled entity types. L0–L5 platform layers, Comparison Engine, Search API, and Radar Engine are implemented on Customer; Salesman (M32.9), Supplier (M32.10), and Item (M32.11) adopt the full pipeline via entity packs.



```text

Domain Worker Refresh

  → Domain Aggregator (unchanged business logic)

  → IEntityAnalyticsProducer

      → ReplaceCurrentMetrics (L0)                    ← M32.2

      → EnsurePriorMonthClosed + SaveMonthlyHistory (L1) ← M32.3

      → EntityRankingEngine.ComputeAndPersistRanks (L2) ← M32.4

      → EntityAttentionEngine.DiffAndPersistSignals (L3) ← M32.5

      → EntityRelationshipEngine.PersistRollups (L4) ← M32.6

      → [M32.8] EntityRadarEngine → L5

  → BTRPD_EntityAnalytics_Current + _Monthly + _Ranking + _Attention + _Relationship (+ L5)



HTTP GET /api/entity-analytics/{entityType}/{entityId}

  → EntityAnalyticsService (enabled-type gate)

  → EntityPerformanceProfileComposer

      → L0 reads for Overview / KPI Summary

      → EntityTrendEngine for Trend section (L1 reads)

      → EntityRankingEngine for Ranking section (L2 reads)

      → EntityAttentionEngine for Attention section (L3 reads)

      → EntityRelationshipEngine for Related Entities section (L4 reads)

      → [M32.7] EntityComparisonEngine for Comparison section

  → EntityPerformanceProfileResponse

```



---



## Capability-Layer vs Entity-Pack Work



| Work type | When | What you add |

|-----------|------|--------------|

| **Capability layer** (M32.5–M32.8) | Platform milestone | SQL table, engine service, repository methods, composer section, Vue component — **once** for all entity types |

| **Entity pack** (M32.9+) | Per entity type | Registrar + producer + evidence resolver + worker hook — **no platform service edits** |



When implementing M32.5+, extend the Customer producer first. Entity packs inherit engines automatically via KPI pack metadata and producer hooks.



---



## Extension Points (Open/Closed)



| Extension | Interface | Register via |

|-----------|-----------|--------------|

| Entity type metadata | `IEntityAnalyticsRegistrar` | DI singleton + `EntityAnalyticsRegistryBootstrap` |

| KPI metadata & packs | `IKpiRegistry` | Entity registrar `RegisterMetadata` / `RegisterPack` |

| Dimension labels | `IDimensionLabelRegistry` | Entity registrar `Register(entityType, kpiId, label)` |

| L0 production | `IEntityAnalyticsProducer` | Scrutor scan, hook in domain worker |

| Evidence links | `IEntityProfileEvidenceResolver` | Scrutor scan |

| Profile composition | `IEntityProfileBuilder` | Default: `EntityPerformanceProfileComposer` |



**Do not modify** platform services when adding an entity type. Add a new registrar + producer + evidence resolver.



---



## Producer Lifecycle



1. Domain worker completes aggregator and saves domain snapshots.

2. Worker calls `EntityAnalyticsProducerOrchestrator.ProduceForDomain(workerDomain, context)` **inside the same `TransHelper` scope** as domain save.

3. Orchestrator resolves all `IEntityAnalyticsProducer` instances where `WorkerDomain` matches.

4. Producer reads **in-memory** `context.DomainInput` (typed per entity). **No DAL calls.**

5. Producer builds `EntityAnalyticsCurrentRow` list:

   - Catalog KPI IDs for metrics (`CU-KPI-009`, …)

   - `EA-META-*` rows for identity (`DisplayName`, `IsActive`)

   - `EA-DIM-*` rows for overview dimensions

6. Producer calls `ReplaceCurrentMetrics(entityType, rows, refreshLogId)` — **always**, even when empty (clears stale L0).

7. Producer calls `EnsurePriorMonthClosed` then `SaveMonthlyHistory` for **TrendEligible** KPIs in the current calendar month (M32.3).

8. Producer calls `IEntityRankingEngine.ComputeAndPersistRanks` after L1 save (M32.4) — engine reads L1 population; no duplicate KPI calculation.

9. **[M32.5]** Producer calls `IEntityAttentionEngine.DiffAndPersistSignals` — maps domain attention signals to L3 event log.

10. **[M32.6]** Producer calls `IEntityRelationshipEngine.ComputeRollups` — materializes L4 Top-N rows from relationship pack.

11. **[M32.8]** Producer calls `IEntityRadarEngine.ComputeScores` — peer-normalized L5 axis scores.



### Customer example



- **Producer:** `CustomerEntityAnalyticsProducer`

- **Input:** `CustomerEntityAnalyticsProduceInput` (portfolio + attention aggregate)

- **Entity key:** `CustomerCode` (business identifier = `EntityId` = `EntityCode`)

- **Worker hook:** `RefreshDashboardCustomerSnapshotWorker` after domain save

- **L1 KPIs:** CU-KPI-009, CU-KPI-010, FI-KPI-013 (same portfolio row values as L0)

- **L2 KPIs:** CU-KPI-009 (`RankEligible`), CU-KPI-010 (`RankEligible`)



---



## KPI Registry Usage



KPI definitions live in code (`CustomerEntityAnalyticsRegistrar.RegisterKpiMetadata`). Each `EntityKpiMetadata` includes:



| Field | Purpose |

|-------|---------|

| `KpiId` | Immutable catalog ID |

| `Category` | Profile grouping (Financial, Risk, …) |

| `PeriodSemantics` / `TimeGrain` | MTD vs point-in-time; history grain |

| `ValueType` / `AggregationType` | Numeric/Text; Sum/LastValue |

| `Direction` | HigherIsBetter / LowerIsBetter / Neutral — also used as **ranking direction** for L2 |

| `DisplayPrecision` / `NullableBehavior` | Formatting |

| `TrendEligible` / `RankEligible` | L1 trend / L2 ranking participation |

| `EvidenceRoute` / `EvidenceFilterDimension` | Drill-down links |

| `SourceDomain` | Originating aggregator domain |

| `ApplicableEntityTypes` | Entity scope |

| `DefinitionVersion` | Semantic version stored on L0 rows |



Validate packs at startup or in tests:



```csharp

registry.ValidatePack("customer-default"); // returns missing KPI IDs

```



---



## Snapshot Lifecycle



### L0 CURRENT — implemented M32.2



| Attribute | Location |

|-----------|----------|

| Table | `BTRPD_EntityAnalytics_Current` |

| Snapshot key | `EntityAnalyticsConstants.CurrentSnapshotKey` = `CURRENT` |

| Snapshot version | `EntityAnalyticsConstants.CurrentSnapshotVersion` = `L0-v1` |

| Grain | `(EntityType, EntityId, KpiId)` |

| Refresh correlation | `LastRefreshLogId` |



### L1 MONTHLY — implemented M32.3



| Attribute | Location |

|-----------|----------|

| Table | `BTRPD_EntityAnalytics_Monthly` |

| Grain | `(EntityType, EntityId, PeriodYear, PeriodMonth, KpiId)` |

| Open vs closed | `IsClosed` — upsert allowed only when `IsClosed = 0` |

| Month-close meta | `BTRPD_EntityAnalytics_MonthClose` |

| Retention | `PurgeHistoryOlderThan` uses `HistoryRetentionMonths` (default 36) |



### L2 RANKING — implemented M32.4



| Attribute | Location |

|-----------|----------|

| Table | `BTRPD_EntityAnalytics_Ranking` |

| Grain | `(EntityType, EntityId, PeriodYear, PeriodMonth, KpiId)` |

| Rank position | `RankPosition` — **1 = best** (competition ranking) |

| Population | `PopulationSize` — active entities with non-null L1 value |

| Eligibility | `RankEligible = true` and `Direction != Neutral` |



### L3 ATTENTION — planned M32.5



| Attribute | Location |

|-----------|----------|

| Table | `BTRPD_EntityAnalytics_Attention` (not yet created) |

| Grain | `(EntityType, EntityId, SignalCode)` |

| Fields | `FirstSeenPeriod`, `LastSeenPeriod`, `IsActive` |

| Source | Domain attention aggregators — no new signal rules in platform |

| Repository | `IEntityAnalyticsAttentionRepository` — read stub today |



### L4 RELATIONSHIP — implemented M32.6



| Attribute | Location |

|-----------|----------|

| Table | `BTRPD_EntityAnalytics_Relationship` |

| Grain | `(SourceEntityType, SourceEntityId, RelationshipCode, PeriodYear, PeriodMonth, Rank)` |

| Pattern | Top-N rollups per relationship pack; atomic replace per period |

| Repository | `IEntityAnalyticsRelationshipRepository` — `ReplaceRelationshipRollups` + `GetRelationshipRollups` |

| Engine | `EntityRelationshipEngine` |

| Registry | `IRelationshipDefinitionRegistry` + entity catalogs (e.g. `CustomerRelationshipCatalog`) |



### L5 RADAR — M32.8

| Attribute | Location |
|-----------|----------|
| Table | `BTRPD_EntityAnalytics_Radar` |
| Grain | `(EntityType, EntityId, PeriodYear, PeriodMonth, AxisKpiId)` |
| Scores | 0–100 peer-percentile normalized (ADR-EA-007) |
| Engine | `EntityRadarEngine` + `PeerGroupResolver` |
| Repository | `IEntityAnalyticsRadarRepository` — `SaveRadarScores`, `GetRadarScores`, `GetRadarScoresBatch` |
| KPI metadata | `RadarEligible`, `RadarAxisOrder`, `RadarValueSource`, `RadarSourceKpiId`, `RadarDisplayName`, `PeerGroupRule` |

Profile and compare read L5 only — no normalization at HTTP request time.

---

## Radar Engine (M32.8)

`EntityRadarEngine` runs after L4 in the producer pipeline:

1. Resolve `RadarEligible` axes from KPI pack (ordered by `RadarAxisOrder`)
2. Build peer groups via `PeerGroupResolver` from `EntityTypeRegistration.PeerGroupRuleId`
3. Skip entities when peer group &lt; 5 (`MinRadarPeerGroupSize`)
4. Resolve axis raw values from L0–L3 per `RadarValueSource`
5. Compute peer-percentile scores → `SaveRadarScores`

Customer v1 axes: Revenue, Growth, Stability, Portfolio Strength, Attention Risk, Engagement (see `m32.8-implementation-summary.md`).

---



## Trend Engine (M32.3)



`EntityTrendEngine` reads L1 history for `TrendEligible` KPIs in the entity pack. It organizes chronological series only — **no MoM/YoY computation** (deferred to M32.7 Comparison Engine).



Profile `Trend` section: `IsAvailable = true` when at least one series has points.



---



## Ranking Engine (M32.4)



`EntityRankingEngine` computes competition ranking (1, 2, 2, 4) over the active population at refresh:



| Rule | Behavior |

|------|----------|

| Data source | L1 `NumericValue` for the period (not recomputed from transactions) |

| Active filter | L0 `EA-META-IS-ACTIVE = 1` |

| Null values | Excluded — no L2 row for that entity/KPI/period |

| Direction | `HigherIsBetter` → descending sort; `LowerIsBetter` → ascending |



Profile `Ranking` section: `IsAvailable = true` when L2 history exists for rank-eligible KPIs.



---



## Comparison Engine (M32.7)



Read-time engine — **no snapshot table**. Composes L0/L1 and L5 (peer/radar modes) via `ComparisonContext`:



| Mode | Data source | Consumer |
|------|-------------|----------|
| Cross-Period | L0 CURRENT vs L1 prior months; MoM/YoY deltas | Profile `Comparison` section |
| Multi-Entity | Batch L0 + delegated L1–L4 per entity (2–5 entities) | `GET /api/entity-analytics/compare`, `CustomerCompareView` |

**Key types:** `IEntityComparisonEngine`, `EntityComparisonEngine`, `EntityComparisonCalculator`, `ComparisonContext`, `ComparisonMetricDto`.

**Search:** `GET /api/entity-analytics/search` — L0-backed entity picker (min 2 chars, entity-type agnostic).

Profile `Comparison` section uses Cross-Period mode by default.



---



## Profile Composition Flow



1. `EntityAnalyticsService.GetProfile` — normalizes entity type, enforces `EntityAnalyticsOptions.EnabledEntityTypes`.

2. `EntityPerformanceProfileComposer.Build`:

   - Resolves identity via `TryResolveIdentity` (meta + dimension rows)

   - Loads L0 metrics (excludes `EA-META-*` / `EA-DIM-*`)

   - Groups KPIs by category via `IKpiRegistry`

   - Formats envelopes via `EntityKpiEnvelopeFormatter`

   - Resolves evidence via `IEntityProfileEvidenceResolver`

   - **Trend** via `IEntityTrendEngine` (M32.3)

   - **Ranking** via `IEntityRankingEngine` (M32.4)

   - **Comparison** via `IEntityComparisonEngine` (M32.7)
   - Placeholders (`NotImplemented`) for Radar until M32.8



### API contract



- **Route:** `GET /api/entity-analytics/{entityType}/{entityId}`

- **Also available:** `GET /api/entity-analytics/types`, `GET /api/entity-analytics/compare`, `GET /api/entity-analytics/search`

- **Response version fields:** `SnapshotVersion`, `ContractVersion` (additive)

- **Unavailable reasons:** `NotImplemented`, `NoSnapshotData`, `NoRegisteredKpis`, `EntityTypeDisabled`



---



## Adding a New Entity Type



Example: **Salesman** (M32.9 entity pack) — **implemented**

Reference: [m32.9-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.9-implementation-summary.md)

1. **Platform registrar** — `EntityTypeCode.Salesman` metadata in `EntityAnalyticsPlatformRegistrar` (`salesman-default`, `salesman-relationships`, route `/analytics/salesmen/{code}`).

2. **`SalesmanEntityAnalyticsRegistrar`** — KPI pack SF-KPI-008/009/010 + radar/meta axes; dimension labels.

3. **`SalesmanEntityAnalyticsProducer`** — maps `DashboardSalesmanAggregateResult.Portfolio` to L0; L0→L5 orchestration (`EntityId = SalesPersonId`, `EntityCode = SalesPersonCode`).

4. **`SalesmanEntityAnalyticsProduceInput`** — wraps salesman aggregate + `DashboardSalesmanRelationshipAggregateResult`.

5. **Worker hook** — `RefreshDashboardSalesmanSnapshotWorker` calls orchestrator after domain save (loads `SalesmanMtdItemRollupDal` + relationship aggregator first).

6. **`SalesmanEntityAnalyticsEvidenceResolver`** — `/reports/sales`, `/reports/piutang` with `salesPersonCode` filter.

7. **Attention / relationship catalogs** — `SalesmanAttentionSignalCatalog`, `SalesmanRelationshipCatalog` (registered in bootstrap).

8. **Enable** — `Salesman` in `EntityAnalytics.EnabledEntityTypes` (API + worker).

9. **Register DI** — `AddSingleton<IEntityAnalyticsRegistrar, SalesmanEntityAnalyticsRegistrar>()`.

10. **Frontend** — `SalesmanProfileView.vue` (shell wrapper), `SalesmanCompareView.vue`, SF01 `ProfileRoute` links.

No changes required to platform engines, `EntityPerformanceProfileComposer`, or `EntityAnalyticsController`.

Example: **Supplier** (M32.10 entity pack) — **implemented**

Reference: [m32.10-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.10-implementation-summary.md)

1. **Platform registrar** — `EntityTypeCode.Supplier` metadata in `EntityAnalyticsPlatformRegistrar` (`supplier-default`, `supplier-relationships`, route `/analytics/suppliers/{code}`).

2. **`SupplierEntityAnalyticsRegistrar`** — KPI pack PU-KPI-001/002/003 + radar/meta axes; dimension labels.

3. **`SupplierEntityAnalyticsProducer`** — maps `DashboardPurchasingManagementAggregateResult.Portfolio` to L0; L0→L5 orchestration (`EntityId = SupplierId`, `EntityCode = SupplierCode`).

4. **`SupplierEntityAnalyticsProduceInput`** — wraps management aggregate + `DashboardSupplierRelationshipAggregateResult`.

5. **Worker hook** — `RefreshDashboardPurchasingManagementSnapshotWorker` calls orchestrator after domain save (loads `ISupplierDal`, `ISupplierMtdItemRollupDal`, relationship aggregator first).

6. **`SupplierEntityAnalyticsEvidenceResolver`** — `/reports/purchasing`, `/reports/inventory` with `supplierCode` filter.

7. **Attention / relationship catalogs** — `SupplierAttentionSignalCatalog`, `SupplierRelationshipCatalog`.

8. **Enable** — `Supplier` in `EntityAnalytics.EnabledEntityTypes` (API + worker).

9. **Register DI** — `AddSingleton<IEntityAnalyticsRegistrar, SupplierEntityAnalyticsRegistrar>()`.

10. **Frontend** — `SupplierProfileView.vue`, `SupplierCompareView.vue`, PU01 `ProfileRoute` links.

Example: **Item** (M32.11 entity pack) — **implemented**

Reference: [m32.11-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.11-implementation-summary.md)

1. **Platform registrar** — `EntityTypeCode.Item` metadata in `EntityAnalyticsPlatformRegistrar` (`item-default`, `item-relationships`, route `/analytics/items/{code}`).

2. **`ItemEntityAnalyticsRegistrar`** — KPI pack IN-KPI-001/020/021 + radar/meta axes; dimension labels.

3. **`ItemEntityAnalyticsProducer`** — maps `DashboardItemPortfolioRow` list to L0; L0→L5 orchestration (`EntityId = BrgId`, `EntityCode = BrgCode`).

4. **`ItemEntityAnalyticsProduceInput`** — wraps risk/forecast aggregates + portfolio + `DashboardItemRelationshipAggregateResult`.

5. **Worker hook** — `RefreshDashboardInventoryRiskSnapshotWorker` calls orchestrator after domain save (loads `ISalesmanMtdItemRollupDal`, portfolio builder, relationship aggregator first).

6. **`ItemEntityAnalyticsEvidenceResolver`** — `/reports/inventory` with `brgCode` filter.

7. **Attention / relationship catalogs** — `ItemAttentionSignalCatalog`, `ItemRelationshipCatalog`.

8. **Enable** — `Item` in `EntityAnalytics.EnabledEntityTypes` (API + worker).

9. **Register DI** — `AddSingleton<IEntityAnalyticsRegistrar, ItemEntityAnalyticsRegistrar>()`.

10. **Frontend** — `ItemProfileView.vue`, `ItemCompareView.vue`, IN02 `ProfileRoute` links.

**ADR-EA-011:** L1 monthly history only when `DashboardItemPortfolioRow.IsTrendEligible` (stock > 0 OR sale in 24 months).

---



## Repository Layers



`IEntityAnalyticsRepository` composes layer-specific interfaces:



| Interface | Layer | Status |

|-----------|-------|--------|

| `IEntityAnalyticsCurrentRepository` | L0 CURRENT | **Implemented** (M32.2) |

| `IEntityAnalyticsMonthlyRepository` | L1 monthly | **Implemented** (M32.3) |

| `IEntityAnalyticsRankingRepository` | L2 ranking | **Implemented** (M32.4) |

| `IEntityAnalyticsAttentionRepository` | L3 attention | **Implemented** (M32.5) |

| `IEntityAnalyticsRelationshipRepository` | L4 relationships | **Implemented** (M32.6) |

| `IEntityAnalyticsRadarRepository` | L5 radar | **Implemented** (M32.8) |



**Adding L1 to a new entity type:** After L0 write, call `_monthCloseService.EnsurePriorMonthClosed` and `_repository.SaveMonthlyHistory` with TrendEligible KPI values.



**Adding L2 to a new entity type:** After `SaveMonthlyHistory`, call `_rankingEngine.ComputeAndPersistRanks(...)`. Flag KPIs with `RankEligible = true`.



---



## Remaining Roadmap



See [entity-analytics-roadmap-authoritative.md](../../work/btr-portal/entity-analytics/entity-analytics-roadmap-authoritative.md).



| Next | Milestone |
|------|-----------|
| **M32.12** | Future Entity Types (Warehouse, Wilayah, Category) |
| M32.12+ | Future entity types |



---



## Configuration



```json

"EntityAnalytics": {

  "EnabledEntityTypes": [ "Customer", "Salesman", "Supplier", "Item" ],

  "HistoryRetentionMonths": 36

}

```



- `EnabledEntityTypes` — empty array = all registered types enabled (dev convenience).

- `HistoryRetentionMonths` — L1/L2 purge window (default 36); trend chart shows up to 12 months.



---



## Historical Backfill — Item (M32.B1.7)



Item is the **last** entity type in the one-time historical replay pipeline (after Salesman, Customer, Supplier). It has the highest SKU cardinality and should run in a **dedicated maintenance window**.



### Active subset (ADR-EA-011)



L1 monthly history is written only for SKUs that are **trend-eligible** at `PeriodEnd`:



- Stock on hand &gt; 0, **or**

- Last faktur date within the trailing **24 months** (730 days)



Dormant SKUs receive L0 CURRENT from live workers only; they are excluded from historical L1/L2/L5 backfill.



### Recommended staging sequence



1. Dry-run: `--dry-run --entity-type Item --from-period YYYY-MM --to-period YYYY-MM`

2. Smoke: 12-month window on staging

3. Full: 36-month window (`HistoryRetentionMonths`) during off-peak production maintenance



### CLI tuning



```text

btr.portal.worker --domain EntityAnalyticsHistoricalBackfill \

  --entity-type Item --from-period 2023-01 --to-period 2025-12 \

  --confirm BACKFILL --batch-size 500

```



- `--batch-size` (default **500**) chunks L1 `ReplaceMonthlyHistoryForPeriod` inserts for Item. Increase if insert pressure is observed; decrease if transaction timeouts occur.

- SqlBulkCopy optimization is deferred to M32.B1.9.



### Idempotent rerun



To replace a bad month:



```text

--force --from-period YYYY-MM --to-period YYYY-MM --entity-type Item --confirm BACKFILL

```



Ensure the entity-type mutex blocks concurrent live `InventoryRisk` refresh during Item backfill.



---



## Testing Checklist



| Area | Test class |

|------|------------|

| Trend engine / L1 ordering | `EntityTrendEngineTest` |

| Ranking engine / L2 math | `EntityRankingCalculatorTest`, `EntityRankingEngineTest` |

| Producer L0/L1/L2 mapping | `CustomerEntityAnalyticsProducerTest`, `SalesmanEntityAnalyticsProducerTest`, `ItemEntityAnalyticsProducerTest` |
| CU01/CU05 reconciliation | `CustomerEntityAnalyticsReconciliationTest` |
| SF01 reconciliation | `SalesmanEntityAnalyticsReconciliationTest` |
| PU01 reconciliation | `SupplierEntityAnalyticsReconciliationTest` |
| IN02 reconciliation | `ItemEntityAnalyticsReconciliationTest` |
| Item replay backfill | `ItemReplayBackfillReconciliationTest` |
| Relationship aggregator | `DashboardSalesmanRelationshipAggregatorTest`, `DashboardItemRelationshipAggregatorTest` |

| KPI registry / pack validation | `EntityAnalyticsKpiRegistryTest` |

| Profile composer | `EntityPerformanceProfileComposerTest` |

| Enabled-type gate | `EntityAnalyticsServiceTest` |



---



## Related Documents



| Document | Role |

|----------|------|

| [entity-analytics-roadmap-authoritative.md](../../work/btr-portal/entity-analytics/entity-analytics-roadmap-authoritative.md) | Milestone sequence SSOT |

| [entity-analytics-architecture.md](../../work/btr-portal/entity-analytics/entity-analytics-architecture.md) | Platform architecture |

| [implementation-plan-entity-analytics.md](../../work/btr-portal/entity-analytics/implementation-plan-entity-analytics.md) | Execution detail |

| [m32.2-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.2-implementation-summary.md) | Customer milestone |

| [m32.3-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.3-implementation-summary.md) | L1 + Trend milestone |

| [m32.8-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.8-implementation-summary.md) | L5 Radar milestone |
| [m32.9-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.9-implementation-summary.md) | Salesman entity pack |
| [m32.10-implementation-summary.md](../../work/btr-portal/entity-analytics/m32.10-implementation-summary.md) | Supplier entity pack |

