using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SupplierEntityAnalyticsProducer : IEntityAnalyticsProducer
    {
        public string EntityType => EntityTypeCode.Supplier;

        public string WorkerDomain => "PurchasingManagement";

        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityAnalyticsMonthCloseService _monthCloseService;
        private readonly IEntityRankingEngine _rankingEngine;
        private readonly IEntityAttentionEngine _attentionEngine;
        private readonly IEntityRelationshipEngine _relationshipEngine;
        private readonly IEntityRadarEngine _radarEngine;
        private readonly IAttentionSignalRegistry _attentionSignals;
        private readonly IPrincipalSalesOutSnapshotDal _salesOutSnapshotDal;

        public SupplierEntityAnalyticsProducer(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityAnalyticsMonthCloseService monthCloseService,
            IEntityRankingEngine rankingEngine,
            IEntityAttentionEngine attentionEngine,
            IEntityRelationshipEngine relationshipEngine,
            IEntityRadarEngine radarEngine,
            IAttentionSignalRegistry attentionSignals,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal = null)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _monthCloseService = monthCloseService;
            _rankingEngine = rankingEngine;
            _attentionEngine = attentionEngine;
            _relationshipEngine = relationshipEngine;
            _radarEngine = radarEngine;
            _attentionSignals = attentionSignals;
            _salesOutSnapshotDal = salesOutSnapshotDal;
        }

        public void Produce(EntityAnalyticsProduceContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.DomainInput as SupplierEntityAnalyticsProduceInput;
            var portfolio = input?.ManagementAggregate?.Portfolio;
            var (periodYear, periodMonth) = EntityAnalyticsProducerReplaySupport.ResolvePeriod(context);
            var salesOut = ReadOwnedSalesOut(periodYear, periodMonth);

            if (portfolio == null || portfolio.Count == 0)
            {
                var retainedRows = new List<EntityAnalyticsCurrentRow>();
                var retainedMonthly = new List<EntityAnalyticsMonthlyRow>();
                if (salesOut.MatchesPeriod)
                {
                    AppendSnapshotOnlySalesOut(
                        retainedRows, retainedMonthly, periodYear, periodMonth, context.GeneratedAt, salesOut);
                }
                else
                {
                    RetainPersistedSalesOut(retainedRows, context.GeneratedAt, salesOut);
                }

                EntityAnalyticsProducerReplaySupport.PersistL0(
                    _repository, context, EntityTypeCode.Supplier, retainedRows);
                if (retainedMonthly.Count > 0)
                {
                    EntityAnalyticsProducerReplaySupport.PersistL1(
                        _repository, _monthCloseService, context, EntityTypeCode.Supplier, retainedMonthly);
                    _rankingEngine.ComputeAndPersistRanks(
                        EntityTypeCode.Supplier,
                        periodYear,
                        periodMonth,
                        context.RefreshLogId,
                        context.GeneratedAt,
                        context.Replay);
                }

                return;
            }

            var attentionById = BuildAttentionIndex(input.ManagementAggregate);
            var attentionDetailById = BuildAttentionDetailIndex(input.ManagementAggregate);
            var rows = new List<EntityAnalyticsCurrentRow>();
            var monthlyRows = new List<EntityAnalyticsMonthlyRow>();
            var signalsByEntity = new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var supplier in portfolio)
            {
                if (string.IsNullOrWhiteSpace(supplier.SupplierId))
                    continue;

                var entityId = supplier.SupplierId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(supplier.SupplierCode)
                    ? entityId
                    : supplier.SupplierCode.Trim();
                var generatedAt = context.GeneratedAt;

                rows.AddRange(BuildSupplierRows(supplier, entityId, entityCode, generatedAt, attentionById));
                monthlyRows.AddRange(BuildMonthlyRows(supplier, entityId, entityCode, periodYear, periodMonth, generatedAt));
                ComposeSalesOut(rows, monthlyRows, entityId, entityCode, periodYear, periodMonth, generatedAt, salesOut);
                signalsByEntity[entityId] = BuildAttentionSnapshots(
                    entityId,
                    entityCode,
                    attentionById,
                    attentionDetailById);
            }

            AppendSnapshotOnlySalesOut(rows, monthlyRows, periodYear, periodMonth, context.GeneratedAt, salesOut);
            RetainPersistedSalesOut(rows, context.GeneratedAt, salesOut);

            EntityAnalyticsProducerReplaySupport.PersistL0(_repository, context, EntityTypeCode.Supplier, rows);

            EntityAnalyticsProducerReplaySupport.PersistL1(
                _repository, _monthCloseService, context, EntityTypeCode.Supplier, monthlyRows);

            _rankingEngine.ComputeAndPersistRanks(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _attentionEngine.DiffAndPersistSignals(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                signalsByEntity,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _relationshipEngine.PersistRollups(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                BuildRelationshipSnapshots(input, context),
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _radarEngine.ComputeAndPersistScores(
                EntityTypeCode.Supplier,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);
        }

        private SalesOutComposition ReadOwnedSalesOut(int periodYear, int periodMonth)
        {
            var existing = _repository.GetCurrentKpiPopulation(
                EntityTypeCode.Supplier,
                PrincipalKpiCatalog.SalesOutId);

            var snapshot = _salesOutSnapshotDal?.GetCurrent();
            var matchesPeriod = snapshot != null
                && string.Equals(snapshot.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.OrdinalIgnoreCase)
                && snapshot.PeriodYear == periodYear
                && snapshot.PeriodMonth == periodMonth;

            var bySupplierId = new Dictionary<string, PrincipalSalesOutRow>(StringComparer.OrdinalIgnoreCase);
            if (matchesPeriod)
            {
                foreach (var row in snapshot.Principals ?? new List<PrincipalSalesOutRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SupplierId))
                        continue;

                    if (!string.IsNullOrWhiteSpace(row.KpiId)
                        && !string.Equals(row.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    bySupplierId[row.SupplierId.Trim()] = row;
                }
            }

            return new SalesOutComposition(matchesPeriod, bySupplierId, existing);
        }

        private void ComposeSalesOut(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            SalesOutComposition salesOut)
        {
            if (salesOut.MatchesPeriod && salesOut.BySupplierId.TryGetValue(entityId, out var owned))
            {
                AddSalesOutRows(rows, monthlyRows, entityId, entityCode, owned.SalesOutAmount, periodYear, periodMonth, generatedAt);
                return;
            }

            if (salesOut.MatchesPeriod)
                return;

            var persisted = salesOut.Existing.FirstOrDefault(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
            if (persisted?.NumericValue == null)
                return;

            AddSalesOutRows(rows, null, entityId, entityCode, persisted.NumericValue.Value, periodYear, periodMonth, generatedAt);
        }

        private void AppendSnapshotOnlySalesOut(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            int periodYear,
            int periodMonth,
            DateTime generatedAt,
            SalesOutComposition salesOut)
        {
            if (!salesOut.MatchesPeriod)
                return;

            foreach (var owned in salesOut.BySupplierId.Values)
            {
                var entityId = owned.SupplierId.Trim();
                if (ContainsSalesOut(rows, entityId))
                    continue;

                var entityCode = entityId;
                var displayName = string.IsNullOrWhiteSpace(owned.SupplierName) ? entityId : owned.SupplierName.Trim();
                rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, displayName, generatedAt));
                rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, 1m, null, generatedAt));
                AddSalesOutRows(rows, monthlyRows, entityId, entityCode, owned.SalesOutAmount, periodYear, periodMonth, generatedAt);
            }
        }

        private void RetainPersistedSalesOut(
            ICollection<EntityAnalyticsCurrentRow> rows,
            DateTime generatedAt,
            SalesOutComposition salesOut)
        {
            if (salesOut.MatchesPeriod)
                return;

            foreach (var persisted in salesOut.Existing)
            {
                if (string.IsNullOrWhiteSpace(persisted.EntityId) || persisted.NumericValue == null)
                    continue;

                var entityId = persisted.EntityId.Trim();
                if (ContainsSalesOut(rows, entityId))
                    continue;

                var entityCode = string.IsNullOrWhiteSpace(persisted.EntityCode) ? entityId : persisted.EntityCode.Trim();
                CopyIdentity(rows, entityId, entityCode, generatedAt);
                AddSalesOutRows(rows, null, entityId, entityCode, persisted.NumericValue.Value, 0, 0, generatedAt);
            }
        }

        private void CopyIdentity(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            DateTime generatedAt)
        {
            var current = _repository.GetCurrentMetrics(EntityTypeCode.Supplier, entityId);
            var displayName = current.FirstOrDefault(row =>
                string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase));
            var isActive = current.FirstOrDefault(row =>
                string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase));

            if (!rows.Any(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.DisplayName, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(CreateMetaRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.DisplayName,
                    null,
                    displayName?.TextValue ?? entityCode,
                    generatedAt));
            }

            if (!rows.Any(row =>
                    string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.KpiId, EntityAnalyticsMetaKpiIds.IsActive, StringComparison.OrdinalIgnoreCase)))
            {
                rows.Add(CreateMetaRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.IsActive,
                    isActive?.NumericValue ?? 1m,
                    null,
                    generatedAt));
            }
        }

        private void AddSalesOutRows(
            ICollection<EntityAnalyticsCurrentRow> rows,
            ICollection<EntityAnalyticsMonthlyRow> monthlyRows,
            string entityId,
            string entityCode,
            decimal amount,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            if (ContainsSalesOut(rows, entityId))
                return;

            rows.Add(CreateRow(entityId, entityCode, PrincipalKpiCatalog.SalesOutId, amount, null, generatedAt));

            if (monthlyRows == null || periodYear <= 0 || periodMonth <= 0)
                return;

            if (!_kpiRegistry.TryGetMetadata(PrincipalKpiCatalog.SalesOutId, out var metadata) || !metadata.TrendEligible)
                return;

            monthlyRows.Add(new EntityAnalyticsMonthlyRow
            {
                EntityType = EntityTypeCode.Supplier,
                EntityId = entityId,
                EntityCode = entityCode,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                KpiId = PrincipalKpiCatalog.SalesOutId,
                NumericValue = amount,
                PeriodSemantics = metadata.PeriodSemantics,
                DefinitionVersion = metadata.DefinitionVersion,
                IsClosed = false,
                GeneratedAt = generatedAt
            });
        }

        private static bool ContainsSalesOut(IEnumerable<EntityAnalyticsCurrentRow> rows, string entityId)
        {
            return rows.Any(row =>
                string.Equals(row.EntityId, entityId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(row.KpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.OrdinalIgnoreCase));
        }

        private sealed class SalesOutComposition
        {
            public SalesOutComposition(
                bool matchesPeriod,
                IReadOnlyDictionary<string, PrincipalSalesOutRow> bySupplierId,
                IReadOnlyList<EntityAnalyticsPeriodMetricRow> existing)
            {
                MatchesPeriod = matchesPeriod;
                BySupplierId = bySupplierId ?? new Dictionary<string, PrincipalSalesOutRow>(StringComparer.OrdinalIgnoreCase);
                Existing = existing ?? Array.Empty<EntityAnalyticsPeriodMetricRow>();
            }

            public bool MatchesPeriod { get; }

            public IReadOnlyDictionary<string, PrincipalSalesOutRow> BySupplierId { get; }

            public IReadOnlyList<EntityAnalyticsPeriodMetricRow> Existing { get; }
        }

        private IEnumerable<EntityAnalyticsMonthlyRow> BuildMonthlyRows(
            DashboardPurchasingManagementPortfolioRow supplier,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            foreach (var (kpiId, value) in BuildTrendKpiValues(supplier))
            {
                if (!_kpiRegistry.TryGetMetadata(kpiId, out var metadata) || !metadata.TrendEligible)
                    continue;

                yield return new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Supplier,
                    EntityId = entityId,
                    EntityCode = entityCode,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    KpiId = kpiId,
                    NumericValue = value,
                    PeriodSemantics = metadata.PeriodSemantics,
                    DefinitionVersion = metadata.DefinitionVersion,
                    IsClosed = false,
                    GeneratedAt = generatedAt
                };
            }
        }

        private static IEnumerable<(string KpiId, decimal? Value)> BuildTrendKpiValues(
            DashboardPurchasingManagementPortfolioRow supplier)
        {
            yield return ("PU-KPI-001", supplier.MtdPurchaseAmount);
            yield return ("PU-KPI-002", supplier.MtdInvoiceCount);
            yield return ("PU-KPI-003", supplier.PostedPercent);
        }

        private static Dictionary<string, HashSet<string>> BuildAttentionIndex(
            DashboardPurchasingManagementAggregateResult aggregate)
        {
            var index = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (aggregate?.AttentionList == null)
                return index;

            foreach (var row in aggregate.AttentionList)
            {
                if (!string.Equals(
                        row.EntityType,
                        DashboardPurchasingManagementAggregator.EntityTypePrincipal,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.SupplierId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                var key = row.SupplierId.Trim();
                if (!index.TryGetValue(key, out var signals))
                {
                    signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    index[key] = signals;
                }

                signals.Add(row.SignalKey);
            }

            return index;
        }

        private static Dictionary<string, DashboardPurchasingManagementAttentionRow> BuildAttentionDetailIndex(
            DashboardPurchasingManagementAggregateResult aggregate)
        {
            var index = new Dictionary<string, DashboardPurchasingManagementAttentionRow>(StringComparer.OrdinalIgnoreCase);
            if (aggregate?.AttentionList == null)
                return index;

            foreach (var row in aggregate.AttentionList)
            {
                if (string.IsNullOrWhiteSpace(row.SupplierId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                var key = $"{row.SupplierId.Trim()}:{row.SignalKey.Trim()}";
                index[key] = row;
            }

            return index;
        }

        private IReadOnlyList<EntityAttentionSignalSnapshot> BuildAttentionSnapshots(
            string entityId,
            string entityCode,
            IReadOnlyDictionary<string, HashSet<string>> attentionById,
            IReadOnlyDictionary<string, DashboardPurchasingManagementAttentionRow> attentionDetailById)
        {
            if (!attentionById.TryGetValue(entityId, out var signalCodes) || signalCodes.Count == 0)
                return Array.Empty<EntityAttentionSignalSnapshot>();

            var snapshots = new List<EntityAttentionSignalSnapshot>();
            foreach (var signalCode in signalCodes.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                attentionDetailById.TryGetValue($"{entityId}:{signalCode}", out var detail);
                if (!_attentionSignals.TryResolve(EntityType, signalCode, out var definition))
                {
                    definition = new AttentionSignalDefinition
                    {
                        SignalCode = signalCode,
                        SignalCategory = "General",
                        SignalTitle = detail?.SignalLabel ?? signalCode
                    };
                }

                snapshots.Add(new EntityAttentionSignalSnapshot
                {
                    EntityId = entityId,
                    EntityCode = entityCode,
                    SignalCode = signalCode,
                    SignalCategory = definition.SignalCategory,
                    SignalTitle = detail?.SignalLabel ?? definition.SignalTitle ?? signalCode
                });
            }

            return snapshots;
        }

        private static IReadOnlyList<EntityRelationshipSnapshot> BuildRelationshipSnapshots(
            SupplierEntityAnalyticsProduceInput input,
            EntityAnalyticsProduceContext context)
        {
            var portfolio = input?.ManagementAggregate?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
                return Array.Empty<EntityRelationshipSnapshot>();

            var customerLookup = context?.CustomerIdentityLookup;

            var relationshipById = input.RelationshipAggregate?.BySupplierId
                ?? new Dictionary<string, DashboardSupplierRelationshipSupplierRollup>(StringComparer.OrdinalIgnoreCase);

            var snapshots = new List<EntityRelationshipSnapshot>();

            foreach (var supplier in portfolio)
            {
                if (string.IsNullOrWhiteSpace(supplier.SupplierId))
                    continue;

                var entityId = supplier.SupplierId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(supplier.SupplierCode)
                    ? entityId
                    : supplier.SupplierCode.Trim();

                if (!relationshipById.TryGetValue(entityId, out var rollup))
                    continue;

                foreach (var customer in rollup.TopCustomers)
                {
                    var customerIdentity = EntityAnalyticsCustomerIdentityResolver.Resolve(
                        customer.CustomerCode,
                        customerLookup,
                        customerName: customer.CustomerName);

                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SupplierRelationshipCatalog.TopCustomersByOmzet,
                        TargetEntityType = EntityTypeCode.Customer,
                        TargetEntityId = customerIdentity.CustomerId,
                        TargetEntityCode = customerIdentity.CustomerCode,
                        TargetDisplayName = customer.CustomerName ?? customerIdentity.CustomerCode,
                        MetricValue = customer.MetricValue
                    });
                }

                foreach (var salesman in rollup.TopSalesmen)
                {
                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SupplierRelationshipCatalog.TopSalesmenByOmzet,
                        TargetEntityType = EntityTypeCode.Salesman,
                        TargetEntityId = salesman.SalesPersonId,
                        TargetEntityCode = salesman.SalesPersonCode ?? salesman.SalesPersonId,
                        TargetDisplayName = salesman.SalesPersonName ?? salesman.SalesPersonCode,
                        MetricValue = salesman.MetricValue
                    });
                }

                foreach (var item in rollup.TopItems)
                {
                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SupplierRelationshipCatalog.TopProductsByOmzet,
                        TargetEntityType = EntityTypeCode.Item,
                        TargetEntityId = item.BrgId,
                        TargetEntityCode = item.BrgCode ?? item.BrgId,
                        TargetDisplayName = item.BrgName,
                        MetricValue = item.MetricValue
                    });
                }
            }

            return snapshots;
        }

        private static IEnumerable<EntityAnalyticsCurrentRow> BuildSupplierRows(
            DashboardPurchasingManagementPortfolioRow supplier,
            string entityId,
            string entityCode,
            DateTime generatedAt,
            IReadOnlyDictionary<string, HashSet<string>> attentionById)
        {
            var rows = new List<EntityAnalyticsCurrentRow>();

            foreach (var (kpiId, value) in BuildTrendKpiValues(supplier))
            {
                rows.Add(CreateRow(entityId, entityCode, kpiId, value, null, generatedAt));
            }

            if (supplier.InventoryValue.HasValue)
            {
                rows.Add(CreateRow(
                    entityId,
                    entityCode,
                    EntityAnalyticsMetaKpiIds.InventoryValue,
                    supplier.InventoryValue,
                    null,
                    generatedAt));
            }

            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, supplier.SupplierName, generatedAt));
            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, supplier.IsActiveMtd ? 1m : 0m, null, generatedAt));

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.PurchaseShare, supplier.PercentOfPurchase, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.AtRiskValue, supplier.AtRiskValue, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveMtd, supplier.IsActiveMtd ? "Yes" : "No", generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveSkuCount, supplier.ActiveSkuCount.ToString(CultureInfo.InvariantCulture), generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.CatalogPenetration, supplier.CatalogPenetrationPercent, generatedAt);

            if (attentionById.TryGetValue(entityId, out var signals) && signals.Count > 0)
            {
                var signalList = string.Join(", ", signals.OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
                AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.AttentionSignals, signalList, generatedAt);
            }

            return rows;
        }

        private static void AddDimension(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            string dimensionKpiId,
            string value,
            DateTime generatedAt)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            rows.Add(CreateMetaRow(entityId, entityCode, dimensionKpiId, null, value.Trim(), generatedAt));
        }

        private static void AddDimension(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            string dimensionKpiId,
            decimal? value,
            DateTime generatedAt)
        {
            if (!value.HasValue)
                return;

            rows.Add(CreateMetaRow(entityId, entityCode, dimensionKpiId, value, null, generatedAt));
        }

        private static EntityAnalyticsCurrentRow CreateRow(
            string entityId,
            string entityCode,
            string kpiId,
            decimal? numericValue,
            string textValue,
            DateTime generatedAt)
        {
            return new EntityAnalyticsCurrentRow
            {
                EntityAnalyticsCurrentId = Ulid.NewUlid().ToString(),
                EntityType = EntityTypeCode.Supplier,
                EntityId = entityId,
                EntityCode = entityCode,
                KpiId = kpiId,
                NumericValue = numericValue,
                TextValue = textValue,
                DefinitionVersion = 1,
                GeneratedAt = generatedAt
            };
        }

        private static EntityAnalyticsCurrentRow CreateMetaRow(
            string entityId,
            string entityCode,
            string kpiId,
            decimal? numericValue,
            string textValue,
            DateTime generatedAt)
        {
            return CreateRow(entityId, entityCode, kpiId, numericValue, textValue, generatedAt);
        }
    }
}
