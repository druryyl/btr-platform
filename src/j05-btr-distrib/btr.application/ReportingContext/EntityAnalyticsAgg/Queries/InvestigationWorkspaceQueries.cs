using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Queries
{
    public class PopulationMapRequest
    {
        public string EntityType { get; set; }

        public string PresetId { get; set; }

        public string DimensionFilter { get; set; }

        public bool? AttentionOnly { get; set; }
    }

    public class PopulationMapResponseDto
    {
        public string EntityType { get; set; }

        public string PresetId { get; set; }

        public string PresetDisplayName { get; set; }

        public string AxisXKpiId { get; set; }

        public string AxisYKpiId { get; set; }

        public string AxisXLabel { get; set; }

        public string AxisYLabel { get; set; }

        public string AxisXUnit { get; set; }

        public string AxisYUnit { get; set; }

        public int TotalPopulationCount { get; set; }

        public int FilteredPopulationCount { get; set; }

        public string ActiveFilterDescription { get; set; }

        public DateTime? GeneratedAt { get; set; }

        public List<PopulationMapPointDto> Points { get; set; } = new List<PopulationMapPointDto>();
    }

    public class PopulationMapPointDto
    {
        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string DisplayName { get; set; }

        public decimal? AxisX { get; set; }

        public decimal? AxisY { get; set; }

        public string FormattedAxisX { get; set; }

        public string FormattedAxisY { get; set; }

        public decimal? AxisXPercentile { get; set; }

        public decimal? AxisYPercentile { get; set; }

        public string DimensionValue { get; set; }

        public bool IsActive { get; set; }

        public int ActiveAttentionCount { get; set; }

        public bool MatchesFilter { get; set; }
    }

    public class PeerDistributionRequest
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string KpiId { get; set; }

        public string DimensionFilter { get; set; }
    }

    public class PeerDistributionResponseDto
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string KpiId { get; set; }

        public string KpiDisplayName { get; set; }

        public string Unit { get; set; }

        public int PeerGroupSize { get; set; }

        public string PeerGroupRuleId { get; set; }

        public decimal? SelectedValue { get; set; }

        public string FormattedSelectedValue { get; set; }

        public decimal? SelectedPercentile { get; set; }

        public decimal? PeerMin { get; set; }

        public decimal? PeerMax { get; set; }

        public string FormattedPeerRange { get; set; }

        public List<PeerDistributionBinDto> Bins { get; set; } = new List<PeerDistributionBinDto>();
    }

    public class PeerDistributionBinDto
    {
        public int BinIndex { get; set; }

        public decimal BinStart { get; set; }

        public decimal BinEnd { get; set; }

        public int Count { get; set; }

        public string Label { get; set; }
    }

    public class MapPresetDto
    {
        public string PresetId { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string AxisXKpiId { get; set; }

        public string AxisYKpiId { get; set; }

        public string AxisXLabel { get; set; }

        public string AxisYLabel { get; set; }

        public bool IsDefault { get; set; }
    }
}
