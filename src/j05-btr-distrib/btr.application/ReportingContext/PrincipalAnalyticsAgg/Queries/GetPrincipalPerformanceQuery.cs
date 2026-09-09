using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using MediatR;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries
{
    public class GetPrincipalPerformanceQuery : IRequest<PrincipalPerformanceResponse>
    {
    }

    public class PrincipalPerformanceResponse
    {
        public bool IsAvailable { get; set; }

        public string KpiId { get; set; }

        public string KpiName { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }

        public int UnknownPrincipalExceptionCount { get; set; }

        public IList<string> Disclosures { get; set; }
            = new List<string>();

        public IList<PrincipalPerformanceRankingItem> Ranking { get; set; }
            = new List<PrincipalPerformanceRankingItem>();
    }

    public class PrincipalPerformanceRankingItem
    {
        public int Rank { get; set; }

        public string PrincipalName { get; set; }

        public string SupplierId { get; set; }

        public string KpiId { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }
    }

    public class GetPrincipalPerformanceHandler
        : IRequestHandler<GetPrincipalPerformanceQuery, PrincipalPerformanceResponse>
    {
        private readonly IPrincipalSalesOutSnapshotDal _snapshotDal;

        public GetPrincipalPerformanceHandler(IPrincipalSalesOutSnapshotDal snapshotDal)
        {
            _snapshotDal = snapshotDal;
        }

        public Task<PrincipalPerformanceResponse> Handle(
            GetPrincipalPerformanceQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(PrincipalPerformanceComposer.Compose(_snapshotDal.GetCurrent()));
        }
    }

    public static class PrincipalPerformanceComposer
    {
        public static PrincipalPerformanceResponse Compose(PrincipalSalesOutAggregateResult snapshot)
        {
            var response = new PrincipalPerformanceResponse
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                KpiName = "Principal Sales-Out",
                Disclosures = PrincipalSalesOutDisclosure.Statements.ToList()
            };

            if (snapshot is null || snapshot.KpiId != PrincipalKpiCatalog.SalesOutId)
                return response;

            var ranking = (snapshot.Principals ?? new List<PrincipalSalesOutRow>())
                .Where(row => row != null && row.KpiId == PrincipalKpiCatalog.SalesOutId)
                .OrderBy(row => row.SortOrder)
                .ThenByDescending(row => row.SalesOutAmount)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) => new PrincipalPerformanceRankingItem
                {
                    Rank = row.SortOrder > 0 ? row.SortOrder : index + 1,
                    PrincipalName = row.SupplierName ?? string.Empty,
                    SupplierId = row.SupplierId ?? string.Empty,
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = row.SalesOutAmount
                })
                .ToList();

            response.IsAvailable = true;
            response.PeriodYear = snapshot.PeriodYear;
            response.PeriodMonth = snapshot.PeriodMonth;
            response.GeneratedAt = snapshot.GeneratedAt;
            response.PrincipalSalesOutAmount = ranking.Sum(row => row.PrincipalSalesOutAmount);
            response.UnknownPrincipalExceptionCount = CountUnknownPrincipalExceptions(snapshot.DataQuality);
            response.Ranking = ranking;
            return response;
        }

        private static int CountUnknownPrincipalExceptions(
            IEnumerable<PrincipalSalesOutDataQualityRow> dataQuality)
        {
            return (dataQuality ?? Enumerable.Empty<PrincipalSalesOutDataQualityRow>())
                .Where(IsUnknownPrincipalException)
                .Sum(row => row.LineCount);
        }

        private static bool IsUnknownPrincipalException(PrincipalSalesOutDataQualityRow row)
        {
            if (row is null || string.IsNullOrWhiteSpace(row.ExceptionCode))
                return false;

            return row.ExceptionCode == PrincipalSalesOutSnapshot.BlankSupplierExceptionCode
                || row.ExceptionCode == PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode;
        }
    }
}
