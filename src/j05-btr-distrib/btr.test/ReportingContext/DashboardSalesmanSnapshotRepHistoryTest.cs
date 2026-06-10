using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesmanSnapshotRepHistoryTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode FixedPeriode = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardSalesmanAggregator _aggregator = new DashboardSalesmanAggregator();

        [Fact]
        public void ReplaceCurrent_TwoRefreshesSameMonth_UpdatesSingleHistoryRow()
        {
            var snapshotDal = new InMemorySalesmanSnapshotDal();
            var first = Aggregate(faktur: new[] { Faktur("SP001", "S001", "Rep A", 100m) });
            var second = Aggregate(faktur: new[] { Faktur("SP001", "S001", "Rep A", 150m) });

            snapshotDal.ReplaceCurrent(first, "refresh-log-1");
            snapshotDal.ReplaceCurrent(second, "refresh-log-2");

            var history = snapshotDal.ListRepHistory("SP001", 12);

            history.Should().ContainSingle();
            history[0].PeriodYear.Should().Be(2026);
            history[0].PeriodMonth.Should().Be(6);
            history[0].CompletedOmzet.Should().Be(150m);
        }

        [Fact]
        public void ReplaceCurrent_DoesNotDeletePriorMonthHistory()
        {
            var snapshotDal = new InMemorySalesmanSnapshotDal();
            var mayAggregate = new DashboardSalesmanAggregateResult
            {
                PeriodYear = 2026,
                PeriodMonth = 5,
                GeneratedAt = FixedGeneratedAt,
                RepHistory = new List<DashboardSalesmanRepHistoryRow>
                {
                    HistoryRow(2026, 5, "SP001", "S001", "Rep A", 80m)
                }
            };
            var juneAggregate = Aggregate(faktur: new[] { Faktur("SP001", "S001", "Rep A", 100m) });

            snapshotDal.ReplaceCurrent(mayAggregate, "refresh-log-may");
            snapshotDal.ReplaceCurrent(juneAggregate, "refresh-log-june");

            var history = snapshotDal.ListRepHistory("SP001", 12);

            history.Should().HaveCount(2);
            history[0].PeriodMonth.Should().Be(5);
            history[0].CompletedOmzet.Should().Be(80m);
            history[1].PeriodMonth.Should().Be(6);
            history[1].CompletedOmzet.Should().Be(100m);
        }

        private DashboardSalesmanAggregateResult Aggregate(
            IEnumerable<FakturView> faktur = null,
            IEnumerable<SalesPersonModel> salespeople = null)
        {
            return _aggregator.Aggregate(
                faktur ?? Array.Empty<FakturView>(),
                Array.Empty<PiutangOpenBalanceWithSalesmanDto>(),
                Array.Empty<CustomerLastFakturWithSalesmanDto>(),
                salespeople ?? new[] { SalesPerson("SP001", "S001", "Rep A") },
                new Dictionary<string, decimal?>(),
                FixedPeriode,
                FixedToday,
                FixedGeneratedAt);
        }

        private static DashboardSalesmanRepHistoryRow HistoryRow(
            int year,
            int month,
            string salesPersonId,
            string salesPersonCode,
            string salesPersonName,
            decimal completedOmzet)
        {
            return new DashboardSalesmanRepHistoryRow
            {
                PeriodYear = year,
                PeriodMonth = month,
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                SalesPersonName = salesPersonName,
                CompletedOmzet = completedOmzet,
                IsActive = true
            };
        }

        private static FakturView Faktur(
            string salesPersonId,
            string salesPersonCode,
            string salesPersonName,
            decimal grandTotal)
        {
            return new FakturView
            {
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                SalesPersonName = salesPersonName,
                CustomerCode = "C001",
                Customer = "Customer",
                GrandTotal = grandTotal,
                Tgl = FixedToday
            };
        }

        private static SalesPersonModel SalesPerson(string id, string code, string name)
        {
            return new SalesPersonModel
            {
                SalesPersonId = id,
                SalesPersonCode = code,
                SalesPersonName = name
            };
        }

        /// <summary>
        /// In-memory snapshot store that mirrors BTRPD_SalesmanRepHistory MERGE upsert semantics.
        /// </summary>
        private sealed class InMemorySalesmanSnapshotDal : IDashboardSalesmanSnapshotDal
        {
            private readonly Dictionary<string, DashboardSalesmanRepHistoryRow> _repHistory =
                new Dictionary<string, DashboardSalesmanRepHistoryRow>(StringComparer.OrdinalIgnoreCase);

            private DashboardSalesmanAggregateResult _current;

            public DashboardSalesmanAggregateResult GetCurrent() => _current;

            public void ReplaceCurrent(DashboardSalesmanAggregateResult result, string refreshLogId)
            {
                _current = result;
                UpsertRepHistory(result?.RepHistory);
            }

            public IList<DashboardSalesmanPrincipalAchievementRow> ListPrincipalAchievement(string salesPersonId)
            {
                return Array.Empty<DashboardSalesmanPrincipalAchievementRow>();
            }

            public IList<DashboardSalesmanRepHistoryRow> ListRepHistory(string salesPersonId, int months)
            {
                if (months <= 0)
                    months = 12;

                if (months > 12)
                    months = 12;

                return _repHistory.Values
                    .Where(r => string.Equals(r.SalesPersonId, salesPersonId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.PeriodYear)
                    .ThenByDescending(r => r.PeriodMonth)
                    .Take(months)
                    .OrderBy(r => r.PeriodYear)
                    .ThenBy(r => r.PeriodMonth)
                    .ToList();
            }

            private void UpsertRepHistory(IEnumerable<DashboardSalesmanRepHistoryRow> rows)
            {
                foreach (var row in rows ?? Enumerable.Empty<DashboardSalesmanRepHistoryRow>())
                {
                    if (string.IsNullOrWhiteSpace(row.SalesPersonId))
                        continue;

                    var key = RepHistoryKey(row.PeriodYear, row.PeriodMonth, row.SalesPersonId);
                    _repHistory[key] = new DashboardSalesmanRepHistoryRow
                    {
                        PeriodYear = row.PeriodYear,
                        PeriodMonth = row.PeriodMonth,
                        SalesPersonId = row.SalesPersonId,
                        SalesPersonCode = row.SalesPersonCode,
                        SalesPersonName = row.SalesPersonName,
                        TargetAmount = row.TargetAmount,
                        CompletedOmzet = row.CompletedOmzet,
                        AchievementPercent = row.AchievementPercent,
                        AchievementBand = row.AchievementBand,
                        OpenBalance = row.OpenBalance,
                        IsActive = row.IsActive
                    };
                }
            }

            private static string RepHistoryKey(int periodYear, int periodMonth, string salesPersonId)
            {
                return $"{periodYear}|{periodMonth}|{salesPersonId?.Trim()}";
            }
        }
    }
}
