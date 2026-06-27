using System;
using System.Diagnostics;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Application;
using btr.nuna.Domain;
using Microsoft.Extensions.Options;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public interface IRefreshDashboardInventoryRiskSnapshotWorker
        : INunaServiceVoid<RefreshDashboardInventoryRiskSnapshotRequest>
    {
    }

    public class RefreshDashboardInventoryRiskSnapshotWorker : IRefreshDashboardInventoryRiskSnapshotWorker
    {
        private const string Domain = "InventoryRisk";
        private const int MaxErrorMessageLength = 500;

        private readonly IStokBalanceViewDal _stokBalanceViewDal;
        private readonly IBrgLastFakturDal _brgLastFakturDal;
        private readonly IBrgConsumptionDal _brgConsumptionDal;
        private readonly IBrgWarehouseConsumptionDal _brgWarehouseConsumptionDal;
        private readonly IWarehouseDal _warehouseDal;
        private readonly IDashboardPurchasingManagementSnapshotDal _purchasingMgmtSnapshotDal;
        private readonly DashboardInventoryRiskAggregator _aggregator;
        private readonly DashboardInventoryForecastAggregator _forecastAggregator;
        private readonly DashboardInventoryOptimizationAggregator _optimizationAggregator;
        private readonly ISalesmanMtdItemRollupDal _salesmanMtdItemRollupDal;
        private readonly DashboardItemPortfolioBuilder _itemPortfolioBuilder;
        private readonly DashboardItemRelationshipAggregator _itemRelationshipAggregator;
        private readonly EntityAnalyticsProducerOrchestrator _entityAnalyticsOrchestrator;
        private readonly ICustomerDal _customerDal;
        private readonly IDashboardInventoryRiskSnapshotDal _snapshotDal;
        private readonly IDashboardSnapshotRefreshLogDal _refreshLogDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;
        private readonly DashboardSnapshotOptions _options;

        public RefreshDashboardInventoryRiskSnapshotWorker(
            IStokBalanceViewDal stokBalanceViewDal,
            IBrgLastFakturDal brgLastFakturDal,
            IBrgConsumptionDal brgConsumptionDal,
            IBrgWarehouseConsumptionDal brgWarehouseConsumptionDal,
            IWarehouseDal warehouseDal,
            IDashboardPurchasingManagementSnapshotDal purchasingMgmtSnapshotDal,
            DashboardInventoryRiskAggregator aggregator,
            DashboardInventoryForecastAggregator forecastAggregator,
            DashboardInventoryOptimizationAggregator optimizationAggregator,
            ISalesmanMtdItemRollupDal salesmanMtdItemRollupDal,
            DashboardItemPortfolioBuilder itemPortfolioBuilder,
            DashboardItemRelationshipAggregator itemRelationshipAggregator,
            EntityAnalyticsProducerOrchestrator entityAnalyticsOrchestrator,
            ICustomerDal customerDal,
            IDashboardInventoryRiskSnapshotDal snapshotDal,
            IDashboardSnapshotRefreshLogDal refreshLogDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider,
            DashboardSnapshotOptions options)
        {
            _stokBalanceViewDal = stokBalanceViewDal;
            _brgLastFakturDal = brgLastFakturDal;
            _brgConsumptionDal = brgConsumptionDal;
            _brgWarehouseConsumptionDal = brgWarehouseConsumptionDal;
            _warehouseDal = warehouseDal;
            _purchasingMgmtSnapshotDal = purchasingMgmtSnapshotDal;
            _aggregator = aggregator;
            _forecastAggregator = forecastAggregator;
            _optimizationAggregator = optimizationAggregator;
            _salesmanMtdItemRollupDal = salesmanMtdItemRollupDal;
            _itemPortfolioBuilder = itemPortfolioBuilder;
            _itemRelationshipAggregator = itemRelationshipAggregator;
            _entityAnalyticsOrchestrator = entityAnalyticsOrchestrator;
            _customerDal = customerDal;
            _snapshotDal = snapshotDal;
            _refreshLogDal = refreshLogDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public void Execute(RefreshDashboardInventoryRiskSnapshotRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var refreshLogId = Ulid.NewUlid().ToString();
            var startedAt = _tglJamDal.Now;

            WorkerProgressScope.Current.StepStarted($"{Domain}:Initialize", "Initialize refresh log");
            _refreshLogDal.InsertRunning(new DashboardSnapshotRefreshLogModel
            {
                RefreshLogId = refreshLogId,
                Domain = Domain,
                StartedAt = startedAt,
                Status = "Running",
                TriggeredBy = request.TriggeredBy ?? "Scheduler"
            });
            WorkerProgressScope.Current.StepCompleted($"{Domain}:Initialize");

            try
            {
                var generatedAt = _tglJamDal.Now;
                var today = _businessDateProvider.Today;
                const int loadSteps = 6;

                WorkerProgressScope.Current.StepStarted($"{Domain}:Load", "Load source data");
                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load stock balances", 1, loadSteps);
                var rows = _stokBalanceViewDal.ListData();
                var rowCount = rows == null ? 0 : System.Linq.Enumerable.Count(rows);

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load last faktur by item", 2, loadSteps);
                var lastFakturRows = _brgLastFakturDal.ListLastFakturByBrg()?.ToList()
                    ?? new System.Collections.Generic.List<BrgLastFakturDto>();

                var window30Start = today.Date.AddDays(-(InventoryForecastPolicy.AdcWindow30Days - 1));
                var window90Start = today.Date.AddDays(-(InventoryForecastPolicy.AdcWindow90Days - 1));

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load item consumption", 3, loadSteps);
                var consumptionRows = _brgConsumptionDal.ListConsumptionByBrg(window30Start, window90Start, today)?.ToList()
                    ?? new System.Collections.Generic.List<BrgConsumptionDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load daily company consumption", 4, loadSteps);
                var dailyConsumptionRows = _brgConsumptionDal.ListDailyCompanyConsumption(window30Start, today)?.ToList()
                    ?? new System.Collections.Generic.List<DailyCompanyConsumptionDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load warehouse consumption", 5, loadSteps);
                var warehouseConsumptionRows = _brgWarehouseConsumptionDal.ListConsumptionByBrgWarehouse(window30Start, today)?.ToList()
                    ?? new System.Collections.Generic.List<BrgWarehouseConsumptionDto>();

                WorkerProgressScope.Current.ReportPhaseProgress($"{Domain}: Load purchasing management snapshot", 6, loadSteps);
                var purchasingMgmt = _purchasingMgmtSnapshotDal.GetCurrent();
                var warehouses = _warehouseDal.ListAllForPortal()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.InventoryContext.WarehouseAgg.WarehouseModel>();
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Load", new WorkerProgressStepInfo
                {
                    RecordCount = rowCount + lastFakturRows.Count + consumptionRows.Count + dailyConsumptionRows.Count + warehouseConsumptionRows.Count
                });

                WorkerProgressScope.Current.StepStarted($"{Domain}:Aggregate", "Aggregate metrics");
                var aggregate = _aggregator.Aggregate(rows, lastFakturRows, today, generatedAt);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Aggregate");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateForecast", "Aggregate inventory forecast");
                var forecast = _forecastAggregator.Aggregate(
                    rows,
                    lastFakturRows,
                    consumptionRows,
                    dailyConsumptionRows,
                    aggregate,
                    today,
                    generatedAt,
                    _options.InventoryForecastPlanningHorizonDays,
                    _options.InventoryForecastDefaultLeadTimeDays,
                    _options.InventoryForecastCoverageDays,
                    _options.InventoryForecastOverstockDosDays,
                    _options.InventoryForecastMinDosHealthy);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateForecast");

                WorkerProgressScope.Current.StepStarted($"{Domain}:AggregateOptimization", "Aggregate inventory optimization");
                var optimization = _optimizationAggregator.Aggregate(
                    forecast.ItemContexts,
                    rows,
                    warehouseConsumptionRows,
                    warehouses,
                    aggregate,
                    purchasingMgmt,
                    forecast,
                    today,
                    generatedAt,
                    _options);
                WorkerProgressScope.Current.StepCompleted($"{Domain}:AggregateOptimization");

                var periode = CurrentMonthPeriode(today);
                var itemRollupRows = _salesmanMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<SalesmanMtdItemRollupDto>();
                var itemGroups = DashboardInventoryItemGroupBuilder.BuildItemGroups(rows);
                var portfolio = _itemPortfolioBuilder.Build(
                    itemGroups,
                    forecast.ItemContexts,
                    aggregate,
                    itemRollupRows,
                    lastFakturRows,
                    today);
                var relationshipAggregate = _itemRelationshipAggregator.Aggregate(itemRollupRows, today, generatedAt);

                WorkerProgressScope.Current.StepStarted($"{Domain}:Save", "Save snapshot");
                var customers = _customerDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.CustomerAgg.CustomerModel>();
                using (var trans = TransHelper.NewScope())
                {
                    _snapshotDal.ReplaceCurrent(aggregate, forecast, optimization, refreshLogId);

                    WorkerProgressScope.Current.StepStarted($"{Domain}:EntityAnalytics", "Produce entity analytics L0+L1+L2+L3+L4+L5 snapshot");
                    _entityAnalyticsOrchestrator.ProduceForDomain(Domain, new EntityAnalyticsProduceContext
                    {
                        RefreshLogId = refreshLogId,
                        GeneratedAt = generatedAt,
                        BusinessDate = today,
                        CustomerIdentityLookup = EntityAnalyticsCustomerIdentityResolver.BuildLookup(customers),
                        DomainInput = new ItemEntityAnalyticsProduceInput
                        {
                            RiskAggregate = aggregate,
                            ForecastAggregate = forecast,
                            RelationshipAggregate = relationshipAggregate,
                            Portfolio = portfolio
                        }
                    });
                    WorkerProgressScope.Current.StepCompleted($"{Domain}:EntityAnalytics");

                    trans.Complete();
                }
                WorkerProgressScope.Current.StepCompleted($"{Domain}:Save");

                sw.Stop();
                _refreshLogDal.MarkSuccess(refreshLogId, (int)sw.ElapsedMilliseconds);

                request.Result = new RefreshDashboardInventoryRiskSnapshotResult
                {
                    RefreshLogId = refreshLogId,
                    DurationMs = (int)sw.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                var message = ex.Message ?? ex.GetType().Name;
                if (message.Length > MaxErrorMessageLength)
                    message = message.Substring(0, MaxErrorMessageLength);

                _refreshLogDal.MarkFailed(refreshLogId, (int)sw.ElapsedMilliseconds, message);
                WorkerProgressScope.Current.StepFailed($"{Domain}:Execute", message);
                throw;
            }
        }

        private static Periode CurrentMonthPeriode(DateTime today)
        {
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return new Periode(monthStart, monthEnd);
        }
    }
}
