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
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class ItemEntityAnalyticsProducer : IEntityAnalyticsProducer
    {
        public string EntityType => EntityTypeCode.Item;

        public string WorkerDomain => "InventoryRisk";

        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityAnalyticsMonthCloseService _monthCloseService;
        private readonly IEntityRankingEngine _rankingEngine;
        private readonly IEntityAttentionEngine _attentionEngine;
        private readonly IEntityRelationshipEngine _relationshipEngine;
        private readonly IEntityRadarEngine _radarEngine;
        private readonly IAttentionSignalRegistry _attentionSignals;

        public ItemEntityAnalyticsProducer(
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

            var input = context.DomainInput as ItemEntityAnalyticsProduceInput;
            var portfolio = input?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
            {
                _repository.ReplaceCurrentMetrics(EntityTypeCode.Item, Array.Empty<EntityAnalyticsCurrentRow>(), context.RefreshLogId);
                return;
            }

            var attentionById = BuildAttentionIndex(input);
            var attentionDetailById = BuildAttentionDetailIndex(input);
            var rows = new List<EntityAnalyticsCurrentRow>();
            var monthlyRows = new List<EntityAnalyticsMonthlyRow>();
            var signalsByEntity = new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>(
                StringComparer.OrdinalIgnoreCase);

            var businessDate = context.BusinessDate == default
                ? context.GeneratedAt.Date
                : context.BusinessDate.Date;
            var periodYear = businessDate.Year;
            var periodMonth = businessDate.Month;

            foreach (var item in portfolio)
            {
                if (string.IsNullOrWhiteSpace(item.BrgId))
                    continue;

                var entityId = item.BrgId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(item.BrgCode)
                    ? entityId
                    : item.BrgCode.Trim();
                var generatedAt = context.GeneratedAt;

                rows.AddRange(BuildItemRows(item, entityId, entityCode, generatedAt, attentionById));
                if (item.IsTrendEligible)
                {
                    monthlyRows.AddRange(BuildMonthlyRows(item, entityId, entityCode, periodYear, periodMonth, generatedAt));
                }

                signalsByEntity[entityId] = BuildAttentionSnapshots(
                    entityId,
                    entityCode,
                    attentionById,
                    attentionDetailById);
            }

            _repository.ReplaceCurrentMetrics(EntityTypeCode.Item, rows, context.RefreshLogId);

            _monthCloseService.EnsurePriorMonthClosed(EntityTypeCode.Item, context);
            _repository.SaveMonthlyHistory(EntityTypeCode.Item, monthlyRows, context.RefreshLogId);

            _rankingEngine.ComputeAndPersistRanks(
                EntityTypeCode.Item,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt);

            _attentionEngine.DiffAndPersistSignals(
                EntityTypeCode.Item,
                periodYear,
                periodMonth,
                signalsByEntity,
                context.RefreshLogId,
                context.GeneratedAt);

            _relationshipEngine.PersistRollups(
                EntityTypeCode.Item,
                periodYear,
                periodMonth,
                BuildRelationshipSnapshots(input),
                context.RefreshLogId,
                context.GeneratedAt);

            _radarEngine.ComputeAndPersistScores(
                EntityTypeCode.Item,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt);
        }

        private IEnumerable<EntityAnalyticsMonthlyRow> BuildMonthlyRows(
            DashboardItemPortfolioRow item,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            foreach (var (kpiId, value) in BuildTrendKpiValues(item))
            {
                if (!_kpiRegistry.TryGetMetadata(kpiId, out var metadata) || !metadata.TrendEligible)
                    continue;

                if (!value.HasValue)
                    continue;

                yield return new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Item,
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

        private static IEnumerable<(string KpiId, decimal? Value)> BuildTrendKpiValues(DashboardItemPortfolioRow item)
        {
            yield return ("IN-KPI-001", item.InventoryValue);
            yield return ("IN-KPI-020", item.DaysOfSupply);
            yield return ("IN-KPI-021", item.RecommendedPurchaseQty);
        }

        private static Dictionary<string, HashSet<string>> BuildAttentionIndex(ItemEntityAnalyticsProduceInput input)
        {
            var index = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            if (input?.RiskAggregate?.AttentionList != null)
            {
                foreach (var row in input.RiskAggregate.AttentionList)
                {
                    AddSignal(index, row.BrgId, row.SignalKey);
                }
            }

            if (input?.ForecastAggregate?.TopRisks != null)
            {
                foreach (var row in input.ForecastAggregate.TopRisks)
                {
                    AddSignal(index, row.BrgId, row.SignalKey);
                }
            }

            return index;
        }

        private static Dictionary<string, AttentionDetail> BuildAttentionDetailIndex(ItemEntityAnalyticsProduceInput input)
        {
            var index = new Dictionary<string, AttentionDetail>(StringComparer.OrdinalIgnoreCase);

            if (input?.RiskAggregate?.AttentionList != null)
            {
                foreach (var row in input.RiskAggregate.AttentionList)
                {
                    if (string.IsNullOrWhiteSpace(row.BrgId) || string.IsNullOrWhiteSpace(row.SignalKey))
                        continue;

                    var key = $"{row.BrgId.Trim()}:{row.SignalKey.Trim()}";
                    index[key] = new AttentionDetail
                    {
                        SignalLabel = row.SignalLabel
                    };
                }
            }

            if (input?.ForecastAggregate?.TopRisks != null)
            {
                foreach (var row in input.ForecastAggregate.TopRisks)
                {
                    if (string.IsNullOrWhiteSpace(row.BrgId) || string.IsNullOrWhiteSpace(row.SignalKey))
                        continue;

                    var key = $"{row.BrgId.Trim()}:{row.SignalKey.Trim()}";
                    index[key] = new AttentionDetail
                    {
                        SignalLabel = row.SignalLabel
                    };
                }
            }

            return index;
        }

        private static void AddSignal(IDictionary<string, HashSet<string>> index, string brgId, string signalKey)
        {
            if (string.IsNullOrWhiteSpace(brgId) || string.IsNullOrWhiteSpace(signalKey))
                return;

            var key = brgId.Trim();
            if (!index.TryGetValue(key, out var signals))
            {
                signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                index[key] = signals;
            }

            signals.Add(signalKey.Trim());
        }

        private IReadOnlyList<EntityAttentionSignalSnapshot> BuildAttentionSnapshots(
            string entityId,
            string entityCode,
            IReadOnlyDictionary<string, HashSet<string>> attentionById,
            IReadOnlyDictionary<string, AttentionDetail> attentionDetailById)
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
            ItemEntityAnalyticsProduceInput input)
        {
            var portfolio = input?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
                return Array.Empty<EntityRelationshipSnapshot>();

            var relationshipById = input.RelationshipAggregate?.ByBrgId
                ?? new Dictionary<string, DashboardItemRelationshipItemRollup>(StringComparer.OrdinalIgnoreCase);

            var snapshots = new List<EntityRelationshipSnapshot>();

            foreach (var item in portfolio)
            {
                if (string.IsNullOrWhiteSpace(item.BrgId))
                    continue;

                var entityId = item.BrgId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(item.BrgCode)
                    ? entityId
                    : item.BrgCode.Trim();

                if (!string.IsNullOrWhiteSpace(item.SupplierId) || !string.IsNullOrWhiteSpace(item.SupplierCode))
                {
                    var supplierId = !string.IsNullOrWhiteSpace(item.SupplierId)
                        ? item.SupplierId.Trim()
                        : item.SupplierCode.Trim();
                    var supplierCode = !string.IsNullOrWhiteSpace(item.SupplierCode)
                        ? item.SupplierCode.Trim()
                        : supplierId;

                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = ItemRelationshipCatalog.PrimarySupplier,
                        TargetEntityType = EntityTypeCode.Supplier,
                        TargetEntityId = supplierId,
                        TargetEntityCode = supplierCode,
                        TargetDisplayName = item.SupplierName ?? supplierCode
                    });
                }

                if (!relationshipById.TryGetValue(entityId, out var rollup))
                    continue;

                foreach (var customer in rollup.TopCustomers)
                {
                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = ItemRelationshipCatalog.TopCustomersByOmzet,
                        TargetEntityType = EntityTypeCode.Customer,
                        TargetEntityId = customer.CustomerCode,
                        TargetEntityCode = customer.CustomerCode,
                        TargetDisplayName = customer.CustomerName ?? customer.CustomerCode,
                        MetricValue = customer.MetricValue
                    });
                }

                foreach (var salesman in rollup.TopSalesmen)
                {
                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = ItemRelationshipCatalog.TopSalesmenByOmzet,
                        TargetEntityType = EntityTypeCode.Salesman,
                        TargetEntityId = salesman.SalesPersonId,
                        TargetEntityCode = salesman.SalesPersonCode ?? salesman.SalesPersonId,
                        TargetDisplayName = salesman.SalesPersonName ?? salesman.SalesPersonCode,
                        MetricValue = salesman.MetricValue
                    });
                }
            }

            return snapshots;
        }

        private static IEnumerable<EntityAnalyticsCurrentRow> BuildItemRows(
            DashboardItemPortfolioRow item,
            string entityId,
            string entityCode,
            DateTime generatedAt,
            IReadOnlyDictionary<string, HashSet<string>> attentionById)
        {
            var rows = new List<EntityAnalyticsCurrentRow>();

            foreach (var (kpiId, value) in BuildTrendKpiValues(item))
            {
                if (value.HasValue)
                {
                    rows.Add(CreateRow(entityId, entityCode, kpiId, value, null, generatedAt));
                }
            }

            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, item.BrgName, generatedAt));
            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, item.IsActive ? 1m : 0m, null, generatedAt));

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Category, item.CategoryName, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.SupplierName, item.SupplierName, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.MovementClass, item.MovementClass, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.QtyOnHand, item.Qty, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.DaysSinceLastFaktur, item.DaysSinceLastFaktur, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.CustomerCount, item.DistinctCustomerCount, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveMtd, item.IsActive ? "Yes" : "No", generatedAt);

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

        private static void AddDimension(
            ICollection<EntityAnalyticsCurrentRow> rows,
            string entityId,
            string entityCode,
            string dimensionKpiId,
            int value,
            DateTime generatedAt)
        {
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
                EntityType = EntityTypeCode.Item,
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

        private sealed class AttentionDetail
        {
            public string SignalLabel { get; set; }
        }
    }
}
