using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using MediatR;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class CompareEntitiesQuery : IRequest<EntityCompareResponse>
    {
        public string EntityType { get; set; }

        public string EntityIds { get; set; }

        public string KpiIds { get; set; }

        public int? PeriodYear { get; set; }

        public int? PeriodMonth { get; set; }
    }

    public class EntityCompareResponse
    {
        public string EntityType { get; set; }

        public string ContractVersion { get; set; }

        public List<CompareEntityColumnDto> Entities { get; set; } = new List<CompareEntityColumnDto>();

        public CompareKpiSectionDto KpiComparison { get; set; }

        public CompareTrendSectionDto TrendComparison { get; set; }

        public CompareRankingSectionDto RankingComparison { get; set; }

        public CompareAttentionSectionDto AttentionComparison { get; set; }

        public CompareRelationshipSectionDto RelationshipComparison { get; set; }

        public CompareRadarSectionDto RadarComparison { get; set; }

        public ComparePeerSectionDto PeerComparison { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class CompareEntityColumnDto
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public string ProfileRoute { get; set; }
    }

    public class CompareKpiSectionDto : ProfileSectionDtoBase
    {
        public List<CompareKpiRowDto> Rows { get; set; } = new List<CompareKpiRowDto>();
    }

    public class CompareKpiRowDto
    {
        public string KpiId { get; set; }

        public string DisplayName { get; set; }

        public string Unit { get; set; }

        public string Direction { get; set; }

        public List<CompareKpiCellDto> Values { get; set; } = new List<CompareKpiCellDto>();
    }

    public class CompareKpiCellDto
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public decimal? Value { get; set; }

        public string FormattedValue { get; set; }

        public string PeriodLabel { get; set; }
    }

    public class CompareTrendSectionDto : ProfileSectionDtoBase
    {
        public List<CompareTrendOverlayDto> Overlays { get; set; } = new List<CompareTrendOverlayDto>();
    }

    public class CompareTrendOverlayDto
    {
        public string KpiId { get; set; }

        public string DisplayName { get; set; }

        public string Unit { get; set; }

        public List<CompareTrendEntitySeriesDto> EntitySeries { get; set; } = new List<CompareTrendEntitySeriesDto>();
    }

    public class CompareTrendEntitySeriesDto
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public List<ProfileTrendPointDto> Points { get; set; } = new List<ProfileTrendPointDto>();
    }

    public class CompareRankingSectionDto : ProfileSectionDtoBase
    {
        public List<CompareRankingEntityDto> Entities { get; set; } = new List<CompareRankingEntityDto>();
    }

    public class CompareRankingEntityDto
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public ProfileRankingSectionDto Ranking { get; set; }
    }

    public class CompareAttentionSectionDto : ProfileSectionDtoBase
    {
        public List<CompareAttentionEntityDto> Entities { get; set; } = new List<CompareAttentionEntityDto>();
    }

    public class CompareAttentionEntityDto
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public ProfileAttentionSectionDto Attention { get; set; }
    }

    public class CompareRelationshipSectionDto : ProfileSectionDtoBase
    {
        public List<CompareRelationshipEntityDto> Entities { get; set; } = new List<CompareRelationshipEntityDto>();
    }

    public class CompareRelationshipEntityDto
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public ProfileRelatedEntitiesSectionDto RelatedEntities { get; set; }
    }

    public class CompareRadarSectionDto : ProfileSectionDtoBase
    {
        public string PeerGroupRuleId { get; set; }

        public int? PeerGroupSize { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string PeriodLabel { get; set; }

        public List<CompareRadarAxisDto> Axes { get; set; } = new List<CompareRadarAxisDto>();

        public List<CompareRadarOverlayDto> Overlays { get; set; } = new List<CompareRadarOverlayDto>();

        public List<decimal?> PeerAverageScores { get; set; }
    }

    public class CompareRadarAxisDto
    {
        public string KpiId { get; set; }

        public string SignatureDimensionKey { get; set; }

        public string DisplayName { get; set; }

        public string Direction { get; set; }
    }

    public class CompareRadarOverlayDto
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public List<decimal?> Scores { get; set; } = new List<decimal?>();
    }

    public class ComparePeerSectionDto : ProfileSectionDtoBase
    {
        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public string PeerGroupRuleId { get; set; }

        public int? PeerGroupSize { get; set; }

        public ProfileRadarSectionDto Radar { get; set; }
    }

    public class CompareEntitiesHandler : IRequestHandler<CompareEntitiesQuery, EntityCompareResponse>
    {
        private readonly IEntityComparisonEngine _comparisonEngine;
        private readonly IEntityTypeRegistry _entityTypes;

        public CompareEntitiesHandler(IEntityComparisonEngine comparisonEngine, IEntityTypeRegistry entityTypes)
        {
            _comparisonEngine = comparisonEngine;
            _entityTypes = entityTypes;
        }

        public Task<EntityCompareResponse> Handle(CompareEntitiesQuery request, CancellationToken cancellationToken)
        {
            var normalizedType = _entityTypes.NormalizeEntityTypeCode(request.EntityType);
            if (normalizedType is null)
                throw new ArgumentException($"Unknown entity type: {request.EntityType}", nameof(request.EntityType));

            var entityIds = ParseCsv(request.EntityIds);
            if (entityIds.Count < 2 || entityIds.Count > 5)
                throw new ArgumentException("Between 2 and 5 entity IDs are required.", nameof(request.EntityIds));

            var kpiIds = ParseCsv(request.KpiIds);

            var context = new ComparisonContext
            {
                Mode = ComparisonMode.MultiEntity,
                EntityType = normalizedType,
                EntityIds = entityIds,
                MetricKpiIds = kpiIds.Count > 0 ? kpiIds : null,
                PeriodYear = request.PeriodYear,
                PeriodMonth = request.PeriodMonth
            };

            return Task.FromResult(_comparisonEngine.BuildMultiEntityComparison(context));
        }

        private static List<string> ParseCsv(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new List<string>();

            return value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
