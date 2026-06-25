using System;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Progress;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders
{
    public class CustomerReplayDataLoader : IEntityAnalyticsReplayDataLoader
    {
        private readonly IFakturViewDal _fakturViewDal;
        private readonly ICustomerLastFakturDal _lastFakturDal;
        private readonly ICustomerFirstFakturDal _firstFakturDal;
        private readonly ICustomerPurchaseFrequencyDal _purchaseFrequencyDal;
        private readonly IPiutangOpenBalanceDal _openBalanceDal;
        private readonly ICustomerDal _customerDal;
        private readonly ICustomerOmzetHistoryDal _customerOmzetHistoryDal;
        private readonly ICustomerPelunasanSummaryDal _customerPelunasanSummaryDal;
        private readonly ICustomerPaymentBehaviorDal _customerPaymentBehaviorDal;
        private readonly ICustomerMtdItemRollupDal _customerMtdItemRollupDal;
        private readonly DashboardSnapshotOptions _options;

        public CustomerReplayDataLoader(
            IFakturViewDal fakturViewDal,
            ICustomerLastFakturDal lastFakturDal,
            ICustomerFirstFakturDal firstFakturDal,
            ICustomerPurchaseFrequencyDal purchaseFrequencyDal,
            IPiutangOpenBalanceDal openBalanceDal,
            ICustomerDal customerDal,
            ICustomerOmzetHistoryDal customerOmzetHistoryDal,
            ICustomerPelunasanSummaryDal customerPelunasanSummaryDal,
            ICustomerPaymentBehaviorDal customerPaymentBehaviorDal,
            ICustomerMtdItemRollupDal customerMtdItemRollupDal,
            DashboardSnapshotOptions options)
        {
            _fakturViewDal = fakturViewDal;
            _lastFakturDal = lastFakturDal;
            _firstFakturDal = firstFakturDal;
            _purchaseFrequencyDal = purchaseFrequencyDal;
            _openBalanceDal = openBalanceDal;
            _customerDal = customerDal;
            _customerOmzetHistoryDal = customerOmzetHistoryDal;
            _customerPelunasanSummaryDal = customerPelunasanSummaryDal;
            _customerPaymentBehaviorDal = customerPaymentBehaviorDal;
            _customerMtdItemRollupDal = customerMtdItemRollupDal;
            _options = options ?? new DashboardSnapshotOptions();
        }

        public string EntityType => EntityTypeCode.Customer;

        public object Load(EntityAnalyticsReplayContext replayContext)
        {
            if (replayContext is null)
                throw new ArgumentNullException(nameof(replayContext));

            var periodEnd = replayContext.PeriodEnd.Date;
            var periode = new Periode(replayContext.PeriodStart, periodEnd);
            var priorMonth = replayContext.PeriodStart.AddMonths(-1);
            var priorPeriode = new Periode(
                new DateTime(priorMonth.Year, priorMonth.Month, 1),
                new DateTime(priorMonth.Year, priorMonth.Month, DateTime.DaysInMonth(priorMonth.Year, priorMonth.Month)));

            var forecastOptions = CustomerRiskForecastOptions.FromDashboardOptions(_options);
            var portfolioOptions = CustomerPortfolioOptions.FromDashboardOptions(_options);
            var stepPrefix = $"Backfill:{EntityType}:{replayContext.PeriodYear:D4}-{replayContext.PeriodMonth:D2}";

            WorkerProgressScope.Current?.StepStarted($"{stepPrefix}:Load", "Load historical customer source data");

            var bundle = new CustomerReplayDataBundle
            {
                FakturRows = _fakturViewDal.ListData(periode)?.ToList() ?? new System.Collections.Generic.List<FakturView>(),
                OmzetHistoryRows = _customerOmzetHistoryDal.ListOmzetByCustomer(periode, priorPeriode)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerOmzetHistoryDto>(),
                LastFakturRows = _lastFakturDal.ListLastFakturByCustomerAsOf(periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerLastFakturDto>(),
                PiutangRows = _openBalanceDal.ListOpenBalancesAsOf(periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<PiutangOpenBalanceDto>(),
                Customers = _customerDal.ListData()?.ToList()
                    ?? new System.Collections.Generic.List<btr.domain.SalesContext.CustomerAgg.CustomerModel>(),
                PelunasanSummaryRows = _customerPelunasanSummaryDal
                    .ListSummary(periodEnd.AddDays(-forecastOptions.NoPaymentRecencyDays), periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerPelunasanSummaryDto>(),
                PaymentBehaviorRows = _customerPaymentBehaviorDal.ListPaymentBehavior(
                    periodEnd.AddDays(-forecastOptions.PaymentLagLookbackDays),
                    periodEnd,
                    forecastOptions.MinSettledFaktursForLag)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerPaymentBehaviorDto>(),
                LastFakturWithSalesman = _lastFakturDal.ListLastFakturWithSalesmanByCustomerAsOf(periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerLastFakturWithSalesmanDto>(),
                FirstFakturRows = _firstFakturDal.ListFirstFakturByCustomer()?.ToList()
                    ?? new System.Collections.Generic.List<CustomerFirstFakturDto>(),
                FrequencyRows = _purchaseFrequencyDal.ListFakturCountByCustomer(
                    periodEnd.AddMonths(-portfolioOptions.PurchaseFrequencyLookbackMonths),
                    periodEnd)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerPurchaseFrequencyDto>(),
                ItemRollupRows = _customerMtdItemRollupDal.ListMtdItemRollups(periode)?.ToList()
                    ?? new System.Collections.Generic.List<CustomerMtdItemRollupDto>()
            };

            WorkerProgressScope.Current?.StepCompleted($"{stepPrefix}:Load", new WorkerProgressStepInfo
            {
                RecordCount = bundle.FakturRows.Count + bundle.Customers.Count + bundle.PiutangRows.Count
            });

            return bundle;
        }
    }
}
