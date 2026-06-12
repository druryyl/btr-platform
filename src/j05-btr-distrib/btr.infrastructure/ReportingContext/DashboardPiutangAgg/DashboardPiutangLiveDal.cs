using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.Portal;
using btr.application.SupportContext.TglJamAgg;
using btr.nuna.Domain;

namespace btr.infrastructure.ReportingContext.DashboardPiutangAgg
{
    /// <summary>
    /// Legacy live-aggregation helper retained for historical comparison tests.
    /// Production API reads materialized snapshots only.
    /// </summary>
    public class DashboardPiutangLiveDal
    {
        private readonly IPiutangSalesWilayahDal _piutangSalesWilayahDal;
        private readonly ITglJamDal _tglJamDal;
        private readonly IBusinessDateProvider _businessDateProvider;

        public DashboardPiutangLiveDal(
            IPiutangSalesWilayahDal piutangSalesWilayahDal,
            ITglJamDal tglJamDal,
            IBusinessDateProvider businessDateProvider)
        {
            _piutangSalesWilayahDal = piutangSalesWilayahDal;
            _tglJamDal = tglJamDal;
            _businessDateProvider = businessDateProvider;
        }

        public DashboardPiutangResponse GetSummary()
        {
            var periode = OpenReceivablesPeriode();
            var rows = _piutangSalesWilayahDal.ListData(periode)?.ToList()
                       ?? new List<PiutangSalesWilayahDto>();

            var outstanding = rows.Where(r => r.KurangBayar > 1).ToList();
            var today = _businessDateProvider.Today;
            var generatedAt = _tglJamDal.Now;

            var totalPiutang = outstanding.Sum(r => r.KurangBayar);
            var totalCustomer = outstanding
                .Select(ResolveCustomerKey)
                .Where(key => key.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var bucketTotals = outstanding
                .GroupBy(r => PiutangAgingBucketResolver.ResolveBucketKey(r.JatuhTempo, today))
                .ToDictionary(g => g.Key, g => g.Sum(r => r.KurangBayar));

            var agingBuckets = PiutangAgingBucketResolver.BucketDefinitions
                .Select(def => new DashboardPiutangAgingBucket
                {
                    BucketKey = def.Key,
                    BucketLabel = def.Label,
                    Amount = bucketTotals.TryGetValue(def.Key, out var amount) ? amount : 0m,
                    SortOrder = def.SortOrder
                })
                .ToList();

            var overdueCustomerCount = outstanding
                .Where(r => PiutangAgingBucketResolver.ResolveBucketKey(r.JatuhTempo, today) != "Current")
                .GroupBy(r => ResolveCustomerKey(r))
                .Where(g => g.Key.Length > 0)
                .Count(g => g.Sum(r => r.KurangBayar) > 0);

            var current = bucketTotals.TryGetValue("Current", out var currentAmount) ? currentAmount : 0m;
            var over90 = bucketTotals.TryGetValue("DaysOver90", out var over90Amount) ? over90Amount : 0m;

            var topCustomerRisk = outstanding
                .GroupBy(r => ResolveCustomerKey(r))
                .Where(g => g.Key.Length > 0)
                .Select(g => new
                {
                    CustomerName = ResolveCustomerDisplayName(g),
                    CustomerCode = g.Select(r => r.CustomerCode?.Trim())
                        .FirstOrDefault(c => !string.IsNullOrEmpty(c)) ?? g.Key,
                    TotalPiutang = g.Sum(r => r.KurangBayar)
                })
                .OrderByDescending(x => x.TotalPiutang)
                .ThenBy(x => x.CustomerName, StringComparer.OrdinalIgnoreCase)
                .Take(DashboardPiutangAggregator.TopCustomerRiskCount)
                .Select((x, index) => new DashboardPiutangTopCustomerRiskRow
                {
                    Rank = index + 1,
                    CustomerName = x.CustomerName,
                    CustomerCode = x.CustomerCode,
                    TotalPiutang = x.TotalPiutang
                })
                .ToList();

            return new DashboardPiutangResponse
            {
                TotalPiutang = totalPiutang,
                TotalCustomer = totalCustomer,
                GeneratedAt = generatedAt,
                OverdueCustomer = overdueCustomerCount,
                OverduePiutang = totalPiutang - current,
                AgingOver90Amount = over90,
                AgingOver90Percent = totalPiutang > 0 ? over90 / totalPiutang * 100m : (decimal?)null,
                AgingBuckets = agingBuckets,
                TopCustomerRisk = topCustomerRisk
            };
        }

        private Periode OpenReceivablesPeriode()
        {
            var today = _businessDateProvider.Today;
            return new Periode(new DateTime(2000, 1, 1), today);
        }

        private static string ResolveCustomerKey(PiutangSalesWilayahDto row)
        {
            if (row is null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(row.CustomerCode))
                return row.CustomerCode.Trim();

            return row.CustomerName?.Trim() ?? string.Empty;
        }

        private static string ResolveCustomerDisplayName(
            IGrouping<string, PiutangSalesWilayahDto> group)
        {
            return group
                .Select(r => r.CustomerName?.Trim())
                .FirstOrDefault(n => !string.IsNullOrEmpty(n))
                ?? group.Key;
        }
    }
}
