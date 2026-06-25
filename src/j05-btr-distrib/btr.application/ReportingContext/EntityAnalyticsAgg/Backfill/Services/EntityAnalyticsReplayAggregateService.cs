using System;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services
{
    public class EntityAnalyticsReplayAggregateService : IEntityAnalyticsReplayAggregateService
    {
        private readonly DashboardCustomerAggregator _customerAggregator;
        private readonly DashboardCustomerRiskForecastAggregator _customerForecastAggregator;
        private readonly DashboardCollectionOptimizationAggregator _collectionOptimizationAggregator;
        private readonly DashboardCustomerPortfolioAggregator _customerPortfolioAggregator;
        private readonly DashboardCustomerRelationshipAggregator _customerRelationshipAggregator;
        private readonly DashboardSalesmanAggregator _salesmanAggregator;
        private readonly DashboardSalesmanRelationshipAggregator _salesmanRelationshipAggregator;
        private readonly DashboardPurchasingManagementAggregator _supplierAggregator;
        private readonly DashboardSupplierRelationshipAggregator _supplierRelationshipAggregator;
        private readonly DashboardInventoryRiskAggregator _itemRiskAggregator;
        private readonly DashboardInventoryForecastAggregator _itemForecastAggregator;
        private readonly DashboardInventoryOptimizationAggregator _itemOptimizationAggregator;
        private readonly DashboardItemPortfolioBuilder _itemPortfolioBuilder;
        private readonly DashboardItemRelationshipAggregator _itemRelationshipAggregator;
        private readonly DashboardSnapshotOptions _options;

        public EntityAnalyticsReplayAggregateService(
            DashboardCustomerAggregator customerAggregator,
            DashboardCustomerRiskForecastAggregator customerForecastAggregator,
            DashboardCollectionOptimizationAggregator collectionOptimizationAggregator,
            DashboardCustomerPortfolioAggregator customerPortfolioAggregator,
            DashboardCustomerRelationshipAggregator customerRelationshipAggregator,
            DashboardSalesmanAggregator salesmanAggregator,
            DashboardSalesmanRelationshipAggregator salesmanRelationshipAggregator,
            DashboardPurchasingManagementAggregator supplierAggregator,
            DashboardSupplierRelationshipAggregator supplierRelationshipAggregator,
            DashboardInventoryRiskAggregator itemRiskAggregator,
            DashboardInventoryForecastAggregator itemForecastAggregator,
            DashboardInventoryOptimizationAggregator itemOptimizationAggregator,
            DashboardItemPortfolioBuilder itemPortfolioBuilder,
            DashboardItemRelationshipAggregator itemRelationshipAggregator,
            DashboardSnapshotOptions options)
        {
            _customerAggregator = customerAggregator;
            _customerForecastAggregator = customerForecastAggregator;
            _collectionOptimizationAggregator = collectionOptimizationAggregator;
            _customerPortfolioAggregator = customerPortfolioAggregator;
            _customerRelationshipAggregator = customerRelationshipAggregator;
            _salesmanAggregator = salesmanAggregator;
            _salesmanRelationshipAggregator = salesmanRelationshipAggregator;
            _supplierAggregator = supplierAggregator;
            _supplierRelationshipAggregator = supplierRelationshipAggregator;
            _itemRiskAggregator = itemRiskAggregator;
            _itemForecastAggregator = itemForecastAggregator;
            _itemOptimizationAggregator = itemOptimizationAggregator;
            _itemPortfolioBuilder = itemPortfolioBuilder;
            _itemRelationshipAggregator = itemRelationshipAggregator;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public EntityAnalyticsReplayAggregateResult Aggregate(
            EntityAnalyticsReplayContext replayContext,
            object bundle,
            DateTime generatedAt)
        {
            if (replayContext is null)
                throw new ArgumentNullException(nameof(replayContext));
            if (bundle is null)
                throw new ArgumentNullException(nameof(bundle));

            var periodEnd = replayContext.PeriodEnd.Date;
            var periode = new Periode(replayContext.PeriodStart, periodEnd);
            var stepPrefix = $"Backfill:{replayContext.EntityTypeCode}:{replayContext.PeriodYear:D4}-{replayContext.PeriodMonth:D2}";

            object produceInput;
            switch (replayContext.EntityTypeCode)
            {
                case EntityTypeCode.Customer:
                    produceInput = AggregateCustomer((CustomerReplayDataBundle)bundle, periode, periodEnd, generatedAt, stepPrefix);
                    break;
                case EntityTypeCode.Salesman:
                    produceInput = AggregateSalesman((SalesmanReplayDataBundle)bundle, periode, periodEnd, generatedAt, stepPrefix);
                    break;
                case EntityTypeCode.Supplier:
                    produceInput = AggregateSupplier((SupplierReplayDataBundle)bundle, periode, periodEnd, generatedAt, stepPrefix);
                    break;
                case EntityTypeCode.Item:
                    produceInput = AggregateItem((ItemReplayDataBundle)bundle, periode, periodEnd, generatedAt, stepPrefix);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported entity type '{replayContext.EntityTypeCode}' for replay aggregation.");
            }

            return new EntityAnalyticsReplayAggregateResult
            {
                EntityType = replayContext.EntityTypeCode,
                ProduceInput = produceInput,
                EntityCount = EntityAnalyticsReplayEntityCountHelper.CountEntities(replayContext.EntityTypeCode, produceInput),
                RowCounts = EntityAnalyticsReplayEntityCountHelper.BuildRowCounts(replayContext.EntityTypeCode, bundle)
            };
        }

        private CustomerEntityAnalyticsProduceInput AggregateCustomer(
            CustomerReplayDataBundle bundle,
            Periode periode,
            DateTime periodEnd,
            DateTime generatedAt,
            string stepPrefix)
        {
            var forecastOptions = CustomerRiskForecastOptions.FromDashboardOptions(_options);
            var optimizationOptions = CollectionOptimizationOptions.FromDashboardOptions(_options);
            var portfolioOptions = CustomerPortfolioOptions.FromDashboardOptions(_options);

            var aggregate = _customerAggregator.Aggregate(
                bundle.FakturRows,
                bundle.PiutangRows,
                bundle.LastFakturRows,
                bundle.Customers,
                periode,
                periodEnd,
                generatedAt);

            var forecastAggregate = _customerForecastAggregator.Aggregate(
                bundle.PiutangRows,
                bundle.OmzetHistoryRows,
                bundle.LastFakturRows,
                bundle.Customers,
                bundle.PelunasanSummaryRows,
                bundle.PaymentBehaviorRows,
                bundle.FakturRows,
                periodEnd,
                generatedAt,
                forecastOptions);

            var optimizationAggregate = _collectionOptimizationAggregator.Aggregate(
                forecastAggregate.Contexts,
                forecastAggregate,
                collectionSnapshot: null,
                bundle.PiutangRows,
                bundle.FakturRows,
                bundle.Customers,
                periodEnd,
                generatedAt,
                optimizationOptions);

            var portfolioAggregate = _customerPortfolioAggregator.Aggregate(
                aggregate,
                forecastAggregate.Contexts,
                forecastAggregate,
                optimizationAggregate.ContextsByKey,
                salesmanSnapshot: null,
                bundle.Customers,
                bundle.LastFakturWithSalesman,
                bundle.FirstFakturRows,
                bundle.FrequencyRows,
                bundle.FakturRows,
                bundle.PiutangRows,
                periodEnd,
                generatedAt,
                portfolioOptions);

            var relationshipAggregate = _customerRelationshipAggregator.Aggregate(
                bundle.ItemRollupRows,
                periodEnd,
                generatedAt);

            return new CustomerEntityAnalyticsProduceInput
            {
                CustomerAggregate = aggregate,
                ForecastAggregate = forecastAggregate,
                PortfolioAggregate = portfolioAggregate,
                SalesmanSnapshot = null,
                RelationshipAggregate = relationshipAggregate
            };
        }

        private SalesmanEntityAnalyticsProduceInput AggregateSalesman(
            SalesmanReplayDataBundle bundle,
            Periode periode,
            DateTime periodEnd,
            DateTime generatedAt,
            string stepPrefix)
        {
            var aggregate = _salesmanAggregator.Aggregate(
                bundle.FakturRows,
                bundle.PiutangRows,
                bundle.LastFakturRows,
                bundle.Salespeople,
                bundle.Targets,
                periode,
                periodEnd,
                generatedAt,
                _options.SalesmanExposureTopPercent,
                bundle.PrincipalTargets,
                bundle.PrincipalOmzet);

            var relationshipAggregate = _salesmanRelationshipAggregator.Aggregate(
                bundle.ItemRollupRows,
                periodEnd,
                generatedAt);

            return new SalesmanEntityAnalyticsProduceInput
            {
                SalesmanAggregate = aggregate,
                RelationshipAggregate = relationshipAggregate
            };
        }

        private SupplierEntityAnalyticsProduceInput AggregateSupplier(
            SupplierReplayDataBundle bundle,
            Periode periode,
            DateTime periodEnd,
            DateTime generatedAt,
            string stepPrefix)
        {
            var aggregate = _supplierAggregator.Aggregate(
                bundle.InvoiceRows,
                purchasingSnapshot: null,
                inventorySnapshot: null,
                inventoryRiskSnapshot: null,
                periode,
                periodEnd,
                generatedAt,
                _options.PurchasingQualifiedBacklogDays,
                bundle.Suppliers,
                bundle.ItemRollupRows,
                bundle.CatalogCounts);

            var relationshipAggregate = _supplierRelationshipAggregator.Aggregate(
                bundle.ItemRollupRows,
                periodEnd,
                generatedAt);

            return new SupplierEntityAnalyticsProduceInput
            {
                ManagementAggregate = aggregate,
                RelationshipAggregate = relationshipAggregate
            };
        }

        private ItemEntityAnalyticsProduceInput AggregateItem(
            ItemReplayDataBundle bundle,
            Periode periode,
            DateTime periodEnd,
            DateTime generatedAt,
            string stepPrefix)
        {
            var aggregate = _itemRiskAggregator.Aggregate(
                bundle.StokBalanceRows,
                bundle.LastFakturRows,
                periodEnd,
                generatedAt);

            var forecast = _itemForecastAggregator.Aggregate(
                bundle.StokBalanceRows,
                bundle.LastFakturRows,
                bundle.ConsumptionRows,
                bundle.DailyConsumptionRows,
                aggregate,
                periodEnd,
                generatedAt,
                _options.InventoryForecastPlanningHorizonDays,
                _options.InventoryForecastDefaultLeadTimeDays,
                _options.InventoryForecastCoverageDays,
                _options.InventoryForecastOverstockDosDays,
                _options.InventoryForecastMinDosHealthy);

            var optimization = _itemOptimizationAggregator.Aggregate(
                forecast.ItemContexts,
                bundle.StokBalanceRows,
                bundle.WarehouseConsumptionRows,
                bundle.Warehouses,
                aggregate,
                purchasingMgmt: null,
                forecast,
                periodEnd,
                generatedAt,
                _options);

            var itemGroups = DashboardInventoryItemGroupBuilder.BuildItemGroups(bundle.StokBalanceRows);
            var portfolio = _itemPortfolioBuilder.Build(
                itemGroups,
                forecast.ItemContexts,
                aggregate,
                bundle.ItemRollupRows,
                bundle.LastFakturRows,
                periodEnd);

            var relationshipAggregate = _itemRelationshipAggregator.Aggregate(
                bundle.ItemRollupRows,
                periodEnd,
                generatedAt);

            return new ItemEntityAnalyticsProduceInput
            {
                RiskAggregate = aggregate,
                ForecastAggregate = forecast,
                RelationshipAggregate = relationshipAggregate,
                Portfolio = portfolio
            };
        }
    }
}
