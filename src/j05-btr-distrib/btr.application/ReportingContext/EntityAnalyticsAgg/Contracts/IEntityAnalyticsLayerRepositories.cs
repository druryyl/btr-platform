using System;

using System.Collections.Generic;

using btr.application.ReportingContext.EntityAnalyticsAgg.Models;

using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;



namespace btr.application.ReportingContext.EntityAnalyticsAgg.Contracts

{

    /// <summary>L0 CURRENT snapshot reads and writes.</summary>

    public interface IEntityAnalyticsCurrentRepository

    {

        IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId);

        IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetricsBatch(
            string entityType,
            IEnumerable<string> entityIds);

        IReadOnlyList<EntityIdentity> SearchEntities(string entityType, string searchText, int top);



        void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId);



        DateTime? GetLatestGeneratedAt(string entityType, string entityId);

        DateTime? GetLatestGeneratedAtForEntityType(string entityType);

        bool HasAnyCurrentMetrics(string entityType);

        IReadOnlyList<EntityPopulationRow> GetActivePopulation(string entityType, string dimensionKpiId = null);

        IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentKpiPopulation(string entityType, string kpiId);

        IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentDimensionPopulation(string entityType, string dimensionKpiId);

        IReadOnlyDictionary<string, int> GetActiveAttentionCounts(string entityType);

    }



    /// <summary>L1 monthly history (M32.3+).</summary>

    public interface IEntityAnalyticsMonthlyRepository

    {

        IReadOnlyList<EntityAnalyticsMonthlyRow> GetHistory(

            string entityType,

            string entityId,

            int? fromYear = null,

            int? fromMonth = null,

            int? toYear = null,

            int? toMonth = null);



        IReadOnlyList<(int Year, int Month)> GetLatestPeriods(

            string entityType,

            string entityId,

            int maxPeriods);



        IReadOnlyList<EntityAnalyticsMonthlyRow> GetMonthlyMetrics(

            string entityType,

            string entityId,

            int periodYear,

            int periodMonth);



        IReadOnlyList<EntityAnalyticsMonthlyRow> GetMonthlyRange(

            string entityType,

            string entityId,

            int fromYear,

            int fromMonth,

            int toYear,

            int toMonth);



        void SaveMonthlyHistory(

            string entityType,

            IEnumerable<EntityAnalyticsMonthlyRow> rows,

            string refreshLogId);



        void CloseMonth(string entityType, int periodYear, int periodMonth, string refreshLogId);



        bool IsMonthClosed(string entityType, int periodYear, int periodMonth);

        IReadOnlyList<EntityPopulationRow> GetPeriodActivePopulation(
            string entityType,
            int periodYear,
            int periodMonth,
            string dimensionKpiId = null);

        void PurgeHistoryOlderThan(string entityType, int retentionMonths);

    }



    /// <summary>L2 ranking history (M32.4+).</summary>

    public interface IEntityAnalyticsRankingRepository

    {

        void SaveRankingHistory(

            string entityType,

            IEnumerable<EntityAnalyticsRankingRow> rows,

            string refreshLogId);



        IReadOnlyList<EntityAnalyticsRankingRow> GetRankingHistory(

            string entityType,

            string entityId,

            string rankMetricKpiId,

            int? fromYear = null,

            int? fromMonth = null,

            int? toYear = null,

            int? toMonth = null);



        EntityAnalyticsRankingRow GetLatestRanking(

            string entityType,

            string entityId,

            string rankMetricKpiId);



        IReadOnlyList<(int Year, int Month)> GetRankingPeriods(

            string entityType,

            string entityId,

            string rankMetricKpiId,

            int maxPeriods);



        IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetPeriodMetricsForPopulation(

            string entityType,

            int periodYear,

            int periodMonth,

            string kpiId);

    }



    /// <summary>L3 attention event log seam (M32.5+).</summary>

    public interface IEntityAnalyticsAttentionRepository

    {

        void SaveAttentionRecords(

            string entityType,

            IEnumerable<EntityAnalyticsAttentionEventRow> rows,

            string refreshLogId);



        IReadOnlyList<EntityAnalyticsAttentionEventRow> GetAttentionEvents(string entityType, string entityId);

    }



    /// <summary>L4 relationship rollup seam (M32.6+).</summary>

    public interface IEntityAnalyticsRelationshipRepository

    {

        IReadOnlyList<EntityAnalyticsRelationshipRow> GetRelationshipRollups(

            string entityType,

            string entityId,

            string relationshipCode = null);



        void ReplaceRelationshipRollups(

            string sourceEntityType,

            int periodYear,

            int periodMonth,

            IEnumerable<EntityAnalyticsRelationshipRow> rows,

            string refreshLogId);

    }



    /// <summary>L5 radar score seam (M32.8).</summary>

    public interface IEntityAnalyticsRadarRepository

    {

        void SaveRadarScores(

            string entityType,

            IEnumerable<EntityAnalyticsRadarScoreRow> rows,

            string refreshLogId);

        IReadOnlyList<EntityAnalyticsRadarScoreRow> GetRadarScores(

            string entityType,

            string entityId,

            int periodYear,

            int periodMonth);

        IReadOnlyList<EntityAnalyticsRadarScoreRow> GetRadarScoresBatch(

            string entityType,

            IEnumerable<string> entityIds,

            int periodYear,

            int periodMonth);

        IReadOnlyList<(int Year, int Month)> GetLatestRadarPeriods(

            string entityType,

            string entityId,

            int maxPeriods);

    }

}


