using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.nuna.Domain;
using MediatR;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries
{
    public class GetPrincipalSalesOutEvidenceQuery : IRequest<PrincipalSalesOutEvidenceResponse>
    {
        public string SupplierId { get; set; }
    }

    public class PrincipalSalesOutEvidenceResponse
    {
        public bool IsAvailable { get; set; }

        public string KpiId { get; set; }

        public string KpiName { get; set; }

        public string SupplierId { get; set; }

        public string PrincipalName { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }

        public IList<string> Disclosures { get; set; }
            = new List<string>();

        public IList<PrincipalSalesOutEvidenceItem> Lines { get; set; }
            = new List<PrincipalSalesOutEvidenceItem>();
    }

    public class PrincipalSalesOutEvidenceItem
    {
        public string FakturId { get; set; }

        public string FakturCode { get; set; }

        public DateTime FakturDate { get; set; }

        public string FakturItemId { get; set; }

        public string BrgId { get; set; }

        public string SupplierId { get; set; }

        public string KpiId { get; set; }

        public decimal PrincipalSalesOutAmount { get; set; }
    }

    public class GetPrincipalSalesOutEvidenceHandler
        : IRequestHandler<GetPrincipalSalesOutEvidenceQuery, PrincipalSalesOutEvidenceResponse>
    {
        private readonly IPrincipalSalesOutSnapshotDal _snapshotDal;
        private readonly IPrincipalSalesOutEvidenceDal _evidenceDal;

        public GetPrincipalSalesOutEvidenceHandler(
            IPrincipalSalesOutSnapshotDal snapshotDal,
            IPrincipalSalesOutEvidenceDal evidenceDal)
        {
            _snapshotDal = snapshotDal;
            _evidenceDal = evidenceDal;
        }

        public Task<PrincipalSalesOutEvidenceResponse> Handle(
            GetPrincipalSalesOutEvidenceQuery request,
            CancellationToken cancellationToken)
        {
            var supplierId = (request?.SupplierId ?? string.Empty).Trim();
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot is null
                || snapshot.KpiId != PrincipalKpiCatalog.SalesOutId
                || supplierId.Length == 0)
            {
                return Task.FromResult(Empty(supplierId));
            }

            var period = MonthPeriod(snapshot.PeriodYear, snapshot.PeriodMonth);
            var lines = _evidenceDal.ListFakturItemEvidenceForPrincipal(period, supplierId);
            return Task.FromResult(PrincipalSalesOutEvidenceComposer.Compose(
                supplierId,
                snapshot.PeriodYear,
                snapshot.PeriodMonth,
                lines));
        }

        private static PrincipalSalesOutEvidenceResponse Empty(string supplierId)
        {
            return new PrincipalSalesOutEvidenceResponse
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                KpiName = "Principal Sales-Out",
                SupplierId = supplierId,
                Disclosures = PrincipalSalesOutDisclosure.Statements.ToList()
            };
        }

        private static Periode MonthPeriod(int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return new Periode(monthStart, monthEnd);
        }
    }

    public static class PrincipalSalesOutEvidenceComposer
    {
        public static PrincipalSalesOutEvidenceResponse Compose(
            string supplierId,
            int periodYear,
            int periodMonth,
            IEnumerable<PrincipalSalesOutFakturItemEvidenceLine> lines)
        {
            var mapped = (lines ?? Enumerable.Empty<PrincipalSalesOutFakturItemEvidenceLine>())
                .Where(line => line != null)
                .Where(line => string.Equals(
                    (line.SupplierId ?? string.Empty).Trim(),
                    supplierId,
                    StringComparison.OrdinalIgnoreCase))
                .Where(line => !string.IsNullOrWhiteSpace(line.ItemSupplierId))
                .Select(line => new PrincipalSalesOutEvidenceItem
                {
                    FakturId = line.FakturId ?? string.Empty,
                    FakturCode = line.FakturCode ?? string.Empty,
                    FakturDate = line.FakturDate,
                    FakturItemId = line.FakturItemId ?? string.Empty,
                    BrgId = line.BrgId ?? string.Empty,
                    SupplierId = (line.SupplierId ?? string.Empty).Trim(),
                    KpiId = PrincipalKpiCatalog.SalesOutId,
                    PrincipalSalesOutAmount = line.SubTotal - line.DiscRp
                })
                .OrderBy(line => line.FakturDate)
                .ThenBy(line => line.FakturCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.FakturItemId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var principalName = (lines ?? Enumerable.Empty<PrincipalSalesOutFakturItemEvidenceLine>())
                .Where(line => line != null)
                .Where(line => string.Equals(
                    (line.SupplierId ?? string.Empty).Trim(),
                    supplierId,
                    StringComparison.OrdinalIgnoreCase))
                .Select(line => (line.SupplierName ?? string.Empty).Trim())
                .FirstOrDefault(name => name.Length > 0) ?? string.Empty;

            return new PrincipalSalesOutEvidenceResponse
            {
                IsAvailable = true,
                KpiId = PrincipalKpiCatalog.SalesOutId,
                KpiName = "Principal Sales-Out",
                SupplierId = supplierId,
                PrincipalName = principalName,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                PrincipalSalesOutAmount = mapped.Sum(line => line.PrincipalSalesOutAmount),
                Disclosures = PrincipalSalesOutDisclosure.Statements.ToList(),
                Lines = mapped
            };
        }
    }
}
