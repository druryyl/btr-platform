using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Registrars;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Producers
{
    public class SalesmanEntityAnalyticsProducer : IEntityAnalyticsProducer
    {
        public string EntityType => EntityTypeCode.Salesman;

        public string WorkerDomain => "Salesman";

        private readonly IEntityAnalyticsRepository _repository;
        private readonly IKpiRegistry _kpiRegistry;
        private readonly IEntityAnalyticsMonthCloseService _monthCloseService;
        private readonly IEntityRankingEngine _rankingEngine;
        private readonly IEntityAttentionEngine _attentionEngine;
        private readonly IEntityRelationshipEngine _relationshipEngine;
        private readonly IEntityRadarEngine _radarEngine;
        private readonly IAttentionSignalRegistry _attentionSignals;

        public SalesmanEntityAnalyticsProducer(
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

            var input = context.DomainInput as SalesmanEntityAnalyticsProduceInput;
            var portfolio = input?.SalesmanAggregate?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
            {
                _repository.ReplaceCurrentMetrics(EntityTypeCode.Salesman, Array.Empty<EntityAnalyticsCurrentRow>(), context.RefreshLogId);
                return;
            }

            var attentionById = BuildAttentionIndex(input.SalesmanAggregate);
            var attentionDetailById = BuildAttentionDetailIndex(input.SalesmanAggregate);
            var rows = new List<EntityAnalyticsCurrentRow>();
            var monthlyRows = new List<EntityAnalyticsMonthlyRow>();
            var signalsByEntity = new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>(
                StringComparer.OrdinalIgnoreCase);

            var businessDate = context.BusinessDate == default
                ? context.GeneratedAt.Date
                : context.BusinessDate.Date;
            var periodYear = businessDate.Year;
            var periodMonth = businessDate.Month;

            foreach (var rep in portfolio)
            {
                if (string.IsNullOrWhiteSpace(rep.SalesPersonId))
                    continue;

                var entityId = rep.SalesPersonId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(rep.SalesPersonCode)
                    ? entityId
                    : rep.SalesPersonCode.Trim();
                var generatedAt = context.GeneratedAt;

                rows.AddRange(BuildSalesmanRows(rep, entityId, entityCode, generatedAt, attentionById));
                monthlyRows.AddRange(BuildMonthlyRows(rep, entityId, entityCode, periodYear, periodMonth, generatedAt));
                signalsByEntity[entityId] = BuildAttentionSnapshots(
                    entityId,
                    entityCode,
                    attentionById,
                    attentionDetailById);
            }

            _repository.ReplaceCurrentMetrics(EntityTypeCode.Salesman, rows, context.RefreshLogId);

            _monthCloseService.EnsurePriorMonthClosed(EntityTypeCode.Salesman, context);
            _repository.SaveMonthlyHistory(EntityTypeCode.Salesman, monthlyRows, context.RefreshLogId);

            _rankingEngine.ComputeAndPersistRanks(
                EntityTypeCode.Salesman,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt);

            _attentionEngine.DiffAndPersistSignals(
                EntityTypeCode.Salesman,
                periodYear,
                periodMonth,
                signalsByEntity,
                context.RefreshLogId,
                context.GeneratedAt);

            _relationshipEngine.PersistRollups(
                EntityTypeCode.Salesman,
                periodYear,
                periodMonth,
                BuildRelationshipSnapshots(input),
                context.RefreshLogId,
                context.GeneratedAt);

            _radarEngine.ComputeAndPersistScores(
                EntityTypeCode.Salesman,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt);
        }

        private IEnumerable<EntityAnalyticsMonthlyRow> BuildMonthlyRows(
            DashboardSalesmanPortfolioRow rep,
            string entityId,
            string entityCode,
            int periodYear,
            int periodMonth,
            DateTime generatedAt)
        {
            foreach (var (kpiId, value) in BuildTrendKpiValues(rep))
            {
                if (!_kpiRegistry.TryGetMetadata(kpiId, out var metadata) || !metadata.TrendEligible)
                    continue;

                yield return new EntityAnalyticsMonthlyRow
                {
                    EntityType = EntityTypeCode.Salesman,
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
            DashboardSalesmanPortfolioRow rep)
        {
            yield return ("SF-KPI-008", rep.CompletedOmzet);
            yield return ("SF-KPI-009", rep.AchievementPercent);
            yield return ("SF-KPI-010", rep.OpenBalance);
        }

        private static Dictionary<string, HashSet<string>> BuildAttentionIndex(
            DashboardSalesmanAggregateResult aggregate)
        {
            var index = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (aggregate?.AttentionList == null)
                return index;

            foreach (var row in aggregate.AttentionList)
            {
                if (string.IsNullOrWhiteSpace(row.SalesPersonId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                var key = row.SalesPersonId.Trim();
                if (!index.TryGetValue(key, out var signals))
                {
                    signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    index[key] = signals;
                }

                signals.Add(row.SignalKey);
            }

            return index;
        }

        private static Dictionary<string, DashboardSalesmanAttentionRow> BuildAttentionDetailIndex(
            DashboardSalesmanAggregateResult aggregate)
        {
            var index = new Dictionary<string, DashboardSalesmanAttentionRow>(StringComparer.OrdinalIgnoreCase);
            if (aggregate?.AttentionList == null)
                return index;

            foreach (var row in aggregate.AttentionList)
            {
                if (string.IsNullOrWhiteSpace(row.SalesPersonId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                var key = $"{row.SalesPersonId.Trim()}:{row.SignalKey.Trim()}";
                index[key] = row;
            }

            return index;
        }

        private IReadOnlyList<EntityAttentionSignalSnapshot> BuildAttentionSnapshots(
            string entityId,
            string entityCode,
            IReadOnlyDictionary<string, HashSet<string>> attentionById,
            IReadOnlyDictionary<string, DashboardSalesmanAttentionRow> attentionDetailById)
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
            SalesmanEntityAnalyticsProduceInput input)
        {
            var portfolio = input?.SalesmanAggregate?.Portfolio;
            if (portfolio == null || portfolio.Count == 0)
                return Array.Empty<EntityRelationshipSnapshot>();

            var relationshipById = input.RelationshipAggregate?.BySalesPersonId
                ?? new Dictionary<string, DashboardSalesmanRelationshipSalesmanRollup>(StringComparer.OrdinalIgnoreCase);

            var principalsByRep = (input.SalesmanAggregate?.PrincipalAchievement
                ?? new List<DashboardSalesmanPrincipalAchievementRow>())
                .Where(r => !string.IsNullOrWhiteSpace(r.SalesPersonId))
                .GroupBy(r => r.SalesPersonId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.SortOrder).ToList(), StringComparer.OrdinalIgnoreCase);

            var snapshots = new List<EntityRelationshipSnapshot>();

            foreach (var rep in portfolio)
            {
                if (string.IsNullOrWhiteSpace(rep.SalesPersonId))
                    continue;

                var entityId = rep.SalesPersonId.Trim();
                var entityCode = string.IsNullOrWhiteSpace(rep.SalesPersonCode)
                    ? entityId
                    : rep.SalesPersonCode.Trim();

                foreach (var customer in rep.TopCustomers ?? Enumerable.Empty<DashboardSalesmanPortfolioCustomerRow>())
                {
                    if (string.IsNullOrWhiteSpace(customer.CustomerCode))
                        continue;

                    snapshots.Add(new EntityRelationshipSnapshot
                    {
                        SourceEntityId = entityId,
                        SourceEntityCode = entityCode,
                        RelationshipCode = SalesmanRelationshipCatalog.ManagedCustomers,
                        TargetEntityType = EntityTypeCode.Customer,
                        TargetEntityId = customer.CustomerCode.Trim(),
                        TargetEntityCode = customer.CustomerCode.Trim(),
                        TargetDisplayName = customer.CustomerName ?? customer.CustomerCode,
                        MetricValue = customer.MetricValue
                    });
                }

                if (relationshipById.TryGetValue(entityId, out var rollup))
                {
                    foreach (var customer in rollup.TopCustomers)
                    {
                        snapshots.Add(new EntityRelationshipSnapshot
                        {
                            SourceEntityId = entityId,
                            SourceEntityCode = entityCode,
                            RelationshipCode = SalesmanRelationshipCatalog.TopCustomersByOmzet,
                            TargetEntityType = EntityTypeCode.Customer,
                            TargetEntityId = customer.CustomerCode,
                            TargetEntityCode = customer.CustomerCode,
                            TargetDisplayName = customer.CustomerName ?? customer.CustomerCode,
                            MetricValue = customer.MetricValue
                        });
                    }

                    foreach (var item in rollup.TopItems)
                    {
                        snapshots.Add(new EntityRelationshipSnapshot
                        {
                            SourceEntityId = entityId,
                            SourceEntityCode = entityCode,
                            RelationshipCode = SalesmanRelationshipCatalog.TopItemsByOmzet,
                            TargetEntityType = EntityTypeCode.Item,
                            TargetEntityId = item.BrgId,
                            TargetEntityCode = item.BrgCode ?? item.BrgId,
                            TargetDisplayName = item.BrgName,
                            MetricValue = item.MetricValue
                        });
                    }
                }

                if (principalsByRep.TryGetValue(entityId, out var principals))
                {
                    foreach (var principal in principals.Take(10))
                    {
                        if (string.IsNullOrWhiteSpace(principal.SupplierId))
                            continue;

                        snapshots.Add(new EntityRelationshipSnapshot
                        {
                            SourceEntityId = entityId,
                            SourceEntityCode = entityCode,
                            RelationshipCode = SalesmanRelationshipCatalog.TopPrincipalsByOmzet,
                            TargetEntityType = EntityTypeCode.Supplier,
                            TargetEntityId = principal.SupplierId.Trim(),
                            TargetEntityCode = principal.SupplierId.Trim(),
                            TargetDisplayName = principal.SupplierName ?? principal.SupplierId,
                            MetricValue = principal.CompletedOmzet
                        });
                    }
                }
            }

            return snapshots;
        }

        private static IEnumerable<EntityAnalyticsCurrentRow> BuildSalesmanRows(
            DashboardSalesmanPortfolioRow rep,
            string entityId,
            string entityCode,
            DateTime generatedAt,
            IReadOnlyDictionary<string, HashSet<string>> attentionById)
        {
            var rows = new List<EntityAnalyticsCurrentRow>();

            foreach (var (kpiId, value) in BuildTrendKpiValues(rep))
            {
                rows.Add(CreateRow(entityId, entityCode, kpiId, value, null, generatedAt));
            }

            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, rep.SalesPersonName, generatedAt));
            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, rep.IsActive ? 1m : 0m, null, generatedAt));

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Wilayah, rep.WilayahName, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Segment, rep.SegmentName, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.AchievementBand, rep.AchievementBand, generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveMtd, rep.IsActive ? "Yes" : "No", generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.CustomerCount, rep.CustomerCount.ToString(CultureInfo.InvariantCulture), generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.DormantCustomerCount, rep.DormantCustomerCount.ToString(CultureInfo.InvariantCulture), generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.OverdueBalance, rep.OverdueBalance.ToString(CultureInfo.InvariantCulture), generatedAt);
            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.CustomerEngagement, rep.ActiveCustomerCount.ToString(CultureInfo.InvariantCulture), generatedAt);

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
                EntityType = EntityTypeCode.Salesman,
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
