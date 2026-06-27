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

        public SupplierEntityAnalyticsProducer(
            IEntityAnalyticsRepository repository,
            IKpiRegistry kpiRegistry,
            IEntityAnalyticsMonthCloseService monthCloseService,
            IEntityRankingEngine rankingEngine,
            IEntityAttentionEngine attentionEngine,
            IEntityRelationshipEngine relationshipEngine,
            IEntityRadarEngine radarEngine,
            IAttentionSignalRegistry attentionSignals)
        {
            _repository = repository;
            _kpiRegistry = kpiRegistry;
            _monthCloseService = monthCloseService;
            _rankingEngine = rankingEngine;
            _attentionEngine = attentionEngine;
            _relationshipEngine = relationshipEngine;
            _radarEngine = radarEngine;
            _attentionSignals = attentionSignals;
        }

        public void Produce(EntityAnalyticsProduceContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            var input = context.DomainInput as SupplierEntityAnalyticsProduceInput;
            var portfolio = input?.ManagementAggregate?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
            {
                EntityAnalyticsProducerReplaySupport.PersistL0(
                    _repository, context, EntityTypeCode.Supplier, Array.Empty<EntityAnalyticsCurrentRow>());
                return;
            }

            var attentionById = BuildAttentionIndex(input.ManagementAggregate);
            var attentionDetailById = BuildAttentionDetailIndex(input.ManagementAggregate);
            var rows = new List<EntityAnalyticsCurrentRow>();
            var monthlyRows = new List<EntityAnalyticsMonthlyRow>();
            var signalsByEntity = new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>(
                StringComparer.OrdinalIgnoreCase);

            var (periodYear, periodMonth) = EntityAnalyticsProducerReplaySupport.ResolvePeriod(context);

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
                signalsByEntity[entityId] = BuildAttentionSnapshots(
                    entityId,
                    entityCode,
                    attentionById,
                    attentionDetailById);
            }

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
