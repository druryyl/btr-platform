using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Services;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Producers;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;
using btr.infrastructure.FinanceContext.PiutangAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.ReportingContext.DashboardSnapshotAgg;
using btr.infrastructure.SalesContext.CustomerAgg;
using btr.infrastructure.SalesContext.FakturInfoAgg;

namespace btr.test.ReportingContext
{
    public class CustomerReplayDataLoaderIntegrationTest
    {
        [Fact]
        public void LoadAndAggregate_HistoricalMonth_ProducesCustomerPortfolioRows()
        {
            var databaseOptions = new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            };
            if (!IsTestSchemaAvailable(databaseOptions))
                return;
            var options = Options.Create(databaseOptions);
            var period = ResolveHistoricalMonth();

            var loader = new CustomerReplayDataLoader(
                new FakturViewDal(options),
                new CustomerLastFakturDal(options),
                new CustomerFirstFakturDal(options),
                new CustomerPurchaseFrequencyDal(options),
                new PiutangOpenBalanceDal(options),
                new CustomerDal(options),
                new CustomerOmzetHistoryDal(options),
                new CustomerPelunasanSummaryDal(options),
                new CustomerPaymentBehaviorDal(options),
                new CustomerMtdItemRollupDal(options),
                new DashboardSnapshotOptions());

            var aggregateService = new EntityAnalyticsReplayAggregateService(
                new DashboardCustomerAggregator(),
                new DashboardCustomerRiskForecastAggregator(),
                new DashboardCollectionOptimizationAggregator(),
                new DashboardCustomerPortfolioAggregator(),
                new DashboardCustomerRelationshipAggregator(),
                new DashboardSalesmanAggregator(),
                new DashboardSalesmanRelationshipAggregator(),
                new DashboardPurchasingManagementAggregator(),
                new DashboardSupplierRelationshipAggregator(),
                new DashboardInventoryRiskAggregator(),
                new DashboardInventoryForecastAggregator(),
                new DashboardInventoryOptimizationAggregator(),
                new DashboardItemPortfolioBuilder(),
                new DashboardItemRelationshipAggregator(),
                new DashboardSnapshotOptions());

            var replayContext = new EntityAnalyticsReplayContext
            {
                PeriodYear = period.Year,
                PeriodMonth = period.Month,
                PeriodStart = period.PeriodStart,
                PeriodEnd = period.PeriodEnd,
                EntityTypeCode = EntityTypeCode.Customer
            };

            var bundle = (CustomerReplayDataBundle)loader.Load(replayContext);
            bundle.FakturRows.Should().NotBeEmpty("test DB should have faktur rows for a recent closed month");

            var result = aggregateService.Aggregate(replayContext, bundle, DateTime.Now);
            var produceInput = (CustomerEntityAnalyticsProduceInput)result.ProduceInput;

            produceInput.PortfolioAggregate.Customers.Count.Should().BeGreaterThan(0);
            result.EntityCount.Should().BeGreaterThan(0);
        }

        private static YearMonthPeriod ResolveHistoricalMonth()
        {
            var today = DateTime.Today;
            var closed = today.AddMonths(-1);
            return new YearMonthPeriod(closed.Year, closed.Month);
        }

        private static bool IsTestSchemaAvailable(DatabaseOptions databaseOptions)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStringHelper.Get(databaseOptions)))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT CASE WHEN OBJECT_ID('dbo.BTR_Faktur', 'U') IS NULL THEN 0 ELSE 1 END",
                        conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
