using System;
using System.Collections.Generic;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Loaders;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.SalesContext.CustomerAgg.Contracts;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.CustomerAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerReplayDataLoaderTest
    {
        [Fact]
        public void Load_UsesPeriodEndForAsOfQueries()
        {
            var periodEnd = new DateTime(2024, 3, 31);
            var replayContext = new EntityAnalyticsReplayContext
            {
                PeriodYear = 2024,
                PeriodMonth = 3,
                PeriodStart = new DateTime(2024, 3, 1),
                PeriodEnd = periodEnd,
                EntityTypeCode = EntityTypeCode.Customer
            };

            var openBalanceDal = new RecordingPiutangOpenBalanceDal();
            var lastFakturDal = new RecordingCustomerLastFakturDal();

            var loader = new CustomerReplayDataLoader(
                new StubFakturViewDal(),
                lastFakturDal,
                new StubCustomerFirstFakturDal(),
                new StubCustomerPurchaseFrequencyDal(),
                openBalanceDal,
                new StubCustomerDal(),
                new StubCustomerOmzetHistoryDal(),
                new StubCustomerPelunasanSummaryDal(),
                new StubCustomerPaymentBehaviorDal(),
                new StubCustomerMtdItemRollupDal(),
                new DashboardSnapshotOptions());

            var bundle = (CustomerReplayDataBundle)loader.Load(replayContext);

            openBalanceDal.LastAsOfDate.Should().Be(periodEnd.Date);
            lastFakturDal.LastAsOfDate.Should().Be(periodEnd.Date);
            bundle.FakturRows.Should().NotBeNull();
        }

        private sealed class RecordingPiutangOpenBalanceDal : IPiutangOpenBalanceDal
        {
            public DateTime? LastAsOfDate { get; private set; }

            public IReadOnlyList<PiutangOpenBalanceDto> ListOpenBalances() => Array.Empty<PiutangOpenBalanceDto>();

            public IReadOnlyList<PiutangOpenBalanceDto> ListOpenBalancesAsOf(DateTime asOfDate)
            {
                LastAsOfDate = asOfDate;
                return Array.Empty<PiutangOpenBalanceDto>();
            }
        }

        private sealed class RecordingCustomerLastFakturDal : ICustomerLastFakturDal
        {
            public DateTime? LastAsOfDate { get; private set; }

            public IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomer() =>
                Array.Empty<CustomerLastFakturDto>();

            public IEnumerable<CustomerLastFakturWithSalesmanDto> ListLastFakturWithSalesmanByCustomer() =>
                Array.Empty<CustomerLastFakturWithSalesmanDto>();

            public IEnumerable<CustomerLastFakturDto> ListLastFakturByCustomerAsOf(DateTime asOfDate)
            {
                LastAsOfDate = asOfDate;
                return Array.Empty<CustomerLastFakturDto>();
            }

            public IEnumerable<CustomerLastFakturWithSalesmanDto> ListLastFakturWithSalesmanByCustomerAsOf(DateTime asOfDate)
            {
                LastAsOfDate = asOfDate;
                return Array.Empty<CustomerLastFakturWithSalesmanDto>();
            }
        }

        private sealed class StubFakturViewDal : IFakturViewDal
        {
            public IEnumerable<FakturView> ListData(Periode periode) => Array.Empty<FakturView>();
            public IEnumerable<FakturView> ListTerhapus(Periode periode) => Array.Empty<FakturView>();
        }

        private sealed class StubCustomerFirstFakturDal : ICustomerFirstFakturDal
        {
            public IEnumerable<CustomerFirstFakturDto> ListFirstFakturByCustomer() =>
                Array.Empty<CustomerFirstFakturDto>();
        }

        private sealed class StubCustomerPurchaseFrequencyDal : ICustomerPurchaseFrequencyDal
        {
            public IEnumerable<CustomerPurchaseFrequencyDto> ListFakturCountByCustomer(DateTime from, DateTime to) =>
                Array.Empty<CustomerPurchaseFrequencyDto>();
        }

        private sealed class StubCustomerDal : ICustomerDal
        {
            public IEnumerable<CustomerModel> ListData() => Array.Empty<CustomerModel>();
            public IEnumerable<CustomerLocationView> ListLocation() => Array.Empty<CustomerLocationView>();
            public void Insert(CustomerModel data) => throw new NotSupportedException();
            public void Update(CustomerModel data) => throw new NotSupportedException();
            public void Delete(ICustomerKey key) => throw new NotSupportedException();
            public CustomerModel GetData(ICustomerKey key) => throw new NotSupportedException();
        }

        private sealed class StubCustomerOmzetHistoryDal : ICustomerOmzetHistoryDal
        {
            public IEnumerable<CustomerOmzetHistoryDto> ListOmzetByCustomer(Periode periode, Periode priorMonth) =>
                Array.Empty<CustomerOmzetHistoryDto>();
        }

        private sealed class StubCustomerPelunasanSummaryDal : ICustomerPelunasanSummaryDal
        {
            public IEnumerable<CustomerPelunasanSummaryDto> ListSummary(DateTime from, DateTime to) =>
                Array.Empty<CustomerPelunasanSummaryDto>();
        }

        private sealed class StubCustomerPaymentBehaviorDal : ICustomerPaymentBehaviorDal
        {
            public IEnumerable<CustomerPaymentBehaviorDto> ListPaymentBehavior(DateTime from, DateTime to, int minSettled) =>
                Array.Empty<CustomerPaymentBehaviorDto>();
        }

        private sealed class StubCustomerMtdItemRollupDal : ICustomerMtdItemRollupDal
        {
            public IEnumerable<CustomerMtdItemRollupDto> ListMtdItemRollups(Periode periode) =>
                Array.Empty<CustomerMtdItemRollupDto>();
        }
    }
}
