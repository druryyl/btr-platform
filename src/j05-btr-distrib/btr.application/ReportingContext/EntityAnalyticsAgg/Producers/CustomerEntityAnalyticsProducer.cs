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

    public class CustomerEntityAnalyticsProducer : IEntityAnalyticsProducer

    {

        public string EntityType => EntityTypeCode.Customer;



        public string WorkerDomain => "Customer";



        private readonly IEntityAnalyticsRepository _repository;

        private readonly IKpiRegistry _kpiRegistry;

        private readonly IEntityAnalyticsMonthCloseService _monthCloseService;

        private readonly IEntityRankingEngine _rankingEngine;

        private readonly IEntityAttentionEngine _attentionEngine;

        private readonly IEntityRelationshipEngine _relationshipEngine;

        private readonly IEntityRadarEngine _radarEngine;

        private readonly IAttentionSignalRegistry _attentionSignals;



        public CustomerEntityAnalyticsProducer(

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



            var input = context.DomainInput as CustomerEntityAnalyticsProduceInput;

            var customers = input?.PortfolioAggregate?.Customers;

            if (customers == null || customers.Count == 0)
            {
                EntityAnalyticsProducerReplaySupport.PersistL0(
                    _repository, context, EntityTypeCode.Customer, Array.Empty<EntityAnalyticsCurrentRow>());
                return;
            }



            var attentionByCode = BuildAttentionIndex(input.CustomerAggregate);

            var attentionDetailByCode = BuildAttentionDetailIndex(input.CustomerAggregate);

            var rows = new List<EntityAnalyticsCurrentRow>();

            var monthlyRows = new List<EntityAnalyticsMonthlyRow>();

            var signalsByEntity = new Dictionary<string, IReadOnlyList<EntityAttentionSignalSnapshot>>(

                StringComparer.OrdinalIgnoreCase);

            var (periodYear, periodMonth) = EntityAnalyticsProducerReplaySupport.ResolvePeriod(context);



            foreach (var customer in customers)

            {

                if (string.IsNullOrWhiteSpace(customer.CustomerCode))

                    continue;



                var entityId = customer.CustomerCode.Trim();

                var generatedAt = context.GeneratedAt;



                rows.AddRange(BuildCustomerRows(customer, entityId, generatedAt, attentionByCode));

                monthlyRows.AddRange(BuildMonthlyRows(customer, entityId, periodYear, periodMonth, generatedAt));

                signalsByEntity[entityId] = BuildAttentionSnapshots(

                    entityId,

                    entityId,

                    attentionByCode,

                    attentionDetailByCode);

            }



            EntityAnalyticsProducerReplaySupport.PersistL0(_repository, context, EntityTypeCode.Customer, rows);

            EntityAnalyticsProducerReplaySupport.PersistL1(
                _repository, _monthCloseService, context, EntityTypeCode.Customer, monthlyRows);

            _rankingEngine.ComputeAndPersistRanks(
                EntityTypeCode.Customer,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _attentionEngine.DiffAndPersistSignals(
                EntityTypeCode.Customer,
                periodYear,
                periodMonth,
                signalsByEntity,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _relationshipEngine.PersistRollups(
                EntityTypeCode.Customer,
                periodYear,
                periodMonth,
                BuildRelationshipSnapshots(input),
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

            _radarEngine.ComputeAndPersistScores(
                EntityTypeCode.Customer,
                periodYear,
                periodMonth,
                context.RefreshLogId,
                context.GeneratedAt,
                context.Replay);

        }



        private IEnumerable<EntityAnalyticsMonthlyRow> BuildMonthlyRows(

            DashboardCustomerPortfolioCustomerRow customer,

            string entityId,

            int periodYear,

            int periodMonth,

            DateTime generatedAt)

        {

            var entityCode = customer.CustomerCode;



            foreach (var (kpiId, value) in BuildTrendKpiValues(customer))

            {

                if (!_kpiRegistry.TryGetMetadata(kpiId, out var metadata) || !metadata.TrendEligible)

                    continue;



                yield return new EntityAnalyticsMonthlyRow

                {

                    EntityType = EntityTypeCode.Customer,

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

            DashboardCustomerPortfolioCustomerRow customer)

        {

            yield return ("CU-KPI-009", customer.MtdOmzet);

            yield return ("CU-KPI-010", customer.OpenBalance);

            yield return ("FI-KPI-013", customer.OverdueBalance);

        }



        private static Dictionary<string, HashSet<string>> BuildAttentionIndex(

            DashboardCustomerAggregateResult customerAggregate)

        {

            var index = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            if (customerAggregate?.AttentionList == null)

                return index;



            foreach (var row in customerAggregate.AttentionList)

            {

                if (string.IsNullOrWhiteSpace(row.CustomerCode) || string.IsNullOrWhiteSpace(row.SignalKey))

                    continue;



                if (!index.TryGetValue(row.CustomerCode, out var signals))

                {

                    signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    index[row.CustomerCode] = signals;

                }



                signals.Add(row.SignalKey);

            }



            return index;

        }



        private static Dictionary<string, DashboardCustomerAttentionRow> BuildAttentionDetailIndex(

            DashboardCustomerAggregateResult customerAggregate)

        {

            var index = new Dictionary<string, DashboardCustomerAttentionRow>(StringComparer.OrdinalIgnoreCase);

            if (customerAggregate?.AttentionList == null)

                return index;



            foreach (var row in customerAggregate.AttentionList)

            {

                if (string.IsNullOrWhiteSpace(row.CustomerCode) || string.IsNullOrWhiteSpace(row.SignalKey))

                    continue;



                var key = $"{row.CustomerCode.Trim()}:{row.SignalKey.Trim()}";

                index[key] = row;

            }



            return index;

        }



        private IReadOnlyList<EntityAttentionSignalSnapshot> BuildAttentionSnapshots(

            string entityId,

            string entityCode,

            IReadOnlyDictionary<string, HashSet<string>> attentionByCode,

            IReadOnlyDictionary<string, DashboardCustomerAttentionRow> attentionDetailByCode)

        {

            if (!attentionByCode.TryGetValue(entityCode, out var signalCodes) || signalCodes.Count == 0)

                return Array.Empty<EntityAttentionSignalSnapshot>();



            var snapshots = new List<EntityAttentionSignalSnapshot>();

            foreach (var signalCode in signalCodes.OrderBy(s => s, StringComparer.OrdinalIgnoreCase))

            {

                attentionDetailByCode.TryGetValue($"{entityCode}:{signalCode}", out var detail);

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

            CustomerEntityAnalyticsProduceInput input)

        {

            var customers = input?.PortfolioAggregate?.Customers;

            if (customers == null || customers.Count == 0)

                return Array.Empty<EntityRelationshipSnapshot>();



            var forecastByCode = BuildForecastIndex(input.ForecastAggregate);

            var salesmanCodeById = BuildSalesmanCodeIndex(input.SalesmanSnapshot);

            var relationshipByCode = input.RelationshipAggregate?.ByCustomerCode

                ?? new Dictionary<string, DashboardCustomerRelationshipCustomerRollup>(StringComparer.OrdinalIgnoreCase);



            var snapshots = new List<EntityRelationshipSnapshot>();

            foreach (var customer in customers)

            {

                if (string.IsNullOrWhiteSpace(customer.CustomerCode))

                    continue;



                var entityId = customer.CustomerCode.Trim();

                var entityCode = entityId;



                forecastByCode.TryGetValue(entityCode, out var forecast);

                if (forecast != null && !string.IsNullOrWhiteSpace(forecast.SalesPersonId))

                {

                    var salesPersonCode = salesmanCodeById.TryGetValue(forecast.SalesPersonId, out var code)

                        ? code

                        : forecast.SalesPersonId;



                    snapshots.Add(new EntityRelationshipSnapshot

                    {

                        SourceEntityId = entityId,

                        SourceEntityCode = entityCode,

                        RelationshipCode = CustomerRelationshipCatalog.AssignedSalesman,

                        TargetEntityType = EntityTypeCode.Salesman,

                        TargetEntityId = forecast.SalesPersonId.Trim(),

                        TargetEntityCode = salesPersonCode,

                        TargetDisplayName = forecast.SalesPersonName?.Trim()

                            ?? customer.SalesPersonName?.Trim()

                            ?? salesPersonCode

                    });

                }



                if (relationshipByCode.TryGetValue(entityCode, out var rollup))

                {

                    foreach (var item in rollup.TopItems)

                    {

                        snapshots.Add(new EntityRelationshipSnapshot

                        {

                            SourceEntityId = entityId,

                            SourceEntityCode = entityCode,

                            RelationshipCode = CustomerRelationshipCatalog.TopItemsByOmzet,

                            TargetEntityType = EntityTypeCode.Item,

                            TargetEntityId = item.BrgId,

                            TargetEntityCode = item.BrgCode ?? item.BrgId,

                            TargetDisplayName = item.BrgName,

                            MetricValue = item.MetricValue

                        });

                    }



                    foreach (var principal in rollup.TopPrincipals)

                    {

                        snapshots.Add(new EntityRelationshipSnapshot

                        {

                            SourceEntityId = entityId,

                            SourceEntityCode = entityCode,

                            RelationshipCode = CustomerRelationshipCatalog.TopPrincipalsByOmzet,

                            TargetEntityType = EntityTypeCode.Supplier,

                            TargetEntityId = principal.SupplierId,

                            TargetEntityCode = principal.SupplierCode ?? principal.SupplierId,

                            TargetDisplayName = principal.SupplierName,

                            MetricValue = principal.MetricValue

                        });

                    }

                }

            }



            return snapshots;

        }



        private static Dictionary<string, CustomerRiskForecastContext> BuildForecastIndex(

            DashboardCustomerRiskForecastAggregateResult forecastAggregate)

        {

            var index = new Dictionary<string, CustomerRiskForecastContext>(StringComparer.OrdinalIgnoreCase);

            if (forecastAggregate?.Contexts == null)

                return index;



            foreach (var context in forecastAggregate.Contexts)

            {

                if (string.IsNullOrWhiteSpace(context.CustomerCode))

                    continue;



                index[context.CustomerCode.Trim()] = context;

            }



            return index;

        }



        private static Dictionary<string, string> BuildSalesmanCodeIndex(

            DashboardSalesmanAggregateResult salesmanSnapshot)

        {

            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (salesmanSnapshot == null)

                return index;



            void Add(string salesPersonId, string salesPersonCode)

            {

                if (string.IsNullOrWhiteSpace(salesPersonId) || string.IsNullOrWhiteSpace(salesPersonCode))

                    return;



                index[salesPersonId.Trim()] = salesPersonCode.Trim();

            }



            foreach (var row in salesmanSnapshot.TopOmzet ?? Enumerable.Empty<DashboardSalesmanTopOmzetRow>())

                Add(row.SalesPersonId, row.SalesPersonCode);



            foreach (var row in salesmanSnapshot.TopPiutang ?? Enumerable.Empty<DashboardSalesmanTopPiutangRow>())

                Add(row.SalesPersonId, row.SalesPersonCode);



            foreach (var row in salesmanSnapshot.PrincipalAchievement ?? Enumerable.Empty<DashboardSalesmanPrincipalAchievementRow>())

                Add(row.SalesPersonId, row.SalesPersonCode);



            return index;

        }



        private static IEnumerable<EntityAnalyticsCurrentRow> BuildCustomerRows(

            DashboardCustomerPortfolioCustomerRow customer,

            string entityId,

            DateTime generatedAt,

            IReadOnlyDictionary<string, HashSet<string>> attentionByCode)

        {

            var entityCode = customer.CustomerCode;

            var rows = new List<EntityAnalyticsCurrentRow>();



            foreach (var (kpiId, value) in BuildTrendKpiValues(customer))

            {

                rows.Add(CreateRow(entityId, entityCode, kpiId, value, null, generatedAt));

            }



            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.DisplayName, null, customer.CustomerName, generatedAt));

            rows.Add(CreateMetaRow(entityId, entityCode, EntityAnalyticsMetaKpiIds.IsActive, customer.IsActiveMtd ? 1m : 0m, null, generatedAt));



            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Wilayah, customer.WilayahName, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Klasifikasi, customer.Klasifikasi, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Salesman, customer.SalesPersonName, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Lifecycle,

                customer.LifecycleLabel ?? customer.LifecycleStage, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.Tier,

                customer.TierLabel ?? customer.PortfolioTier, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.PortfolioAction,

                customer.PrimaryActionLabel ?? customer.PrimaryActionKey, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.M29Category, customer.M29Category, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.M29PrimarySignal, customer.M29PrimarySignalKey, generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.LastPurchaseDate,

                customer.LastPurchaseDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.FakturCount6Mo,

                customer.FakturCount6Mo.ToString(CultureInfo.InvariantCulture), generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.ActiveMtd,

                customer.IsActiveMtd ? "Yes" : "No", generatedAt);

            AddDimension(rows, entityId, entityCode, EntityAnalyticsMetaKpiIds.PortfolioPriorityScore,

                customer.PortfolioPriorityScore.ToString(CultureInfo.InvariantCulture), generatedAt);



            if (attentionByCode.TryGetValue(entityCode, out var signals) && signals.Count > 0)

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

                EntityType = EntityTypeCode.Customer,

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


