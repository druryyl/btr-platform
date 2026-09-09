using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries
{
    public class GetPrincipalReturnEvidenceQuery : IRequest<PrincipalReturnEvidenceResponse>
    {
        public string SupplierId { get; set; }
    }

    public class PrincipalReturnEvidenceResponse
    {
        public bool IsAvailable { get; set; }

        public string GoodReturnAmountKpiId { get; set; }

        public string BrokenReturnAmountKpiId { get; set; }

        public string TotalReturnAmountKpiId { get; set; }

        public string ReturnPercentageKpiId { get; set; }

        public string SupplierId { get; set; }

        public string PrincipalName { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal GoodReturnAmount { get; set; }

        public decimal BrokenReturnAmount { get; set; }

        public decimal TotalReturnAmount { get; set; }

        public decimal? ReturnPercentage { get; set; }

        public IList<string> Disclosures { get; set; }
            = new List<string>();

        public IList<PrincipalReturnEvidenceItem> Lines { get; set; }
            = new List<PrincipalReturnEvidenceItem>();
    }

    public class PrincipalReturnEvidenceItem
    {
        public string ReturJualId { get; set; }

        public string ReturJualCode { get; set; }

        public DateTime ReturJualDate { get; set; }

        public string ReturJualItemId { get; set; }

        public string BrgId { get; set; }

        public string JenisRetur { get; set; }

        public string SupplierId { get; set; }

        public string KpiId { get; set; }

        public decimal ReturnAmount { get; set; }
    }

    public class GetPrincipalReturnEvidenceHandler
        : IRequestHandler<GetPrincipalReturnEvidenceQuery, PrincipalReturnEvidenceResponse>
    {
        private readonly IPrincipalReturnSnapshotDal _snapshotDal;
        private readonly IPrincipalReturnEvidenceDal _evidenceDal;

        public GetPrincipalReturnEvidenceHandler(
            IPrincipalReturnSnapshotDal snapshotDal,
            IPrincipalReturnEvidenceDal evidenceDal)
        {
            _snapshotDal = snapshotDal;
            _evidenceDal = evidenceDal;
        }

        public Task<PrincipalReturnEvidenceResponse> Handle(
            GetPrincipalReturnEvidenceQuery request,
            CancellationToken cancellationToken)
        {
            var supplierId = (request?.SupplierId ?? string.Empty).Trim();
            var snapshot = _snapshotDal.GetCurrent();
            if (snapshot is null
                || !string.Equals(snapshot.TotalReturnKpiId, PrincipalKpiCatalog.TotalReturnAmountId, StringComparison.Ordinal)
                || supplierId.Length == 0)
            {
                return Task.FromResult(Empty(supplierId));
            }

            var lines = _evidenceDal.ListReturnItemEvidenceForPrincipal(
                snapshot.PeriodYear,
                snapshot.PeriodMonth,
                supplierId);
            return Task.FromResult(PrincipalReturnEvidenceComposer.Compose(
                supplierId,
                snapshot.PeriodYear,
                snapshot.PeriodMonth,
                lines));
        }

        private static PrincipalReturnEvidenceResponse Empty(string supplierId)
        {
            return new PrincipalReturnEvidenceResponse
            {
                GoodReturnAmountKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnAmountKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnAmountKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SupplierId = supplierId,
                Disclosures = PrincipalReturnDisclosure.Statements.ToList()
            };
        }
    }

    public static class PrincipalReturnEvidenceComposer
    {
        public static PrincipalReturnEvidenceResponse Compose(
            string supplierId,
            int periodYear,
            int periodMonth,
            IEnumerable<PrincipalReturnItemEvidenceLine> lines)
        {
            var mapped = (lines ?? Enumerable.Empty<PrincipalReturnItemEvidenceLine>())
                .Where(line => line != null)
                .Where(line => string.Equals(
                    (line.SupplierId ?? string.Empty).Trim(),
                    supplierId,
                    StringComparison.OrdinalIgnoreCase))
                .Where(line => !string.IsNullOrWhiteSpace(line.ItemSupplierId))
                .Where(line => string.Equals(line.JenisRetur, "BAGUS", StringComparison.Ordinal)
                    || string.Equals(line.JenisRetur, "RUSAK", StringComparison.Ordinal))
                .Select(line => new PrincipalReturnEvidenceItem
                {
                    ReturJualId = line.ReturJualId ?? string.Empty,
                    ReturJualCode = line.ReturJualCode ?? string.Empty,
                    ReturJualDate = line.ReturJualDate,
                    ReturJualItemId = line.ReturJualItemId ?? string.Empty,
                    BrgId = line.BrgId ?? string.Empty,
                    JenisRetur = line.JenisRetur ?? string.Empty,
                    SupplierId = (line.SupplierId ?? string.Empty).Trim(),
                    KpiId = string.Equals(line.JenisRetur, "BAGUS", StringComparison.Ordinal)
                        ? PrincipalKpiCatalog.GoodReturnAmountId
                        : PrincipalKpiCatalog.BrokenReturnAmountId,
                    ReturnAmount = line.SubTotal - line.DiscRp
                })
                .OrderBy(line => line.ReturJualDate)
                .ThenBy(line => line.ReturJualCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(line => line.ReturJualItemId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var principalName = (lines ?? Enumerable.Empty<PrincipalReturnItemEvidenceLine>())
                .Where(line => line != null)
                .Where(line => string.Equals(
                    (line.SupplierId ?? string.Empty).Trim(),
                    supplierId,
                    StringComparison.OrdinalIgnoreCase))
                .Select(line => (line.SupplierName ?? string.Empty).Trim())
                .FirstOrDefault(name => name.Length > 0) ?? string.Empty;

            var goodReturnAmount = mapped
                .Where(line => line.KpiId == PrincipalKpiCatalog.GoodReturnAmountId)
                .Sum(line => line.ReturnAmount);
            var brokenReturnAmount = mapped
                .Where(line => line.KpiId == PrincipalKpiCatalog.BrokenReturnAmountId)
                .Sum(line => line.ReturnAmount);

            return new PrincipalReturnEvidenceResponse
            {
                IsAvailable = true,
                GoodReturnAmountKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnAmountKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnAmountKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                ReturnPercentageKpiId = PrincipalKpiCatalog.ReturnPercentageId,
                SupplierId = supplierId,
                PrincipalName = principalName,
                PeriodYear = periodYear,
                PeriodMonth = periodMonth,
                GoodReturnAmount = goodReturnAmount,
                BrokenReturnAmount = brokenReturnAmount,
                TotalReturnAmount = goodReturnAmount + brokenReturnAmount,
                Disclosures = PrincipalReturnDisclosure.Statements.ToList(),
                Lines = mapped
            };
        }
    }
}
