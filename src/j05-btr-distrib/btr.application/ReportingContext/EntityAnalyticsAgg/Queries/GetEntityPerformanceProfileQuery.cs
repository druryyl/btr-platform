using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class GetEntityPerformanceProfileQuery : IRequest<EntityPerformanceProfileResponse>
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }
    }

    public class EntityPerformanceProfileResponse
    {
        public bool IsAvailable { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public DateTime? GeneratedAt { get; set; }

        /// <summary>L0 snapshot version backing this profile (e.g. L0-v1).</summary>
        public string SnapshotVersion { get; set; }

        /// <summary>API response contract version for additive evolution tracking.</summary>
        public string ContractVersion { get; set; }

        public ProfileOverviewSectionDto Overview { get; set; }

        public ProfileKpiSummarySectionDto KpiSummary { get; set; }

        public ProfileComparisonSectionDto Comparison { get; set; }

        public ProfileTrendSectionDto Trend { get; set; }

        public ProfileRadarSectionDto Radar { get; set; }

        public ProfileRankingSectionDto Ranking { get; set; }

        public ProfileAttentionSectionDto Attention { get; set; }

        public ProfileRelatedEntitiesSectionDto RelatedEntities { get; set; }

        public ProfileEvidenceSectionDto Evidence { get; set; }
    }

    public abstract class ProfileSectionDtoBase
    {
        public bool IsAvailable { get; set; }

        public string UnavailableReason { get; set; }
    }

    public class ProfileOverviewSectionDto : ProfileSectionDtoBase
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public bool IsActive { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public Dictionary<string, string> Dimensions { get; set; } = new Dictionary<string, string>();
    }

    public class ProfileKpiSummarySectionDto : ProfileSectionDtoBase
    {
        public List<ProfileKpiCategoryGroupDto> Categories { get; set; } = new List<ProfileKpiCategoryGroupDto>();
    }

    public class ProfileKpiCategoryGroupDto
    {
        public string Category { get; set; }

        public List<KpiEnvelopeDto> Kpis { get; set; } = new List<KpiEnvelopeDto>();
    }

    public class KpiEnvelopeDto
    {
        public string KpiId { get; set; }

        public string Category { get; set; }

        public string DisplayName { get; set; }

        public decimal? Value { get; set; }

        public string TextValue { get; set; }

        public string FormattedValue { get; set; }

        public string Unit { get; set; }

        public string Direction { get; set; }

        public string PeriodLabel { get; set; }

        public string EvidenceRoute { get; set; }

        public string FilterDimension { get; set; }

        public string ValueType { get; set; }

        public int DisplayPrecision { get; set; }

        public bool TrendEligible { get; set; }

        public bool RankEligible { get; set; }

        public string NullableBehavior { get; set; }
    }

    public class ComparisonMetricDto
    {
        public string KpiId { get; set; }

        public string DisplayName { get; set; }

        public string Unit { get; set; }

        public string Direction { get; set; }

        public decimal? CurrentValue { get; set; }

        public string CurrentFormatted { get; set; }

        public string CurrentPeriodLabel { get; set; }

        public decimal? PriorMonthValue { get; set; }

        public string PriorMonthFormatted { get; set; }

        public string PriorMonthPeriodLabel { get; set; }

        public decimal? MomDelta { get; set; }

        public decimal? MomGrowthPercent { get; set; }

        public decimal? PriorYearValue { get; set; }

        public string PriorYearFormatted { get; set; }

        public string PriorYearPeriodLabel { get; set; }

        public decimal? YoyDelta { get; set; }

        public decimal? YoyGrowthPercent { get; set; }
    }

    public class ProfileComparisonSectionDto : ProfileSectionDtoBase
    {
        public List<ComparisonMetricDto> Metrics { get; set; } = new List<ComparisonMetricDto>();
    }

    public class ProfileTrendSectionDto : ProfileSectionDtoBase
    {
        public List<ProfileTrendSeriesDto> Series { get; set; } = new List<ProfileTrendSeriesDto>();
    }

    public class ProfileTrendSeriesDto
    {
        public string KpiId { get; set; }

        public string DisplayName { get; set; }

        public string PeriodSemantics { get; set; }

        public string Unit { get; set; }

        public List<ProfileTrendPointDto> Points { get; set; } = new List<ProfileTrendPointDto>();
    }

    public class ProfileTrendPointDto
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal? Value { get; set; }

        public bool IsClosed { get; set; }

        public string PeriodLabel { get; set; }
    }

    public class ProfileRadarSectionDto : ProfileSectionDtoBase
    {
        public string PeerGroupRuleId { get; set; }

        public int? PeerGroupSize { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string PeriodLabel { get; set; }

        public string UnavailableExplanation { get; set; }

        public List<ProfileRadarAxisDto> Axes { get; set; } = new List<ProfileRadarAxisDto>();

        public List<decimal?> PeerAverageScores { get; set; }
    }

    public class ProfileRadarAxisDto
    {
        public string KpiId { get; set; }

        public string SignatureDimensionKey { get; set; }

        public string DisplayName { get; set; }

        public decimal? Score { get; set; }

        public string Direction { get; set; }
    }

    public class ProfileRankingSectionDto : ProfileSectionDtoBase
    {
        public List<ProfileRankingSeriesDto> Series { get; set; } = new List<ProfileRankingSeriesDto>();
    }

    public class ProfileRankingSeriesDto
    {
        public string KpiId { get; set; }

        public string DisplayName { get; set; }

        /// <summary>HigherIsBetter · LowerIsBetter — sourced from KPI metadata Direction.</summary>
        public string RankingDirection { get; set; }

        public string Unit { get; set; }

        public int? CurrentRank { get; set; }

        public int? BestRank { get; set; }

        public int? WorstRank { get; set; }

        public decimal? CurrentPercentile { get; set; }

        public int? CurrentPopulationSize { get; set; }

        public List<ProfileRankingPointDto> Points { get; set; } = new List<ProfileRankingPointDto>();
    }

    public class ProfileRankingPointDto
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string PeriodLabel { get; set; }

        public int RankPosition { get; set; }

        public int PopulationSize { get; set; }

        public decimal Percentile { get; set; }
    }

    public class ProfileAttentionSectionDto : ProfileSectionDtoBase
    {
        public int ActiveSignalCount { get; set; }

        public int HistoricalSignalCount { get; set; }

        public List<ProfileAttentionEventDto> Events { get; set; } = new List<ProfileAttentionEventDto>();
    }

    public class ProfileAttentionEventDto
    {
        public string SignalCode { get; set; }

        public string SignalLabel { get; set; }

        public string SignalCategory { get; set; }

        public bool IsActive { get; set; }

        public int? FirstSeenPeriodYear { get; set; }

        public int? FirstSeenPeriodMonth { get; set; }

        public int? LastSeenPeriodYear { get; set; }

        public int? LastSeenPeriodMonth { get; set; }

        public string FirstSeen { get; set; }

        public string LastSeen { get; set; }

        public int ConsecutivePeriods { get; set; }

        public int TotalOccurrences { get; set; }
    }

    public class ProfileRelatedEntitiesSectionDto : ProfileSectionDtoBase
    {
        public List<ProfileRelationshipBlockDto> Blocks { get; set; } = new List<ProfileRelationshipBlockDto>();
    }

    public class ProfileRelationshipBlockDto
    {
        public string RelationshipCode { get; set; }

        public string RelationshipLabel { get; set; }

        public string DisplayName { get; set; }

        public string TargetEntityType { get; set; }

        public List<ProfileRelatedEntityRowDto> Rows { get; set; } = new List<ProfileRelatedEntityRowDto>();
    }

    public class ProfileRelatedEntityRowDto
    {
        public int Rank { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public string TargetEntityType { get; set; }

        public string TargetEntityCode { get; set; }

        public string TargetEntityName { get; set; }

        public decimal? MetricValue { get; set; }

        public string ProfileRoute { get; set; }
    }

    public class ProfileEvidenceSectionDto : ProfileSectionDtoBase
    {
        public List<ProfileEvidenceLinkDto> Links { get; set; } = new List<ProfileEvidenceLinkDto>();
    }

    public class ProfileEvidenceLinkDto
    {
        public string Category { get; set; }

        public string Label { get; set; }

        public string ReportRoute { get; set; }

        public string FilterDimension { get; set; }
    }

    public class GetEntityPerformanceProfileHandler
        : IRequestHandler<GetEntityPerformanceProfileQuery, EntityPerformanceProfileResponse>
    {
        private readonly IEntityAnalyticsService _service;

        public GetEntityPerformanceProfileHandler(IEntityAnalyticsService service)
        {
            _service = service;
        }

        public Task<EntityPerformanceProfileResponse> Handle(
            GetEntityPerformanceProfileQuery request,
            CancellationToken cancellationToken)
        {
            var result = _service.GetProfile(request.EntityType, request.EntityId);
            return Task.FromResult(result);
        }
    }
}
