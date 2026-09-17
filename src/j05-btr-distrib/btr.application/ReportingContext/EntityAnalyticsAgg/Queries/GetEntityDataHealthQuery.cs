using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using MediatR;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class GetEntityDataHealthQuery : IRequest<EntityDataHealthResponse>
    {
        public string EntityType { get; set; }
    }

    public class EntityDataHealthResponse
    {
        public bool IsAvailable { get; set; }

        public string EntityType { get; set; }

        public int? PeriodYear { get; set; }

        public int? PeriodMonth { get; set; }

        public DateTime? GeneratedAt { get; set; }

        /// <summary>
        /// Share of the Sales-Out population that has a target assignment, as a percentage (0-100).
        /// </summary>
        public decimal? TargetCoveragePercentage { get; set; }

        /// <summary>
        /// Principals in the Sales-Out population with no target responsibility.
        /// </summary>
        public int PrincipalsMissingTargetCount { get; set; }

        /// <summary>
        /// Unknown Principal exception line count from the Principal Sales-Out data-quality output.
        /// </summary>
        public int UnknownPrincipalExceptionCount { get; set; }

        /// <summary>
        /// Unknown Principal exception amount from the Principal Sales-Out data-quality output.
        /// </summary>
        public decimal UnknownPrincipalExceptionAmount { get; set; }
    }

    public class GetEntityDataHealthHandler
        : IRequestHandler<GetEntityDataHealthQuery, EntityDataHealthResponse>
    {
        private const int CoverageScale = 2;

        private readonly IEntityTypeRegistry _entityTypes;
        private readonly IPrincipalSalesOutSnapshotDal _salesOutSnapshotDal;
        private readonly IPrincipalTargetSnapshotDal _targetSnapshotDal;

        public GetEntityDataHealthHandler(
            IEntityTypeRegistry entityTypes,
            IPrincipalSalesOutSnapshotDal salesOutSnapshotDal,
            IPrincipalTargetSnapshotDal targetSnapshotDal)
        {
            _entityTypes = entityTypes;
            _salesOutSnapshotDal = salesOutSnapshotDal;
            _targetSnapshotDal = targetSnapshotDal;
        }

        public Task<EntityDataHealthResponse> Handle(
            GetEntityDataHealthQuery request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EntityType))
                throw new ArgumentException("EntityType is required.");

            if (!_entityTypes.TryGet(request.EntityType, out _))
                throw new ArgumentException($"Unknown entity type: {request.EntityType}");

            // Data Health disclosures are Principal (Supplier) specific (IW-OQ-003).
            if (!string.Equals(request.EntityType, EntityTypeCode.Supplier, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new EntityDataHealthResponse
                {
                    EntityType = request.EntityType,
                    IsAvailable = false
                });
            }

            var salesOut = _salesOutSnapshotDal.GetCurrent();
            var target = _targetSnapshotDal.GetCurrent();

            return Task.FromResult(Compose(salesOut, target, request.EntityType));
        }

        public static EntityDataHealthResponse Compose(
            PrincipalSalesOutAggregateResult snapshot,
            PrincipalTargetAggregateResult target,
            string entityType)
        {
            var response = new EntityDataHealthResponse
            {
                EntityType = entityType,
                IsAvailable = false,
                PrincipalsMissingTargetCount = 0,
                UnknownPrincipalExceptionCount = 0,
                UnknownPrincipalExceptionAmount = 0m
            };

            if (snapshot is null || snapshot.KpiId != PrincipalKpiCatalog.SalesOutId)
                return response;

            var principals = (snapshot.Principals ?? new List<PrincipalSalesOutRow>())
                .Where(row => row != null && row.KpiId == PrincipalKpiCatalog.SalesOutId)
                .ToList();

            var targetsBySupplier = IndexMatchingTargets(target, snapshot.PeriodYear, snapshot.PeriodMonth);

            var total = principals.Count;
            var withTarget = principals.Count(p =>
                targetsBySupplier.ContainsKey((p.SupplierId ?? string.Empty).Trim()));
            var missing = total - withTarget;

            var unknownExceptions = (snapshot.DataQuality ?? new List<PrincipalSalesOutDataQualityRow>())
                .Where(IsUnknownPrincipalException)
                .ToList();

            response.IsAvailable = true;
            response.PeriodYear = snapshot.PeriodYear;
            response.PeriodMonth = snapshot.PeriodMonth;
            response.GeneratedAt = snapshot.GeneratedAt;
            response.PrincipalsMissingTargetCount = missing;
            response.TargetCoveragePercentage = total == 0
                ? (decimal?)null
                : Math.Round((decimal)withTarget / total * 100m, CoverageScale, MidpointRounding.AwayFromZero);
            response.UnknownPrincipalExceptionCount = unknownExceptions.Sum(r => r.LineCount);
            response.UnknownPrincipalExceptionAmount = unknownExceptions.Sum(r => r.Amount);

            return response;
        }

        private static Dictionary<string, PrincipalTargetRow> IndexMatchingTargets(
            PrincipalTargetAggregateResult target,
            int periodYear,
            int periodMonth)
        {
            var bySupplier = new Dictionary<string, PrincipalTargetRow>(StringComparer.OrdinalIgnoreCase);
            if (target is null)
                return bySupplier;

            if (target.KpiId != PrincipalKpiCatalog.TargetId)
                return bySupplier;

            if (target.PeriodYear != periodYear || target.PeriodMonth != periodMonth)
                return bySupplier;

            foreach (var row in target.Principals ?? new List<PrincipalTargetRow>())
            {
                if (row is null)
                    continue;

                if (!string.Equals(row.KpiId, PrincipalKpiCatalog.TargetId, StringComparison.Ordinal))
                    continue;

                var supplierId = (row.SupplierId ?? string.Empty).Trim();
                if (supplierId.Length == 0 || bySupplier.ContainsKey(supplierId))
                    continue;

                bySupplier[supplierId] = row;
            }

            return bySupplier;
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
