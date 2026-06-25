using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Contracts;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.EntityAnalyticsAgg
{
    public class EntityAnalyticsRepository : IEntityAnalyticsRepository
    {
        private readonly DatabaseOptions _opt;
        private readonly IDimensionLabelRegistry _dimensionLabels;

        public EntityAnalyticsRepository(
            IOptions<DatabaseOptions> opt,
            IDimensionLabelRegistry dimensionLabels)
        {
            _opt = opt.Value;
            _dimensionLabels = dimensionLabels;
        }

        public IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetrics(string entityType, string entityId)
        {
            return QueryRows(entityType, entityId)
                .Where(row => !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(row.KpiId))
                .ToList();
        }

        public IReadOnlyList<EntityAnalyticsCurrentRow> GetCurrentMetricsBatch(
            string entityType,
            IEnumerable<string> entityIds)
        {
            var idList = (entityIds ?? Array.Empty<string>())
                .Select(id => id?.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (idList.Count == 0 || string.IsNullOrWhiteSpace(entityType))
                return Array.Empty<EntityAnalyticsCurrentRow>();

            const string sql = @"
SELECT EntityAnalyticsCurrentId, SnapshotKey, EntityType, EntityId, EntityCode, KpiId,
       NumericValue, TextValue, DefinitionVersion, GeneratedAt, UpdatedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Current
WHERE SnapshotKey = @SnapshotKey
  AND EntityType = @EntityType
  AND (EntityId IN @EntityIds OR EntityCode IN @EntityIds)";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsCurrentRow>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    EntityIds = idList
                })
                .Where(row => !EntityAnalyticsMetaKpiIds.IsMetaOrDimension(row.KpiId))
                .ToList();
            }
        }

        public IReadOnlyList<EntityIdentity> SearchEntities(string entityType, string searchText, int top)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(searchText))
                return Array.Empty<EntityIdentity>();

            var pattern = "%" + searchText.Trim() + "%";
            const string sql = @"
SELECT TOP (@Top)
       c.EntityType,
       c.EntityId,
       c.EntityCode,
       MAX(CASE WHEN c.KpiId = @DisplayNameKpiId THEN c.TextValue END) AS DisplayNameText,
       MAX(CASE WHEN c.KpiId = @IsActiveKpiId THEN c.NumericValue END) AS IsActiveNumeric
FROM BTRPD_EntityAnalytics_Current c
WHERE c.SnapshotKey = @SnapshotKey
  AND c.EntityType = @EntityType
  AND (
        c.EntityCode LIKE @Pattern
        OR c.EntityId LIKE @Pattern
        OR EXISTS (
            SELECT 1
            FROM BTRPD_EntityAnalytics_Current d
            WHERE d.SnapshotKey = c.SnapshotKey
              AND d.EntityType = c.EntityType
              AND d.EntityId = c.EntityId
              AND d.KpiId = @DisplayNameKpiId
              AND d.TextValue LIKE @Pattern
        )
      )
GROUP BY c.EntityType, c.EntityId, c.EntityCode
ORDER BY c.EntityCode";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var rows = conn.Query(sql, new
                {
                    Top = top,
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    Pattern = pattern,
                    DisplayNameKpiId = EntityAnalyticsMetaKpiIds.DisplayName,
                    IsActiveKpiId = EntityAnalyticsMetaKpiIds.IsActive
                }).ToList();

                return rows.Select(row =>
                {
                    var entityCode = (string)row.EntityCode ?? (string)row.EntityId;
                    var displayName = (string)row.DisplayNameText;
                    if (string.IsNullOrWhiteSpace(displayName))
                        displayName = entityCode;

                    decimal? isActiveNumeric = row.IsActiveNumeric;
                    var isActive = !isActiveNumeric.HasValue || isActiveNumeric.Value > 0m;

                    return new EntityIdentity
                    {
                        EntityType = entityType,
                        EntityId = entityCode,
                        EntityCode = entityCode,
                        DisplayName = displayName,
                        IsActive = isActive
                    };
                }).ToList();
            }
        }

        public EntityIdentity TryResolveIdentity(string entityType, string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return null;

            var rows = QueryRows(entityType, entityId);
            if (rows.Count == 0)
            {
                return new EntityIdentity
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityCode = entityId,
                    DisplayName = entityId,
                    IsActive = true
                };
            }

            var entityCode = rows
                .Select(r => r.EntityCode)
                .FirstOrDefault(code => !string.IsNullOrWhiteSpace(code)) ?? entityId;

            var displayName = ReadMetaText(rows, EntityAnalyticsMetaKpiIds.DisplayName) ?? entityCode;
            var isActiveNumeric = ReadMetaNumeric(rows, EntityAnalyticsMetaKpiIds.IsActive);
            var isActive = isActiveNumeric.HasValue ? isActiveNumeric.Value > 0m : true;

            var dimensions = new Dictionary<string, string>();
            foreach (var row in rows.Where(r => r.KpiId != null && r.KpiId.StartsWith(EntityAnalyticsMetaKpiIds.DimPrefix)))
            {
                var label = _dimensionLabels.TryGetLabel(entityType, row.KpiId, out var friendly)
                    ? friendly
                    : row.KpiId.Substring(EntityAnalyticsMetaKpiIds.DimPrefix.Length);

                var value = !string.IsNullOrWhiteSpace(row.TextValue)
                    ? row.TextValue
                    : row.NumericValue?.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                    dimensions[label] = value;
            }

            return new EntityIdentity
            {
                EntityType = entityType,
                EntityId = rows.Select(r => r.EntityId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id)) ?? entityId,
                EntityCode = entityCode,
                DisplayName = displayName,
                IsActive = isActive,
                Dimensions = dimensions
            };
        }

        public void ReplaceCurrentMetrics(string entityType, IEnumerable<EntityAnalyticsCurrentRow> rows, string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsCurrentRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Current
WHERE SnapshotKey = @SnapshotKey AND EntityType = @EntityType";

                    conn.Execute(deleteSql, new
                    {
                        SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                        EntityType = entityType
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Current
    (EntityAnalyticsCurrentId, SnapshotKey, EntityType, EntityId, EntityCode, KpiId,
     NumericValue, TextValue, DefinitionVersion, GeneratedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsCurrentId, @SnapshotKey, @EntityType, @EntityId, @EntityCode, @KpiId,
     @NumericValue, @TextValue, @DefinitionVersion, @GeneratedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsCurrentId = string.IsNullOrWhiteSpace(row.EntityAnalyticsCurrentId)
                                    ? Ulid.NewUlid().ToString()
                                    : row.EntityAnalyticsCurrentId,
                                SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                                EntityType = entityType,
                                row.EntityId,
                                row.EntityCode,
                                row.KpiId,
                                row.NumericValue,
                                row.TextValue,
                                DefinitionVersion = row.DefinitionVersion ?? 1,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public DateTime? GetLatestGeneratedAt(string entityType, string entityId)
        {
            const string sql = @"
SELECT MAX(GeneratedAt)
FROM BTRPD_EntityAnalytics_Current
WHERE SnapshotKey = @SnapshotKey
  AND EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QueryFirstOrDefault<DateTime?>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    EntityId = entityId
                });
            }
        }

        public bool HasAnyCurrentMetrics(string entityType)
        {
            // Any L0 row means the dashboard worker produced a snapshot for this entity type.
            const string sql = @"
SELECT TOP 1 1
FROM BTRPD_EntityAnalytics_Current
WHERE SnapshotKey = @SnapshotKey
  AND EntityType = @EntityType";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QueryFirstOrDefault<int?>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType
                }) == 1;
            }
        }

        public IReadOnlyList<EntityAnalyticsMonthlyRow> GetMonthlyMetrics(
            string entityType,
            string entityId,
            int periodYear,
            int periodMonth)
        {
            return GetHistory(entityType, entityId, periodYear, periodMonth, periodYear, periodMonth);
        }

        public IReadOnlyList<EntityAnalyticsMonthlyRow> GetMonthlyRange(
            string entityType,
            string entityId,
            int fromYear,
            int fromMonth,
            int toYear,
            int toMonth)
        {
            return GetHistory(entityType, entityId, fromYear, fromMonth, toYear, toMonth);
        }

        public IReadOnlyList<EntityAnalyticsMonthlyRow> GetHistory(
            string entityType,
            string entityId,
            int? fromYear = null,
            int? fromMonth = null,
            int? toYear = null,
            int? toMonth = null)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return Array.Empty<EntityAnalyticsMonthlyRow>();

            var sql = @"
SELECT EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth, KpiId,
       NumericValue, TextValue, PeriodSemantics, DefinitionVersion, IsClosed,
       GeneratedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Monthly
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)";

            var parameters = new DynamicParameters();
            parameters.Add("EntityType", entityType);
            parameters.Add("EntityId", entityId);

            if (fromYear.HasValue && fromMonth.HasValue)
            {
                sql += " AND (PeriodYear > @FromYear OR (PeriodYear = @FromYear AND PeriodMonth >= @FromMonth))";
                parameters.Add("FromYear", fromYear.Value);
                parameters.Add("FromMonth", fromMonth.Value);
            }

            if (toYear.HasValue && toMonth.HasValue)
            {
                sql += " AND (PeriodYear < @ToYear OR (PeriodYear = @ToYear AND PeriodMonth <= @ToMonth))";
                parameters.Add("ToYear", toYear.Value);
                parameters.Add("ToMonth", toMonth.Value);
            }

            sql += " ORDER BY PeriodYear, PeriodMonth, KpiId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsMonthlyRow>(sql, parameters).ToList();
            }
        }

        public IReadOnlyList<(int Year, int Month)> GetLatestPeriods(
            string entityType,
            string entityId,
            int maxPeriods)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId) || maxPeriods <= 0)
                return Array.Empty<(int, int)>();

            const string sql = @"
SELECT DISTINCT TOP (@MaxPeriods) PeriodYear, PeriodMonth
FROM BTRPD_EntityAnalytics_Monthly
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
ORDER BY PeriodYear DESC, PeriodMonth DESC";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<(int PeriodYear, int PeriodMonth)>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    MaxPeriods = maxPeriods
                })
                .Select(p => (p.PeriodYear, p.PeriodMonth))
                .OrderBy(p => p.PeriodYear)
                .ThenBy(p => p.PeriodMonth)
                .ToList();
            }
        }

        public void SaveMonthlyHistory(
            string entityType,
            IEnumerable<EntityAnalyticsMonthlyRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsMonthlyRow>();
            if (rowList.Count == 0)
                return;

            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string mergeSql = @"
MERGE BTRPD_EntityAnalytics_Monthly AS target
USING (SELECT @EntityAnalyticsMonthlyId AS EntityAnalyticsMonthlyId,
              @EntityType AS EntityType,
              @EntityId AS EntityId,
              @EntityCode AS EntityCode,
              @PeriodYear AS PeriodYear,
              @PeriodMonth AS PeriodMonth,
              @KpiId AS KpiId,
              @NumericValue AS NumericValue,
              @TextValue AS TextValue,
              @PeriodSemantics AS PeriodSemantics,
              @DefinitionVersion AS DefinitionVersion,
              @IsClosed AS IsClosed,
              @GeneratedAt AS GeneratedAt,
              @UpdatedAt AS UpdatedAt,
              @LastRefreshLogId AS LastRefreshLogId) AS source
ON target.EntityType = source.EntityType
   AND target.EntityId = source.EntityId
   AND target.PeriodYear = source.PeriodYear
   AND target.PeriodMonth = source.PeriodMonth
   AND target.KpiId = source.KpiId
WHEN MATCHED AND target.IsClosed = 0 THEN
    UPDATE SET NumericValue = source.NumericValue,
               TextValue = source.TextValue,
               PeriodSemantics = source.PeriodSemantics,
               DefinitionVersion = source.DefinitionVersion,
               GeneratedAt = source.GeneratedAt,
               UpdatedAt = source.UpdatedAt,
               LastRefreshLogId = source.LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (EntityAnalyticsMonthlyId, EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth,
            KpiId, NumericValue, TextValue, PeriodSemantics, DefinitionVersion, IsClosed,
            GeneratedAt, UpdatedAt, LastRefreshLogId)
    VALUES (source.EntityAnalyticsMonthlyId, source.EntityType, source.EntityId, source.EntityCode,
            source.PeriodYear, source.PeriodMonth, source.KpiId, source.NumericValue, source.TextValue,
            source.PeriodSemantics, source.DefinitionVersion, source.IsClosed,
            source.GeneratedAt, source.UpdatedAt, source.LastRefreshLogId);";

                    foreach (var row in rowList)
                    {
                        conn.Execute(mergeSql, new
                        {
                            EntityAnalyticsMonthlyId = Ulid.NewUlid().ToString(),
                            EntityType = entityType,
                            row.EntityId,
                            row.EntityCode,
                            row.PeriodYear,
                            row.PeriodMonth,
                            row.KpiId,
                            row.NumericValue,
                            row.TextValue,
                            PeriodSemantics = row.PeriodSemantics ?? string.Empty,
                            DefinitionVersion = row.DefinitionVersion ?? 1,
                            IsClosed = row.IsClosed,
                            GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                            UpdatedAt = now,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, trans);
                    }

                    trans.Commit();
                }
            }
        }

        public void CloseMonth(string entityType, int periodYear, int periodMonth, string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            if (IsMonthClosed(entityType, periodYear, periodMonth))
                return;

            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string closeRowsSql = @"
UPDATE BTRPD_EntityAnalytics_Monthly
SET IsClosed = 1, UpdatedAt = @UpdatedAt
WHERE EntityType = @EntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth
  AND IsClosed = 0";

                    conn.Execute(closeRowsSql, new
                    {
                        EntityType = entityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth,
                        UpdatedAt = now
                    }, trans);

                    const string insertCloseSql = @"
INSERT INTO BTRPD_EntityAnalytics_MonthClose
    (EntityAnalyticsMonthCloseId, EntityType, PeriodYear, PeriodMonth, ClosedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsMonthCloseId, @EntityType, @PeriodYear, @PeriodMonth, @ClosedAt, @LastRefreshLogId)";

                    conn.Execute(insertCloseSql, new
                    {
                        EntityAnalyticsMonthCloseId = Ulid.NewUlid().ToString(),
                        EntityType = entityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth,
                        ClosedAt = now,
                        LastRefreshLogId = refreshLogId ?? string.Empty
                    }, trans);

                    trans.Commit();
                }
            }
        }

        public bool IsMonthClosed(string entityType, int periodYear, int periodMonth)
        {
            const string sql = @"
SELECT TOP 1 1
FROM BTRPD_EntityAnalytics_MonthClose
WHERE EntityType = @EntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QueryFirstOrDefault<int?>(sql, new
                {
                    EntityType = entityType,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                }) == 1;
            }
        }

        public void PurgeHistoryOlderThan(string entityType, int retentionMonths)
        {
            if (string.IsNullOrWhiteSpace(entityType) || retentionMonths <= 0)
                return;

            var cutoff = DateTime.Now.AddMonths(-retentionMonths);
            var cutoffYear = cutoff.Year;
            var cutoffMonth = cutoff.Month;

            const string sql = @"
DELETE FROM BTRPD_EntityAnalytics_Monthly
WHERE EntityType = @EntityType
  AND (PeriodYear < @CutoffYear OR (PeriodYear = @CutoffYear AND PeriodMonth < @CutoffMonth));

DELETE FROM BTRPD_EntityAnalytics_Ranking
WHERE EntityType = @EntityType
  AND (PeriodYear < @CutoffYear OR (PeriodYear = @CutoffYear AND PeriodMonth < @CutoffMonth));

DELETE FROM BTRPD_EntityAnalytics_Radar
WHERE EntityType = @EntityType
  AND (PeriodYear < @CutoffYear OR (PeriodYear = @CutoffYear AND PeriodMonth < @CutoffMonth));";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Execute(sql, new
                {
                    EntityType = entityType,
                    CutoffYear = cutoffYear,
                    CutoffMonth = cutoffMonth
                });
            }
        }

        public void SaveRankingHistory(
            string entityType,
            IEnumerable<EntityAnalyticsRankingRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsRankingRow>();
            if (rowList.Count == 0)
                return;

            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string mergeSql = @"
MERGE BTRPD_EntityAnalytics_Ranking AS target
USING (SELECT @EntityAnalyticsRankingId AS EntityAnalyticsRankingId,
              @EntityType AS EntityType,
              @EntityId AS EntityId,
              @EntityCode AS EntityCode,
              @PeriodYear AS PeriodYear,
              @PeriodMonth AS PeriodMonth,
              @KpiId AS KpiId,
              @RankPosition AS RankPosition,
              @PopulationSize AS PopulationSize,
              @Percentile AS Percentile,
              @GeneratedAt AS GeneratedAt,
              @UpdatedAt AS UpdatedAt,
              @LastRefreshLogId AS LastRefreshLogId) AS source
ON target.EntityType = source.EntityType
   AND target.EntityId = source.EntityId
   AND target.PeriodYear = source.PeriodYear
   AND target.PeriodMonth = source.PeriodMonth
   AND target.KpiId = source.KpiId
WHEN MATCHED THEN
    UPDATE SET RankPosition = source.RankPosition,
               PopulationSize = source.PopulationSize,
               Percentile = source.Percentile,
               GeneratedAt = source.GeneratedAt,
               UpdatedAt = source.UpdatedAt,
               LastRefreshLogId = source.LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (EntityAnalyticsRankingId, EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth,
            KpiId, RankPosition, PopulationSize, Percentile, GeneratedAt, UpdatedAt, LastRefreshLogId)
    VALUES (source.EntityAnalyticsRankingId, source.EntityType, source.EntityId, source.EntityCode,
            source.PeriodYear, source.PeriodMonth, source.KpiId, source.RankPosition, source.PopulationSize,
            source.Percentile, source.GeneratedAt, source.UpdatedAt, source.LastRefreshLogId);";

                    foreach (var row in rowList)
                    {
                        conn.Execute(mergeSql, new
                        {
                            EntityAnalyticsRankingId = Ulid.NewUlid().ToString(),
                            EntityType = entityType,
                            row.EntityId,
                            EntityCode = row.EntityCode ?? row.EntityId,
                            row.PeriodYear,
                            row.PeriodMonth,
                            KpiId = row.RankMetricKpiId,
                            row.RankPosition,
                            row.PopulationSize,
                            row.Percentile,
                            GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                            UpdatedAt = now,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, trans);
                    }

                    trans.Commit();
                }
            }
        }

        public IReadOnlyList<EntityAnalyticsRankingRow> GetRankingHistory(
            string entityType,
            string entityId,
            string rankMetricKpiId,
            int? fromYear = null,
            int? fromMonth = null,
            int? toYear = null,
            int? toMonth = null)
        {
            if (string.IsNullOrWhiteSpace(entityType)
                || string.IsNullOrWhiteSpace(entityId)
                || string.IsNullOrWhiteSpace(rankMetricKpiId))
            {
                return Array.Empty<EntityAnalyticsRankingRow>();
            }

            const string sql = @"
SELECT EntityType, EntityId, EntityCode, KpiId AS RankMetricKpiId,
       PeriodYear, PeriodMonth, RankPosition, PopulationSize, Percentile,
       GeneratedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Ranking
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
  AND KpiId = @KpiId
  AND (@FromYear IS NULL OR PeriodYear > @FromYear OR (PeriodYear = @FromYear AND PeriodMonth >= @FromMonth))
  AND (@ToYear IS NULL OR PeriodYear < @ToYear OR (PeriodYear = @ToYear AND PeriodMonth <= @ToMonth))
ORDER BY PeriodYear, PeriodMonth";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsRankingRow>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    KpiId = rankMetricKpiId,
                    FromYear = fromYear,
                    FromMonth = fromMonth,
                    ToYear = toYear,
                    ToMonth = toMonth
                }).ToList();
            }
        }

        public EntityAnalyticsRankingRow GetLatestRanking(
            string entityType,
            string entityId,
            string rankMetricKpiId)
        {
            if (string.IsNullOrWhiteSpace(entityType)
                || string.IsNullOrWhiteSpace(entityId)
                || string.IsNullOrWhiteSpace(rankMetricKpiId))
            {
                return null;
            }

            const string sql = @"
SELECT TOP 1 EntityType, EntityId, EntityCode, KpiId AS RankMetricKpiId,
       PeriodYear, PeriodMonth, RankPosition, PopulationSize, Percentile,
       GeneratedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Ranking
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
  AND KpiId = @KpiId
ORDER BY PeriodYear DESC, PeriodMonth DESC";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.QueryFirstOrDefault<EntityAnalyticsRankingRow>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    KpiId = rankMetricKpiId
                });
            }
        }

        public IReadOnlyList<(int Year, int Month)> GetRankingPeriods(
            string entityType,
            string entityId,
            string rankMetricKpiId,
            int maxPeriods)
        {
            if (string.IsNullOrWhiteSpace(entityType)
                || string.IsNullOrWhiteSpace(entityId)
                || string.IsNullOrWhiteSpace(rankMetricKpiId)
                || maxPeriods <= 0)
            {
                return Array.Empty<(int, int)>();
            }

            const string sql = @"
SELECT DISTINCT TOP (@MaxPeriods) PeriodYear, PeriodMonth
FROM BTRPD_EntityAnalytics_Ranking
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
  AND KpiId = @KpiId
ORDER BY PeriodYear DESC, PeriodMonth DESC";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<(int PeriodYear, int PeriodMonth)>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    KpiId = rankMetricKpiId,
                    MaxPeriods = maxPeriods
                })
                .Select(p => (p.PeriodYear, p.PeriodMonth))
                .OrderBy(p => p.PeriodYear)
                .ThenBy(p => p.PeriodMonth)
                .ToList();
            }
        }

        public IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetPeriodMetricsForPopulation(
            string entityType,
            int periodYear,
            int periodMonth,
            string kpiId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(kpiId))
                return Array.Empty<EntityAnalyticsPeriodMetricRow>();

            const string sql = @"
SELECT m.EntityId,
       m.EntityCode,
       m.NumericValue,
       CASE WHEN COALESCE(active.NumericValue, 1) > 0 THEN 1 ELSE 0 END AS IsActive
FROM BTRPD_EntityAnalytics_Monthly m
LEFT JOIN BTRPD_EntityAnalytics_Current active
    ON active.EntityType = m.EntityType
   AND active.EntityId = m.EntityId
   AND active.KpiId = @IsActiveKpiId
   AND active.SnapshotKey = @SnapshotKey
WHERE m.EntityType = @EntityType
  AND m.PeriodYear = @PeriodYear
  AND m.PeriodMonth = @PeriodMonth
  AND m.KpiId = @KpiId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsPeriodMetricRow>(sql, new
                {
                    EntityType = entityType,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth,
                    KpiId = kpiId,
                    IsActiveKpiId = EntityAnalyticsMetaKpiIds.IsActive,
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey
                }).ToList();
            }
        }

        public IReadOnlyList<EntityAnalyticsAttentionEventRow> GetAttentionEvents(string entityType, string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return Array.Empty<EntityAnalyticsAttentionEventRow>();

            const string sql = @"
SELECT EntityAnalyticsAttentionId, EntityType, EntityId, EntityCode, SignalCode, SignalCategory, SignalTitle,
       FirstSeenYear AS FirstSeenPeriodYear, FirstSeenMonth AS FirstSeenPeriodMonth,
       LastSeenYear AS LastSeenPeriodYear, LastSeenMonth AS LastSeenPeriodMonth,
       ConsecutivePeriods, TotalOccurrences, IsActive, GeneratedAt, CreatedAt, UpdatedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Attention
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
ORDER BY IsActive DESC, LastSeenYear DESC, LastSeenMonth DESC, SignalTitle";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsAttentionEventRow>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId
                }).ToList();
            }
        }

        public void SaveAttentionRecords(
            string entityType,
            IEnumerable<EntityAnalyticsAttentionEventRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.Where(r => r != null).ToList() ?? new List<EntityAnalyticsAttentionEventRow>();
            if (rowList.Count == 0)
                return;

            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string mergeSql = @"
MERGE BTRPD_EntityAnalytics_Attention AS target
USING (SELECT @EntityAnalyticsAttentionId AS EntityAnalyticsAttentionId,
              @EntityType AS EntityType,
              @EntityId AS EntityId,
              @EntityCode AS EntityCode,
              @SignalCode AS SignalCode,
              @SignalCategory AS SignalCategory,
              @SignalTitle AS SignalTitle,
              @FirstSeenYear AS FirstSeenYear,
              @FirstSeenMonth AS FirstSeenMonth,
              @LastSeenYear AS LastSeenYear,
              @LastSeenMonth AS LastSeenMonth,
              @ConsecutivePeriods AS ConsecutivePeriods,
              @TotalOccurrences AS TotalOccurrences,
              @IsActive AS IsActive,
              @GeneratedAt AS GeneratedAt,
              @CreatedAt AS CreatedAt,
              @UpdatedAt AS UpdatedAt,
              @LastRefreshLogId AS LastRefreshLogId) AS source
ON target.EntityType = source.EntityType
   AND target.EntityId = source.EntityId
   AND target.SignalCode = source.SignalCode
WHEN MATCHED THEN
    UPDATE SET EntityCode = source.EntityCode,
               SignalCategory = source.SignalCategory,
               SignalTitle = source.SignalTitle,
               FirstSeenYear = source.FirstSeenYear,
               FirstSeenMonth = source.FirstSeenMonth,
               LastSeenYear = source.LastSeenYear,
               LastSeenMonth = source.LastSeenMonth,
               ConsecutivePeriods = source.ConsecutivePeriods,
               TotalOccurrences = source.TotalOccurrences,
               IsActive = source.IsActive,
               GeneratedAt = source.GeneratedAt,
               UpdatedAt = source.UpdatedAt,
               LastRefreshLogId = source.LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (EntityAnalyticsAttentionId, EntityType, EntityId, EntityCode, SignalCode, SignalCategory, SignalTitle,
            FirstSeenYear, FirstSeenMonth, LastSeenYear, LastSeenMonth, ConsecutivePeriods, TotalOccurrences,
            IsActive, GeneratedAt, CreatedAt, UpdatedAt, LastRefreshLogId)
    VALUES (source.EntityAnalyticsAttentionId, source.EntityType, source.EntityId, source.EntityCode,
            source.SignalCode, source.SignalCategory, source.SignalTitle, source.FirstSeenYear, source.FirstSeenMonth,
            source.LastSeenYear, source.LastSeenMonth, source.ConsecutivePeriods, source.TotalOccurrences,
            source.IsActive, source.GeneratedAt, source.CreatedAt, source.UpdatedAt, source.LastRefreshLogId);";

                    foreach (var row in rowList)
                    {
                        var createdAt = row.CreatedAt == default ? now : row.CreatedAt;
                        conn.Execute(mergeSql, new
                        {
                            EntityAnalyticsAttentionId = string.IsNullOrWhiteSpace(row.EntityAnalyticsAttentionId)
                                ? Ulid.NewUlid().ToString()
                                : row.EntityAnalyticsAttentionId,
                            EntityType = entityType,
                            row.EntityId,
                            EntityCode = row.EntityCode ?? row.EntityId,
                            row.SignalCode,
                            row.SignalCategory,
                            row.SignalTitle,
                            FirstSeenYear = row.FirstSeenPeriodYear,
                            FirstSeenMonth = row.FirstSeenPeriodMonth,
                            LastSeenYear = row.LastSeenPeriodYear,
                            LastSeenMonth = row.LastSeenPeriodMonth,
                            row.ConsecutivePeriods,
                            row.TotalOccurrences,
                            row.IsActive,
                            GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                            CreatedAt = createdAt,
                            UpdatedAt = now,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, trans);
                    }

                    trans.Commit();
                }
            }
        }

        public IReadOnlyList<EntityAnalyticsRelationshipRow> GetRelationshipRollups(
            string entityType,
            string entityId,
            string relationshipCode = null)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return Array.Empty<EntityAnalyticsRelationshipRow>();

            const string sql = @"
SELECT SourceEntityType, SourceEntityId, SourceEntityCode, RelationshipCode,
       TargetEntityType, TargetEntityId, TargetEntityCode, TargetDisplayName,
       Rank, MetricValue, PeriodYear, PeriodMonth, GeneratedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Relationship
WHERE SourceEntityType = @EntityType
  AND (SourceEntityId = @EntityId OR SourceEntityCode = @EntityId)
  AND (@RelationshipCode IS NULL OR RelationshipCode = @RelationshipCode)
ORDER BY RelationshipCode, Rank";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsRelationshipRow>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    RelationshipCode = string.IsNullOrWhiteSpace(relationshipCode) ? null : relationshipCode
                }).ToList();
            }
        }

        public void ReplaceRelationshipRollups(
            string sourceEntityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRelationshipRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(sourceEntityType))
                throw new ArgumentException("SourceEntityType is required.", nameof(sourceEntityType));

            if (IsMonthClosed(sourceEntityType, periodYear, periodMonth))
                return;

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsRelationshipRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Relationship
WHERE SourceEntityType = @SourceEntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

                    conn.Execute(deleteSql, new
                    {
                        SourceEntityType = sourceEntityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Relationship
    (EntityAnalyticsRelationshipId, SourceEntityType, SourceEntityId, SourceEntityCode,
     RelationshipCode, TargetEntityType, TargetEntityId, TargetEntityCode, TargetDisplayName,
     Rank, MetricValue, PeriodYear, PeriodMonth, GeneratedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsRelationshipId, @SourceEntityType, @SourceEntityId, @SourceEntityCode,
     @RelationshipCode, @TargetEntityType, @TargetEntityId, @TargetEntityCode, @TargetDisplayName,
     @Rank, @MetricValue, @PeriodYear, @PeriodMonth, @GeneratedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsRelationshipId = Ulid.NewUlid().ToString(),
                                SourceEntityType = sourceEntityType,
                                row.SourceEntityId,
                                SourceEntityCode = row.SourceEntityCode ?? row.SourceEntityId,
                                row.RelationshipCode,
                                row.TargetEntityType,
                                row.TargetEntityId,
                                TargetEntityCode = row.TargetEntityCode ?? row.TargetEntityId,
                                TargetDisplayName = row.TargetDisplayName ?? row.TargetEntityCode ?? row.TargetEntityId,
                                row.Rank,
                                row.MetricValue,
                                PeriodYear = periodYear,
                                PeriodMonth = periodMonth,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public IReadOnlyList<EntityAnalyticsRadarScoreRow> GetRadarScores(
            string entityType,
            string entityId,
            int periodYear,
            int periodMonth)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
                return Array.Empty<EntityAnalyticsRadarScoreRow>();

            const string sql = @"
SELECT EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth, AxisKpiId, Score,
       PeerGroupRuleId, PeerGroupSize, NormalizationMethod, GeneratedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Radar
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth
ORDER BY AxisKpiId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsRadarScoreRow>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                }).ToList();
            }
        }

        public IReadOnlyList<EntityAnalyticsRadarScoreRow> GetRadarScoresBatch(
            string entityType,
            IEnumerable<string> entityIds,
            int periodYear,
            int periodMonth)
        {
            var idList = (entityIds ?? Array.Empty<string>())
                .Select(id => id?.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (idList.Count == 0 || string.IsNullOrWhiteSpace(entityType))
                return Array.Empty<EntityAnalyticsRadarScoreRow>();

            const string sql = @"
SELECT EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth, AxisKpiId, Score,
       PeerGroupRuleId, PeerGroupSize, NormalizationMethod, GeneratedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Radar
WHERE EntityType = @EntityType
  AND (EntityId IN @EntityIds OR EntityCode IN @EntityIds)
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth
ORDER BY EntityId, AxisKpiId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsRadarScoreRow>(sql, new
                {
                    EntityType = entityType,
                    EntityIds = idList,
                    PeriodYear = periodYear,
                    PeriodMonth = periodMonth
                }).ToList();
            }
        }

        public IReadOnlyList<(int Year, int Month)> GetLatestRadarPeriods(
            string entityType,
            string entityId,
            int maxPeriods)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId) || maxPeriods <= 0)
                return Array.Empty<(int, int)>();

            const string sql = @"
SELECT DISTINCT TOP (@MaxPeriods) PeriodYear, PeriodMonth
FROM BTRPD_EntityAnalytics_Radar
WHERE EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)
ORDER BY PeriodYear DESC, PeriodMonth DESC";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<(int PeriodYear, int PeriodMonth)>(sql, new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    MaxPeriods = maxPeriods
                })
                .Select(p => (p.PeriodYear, p.PeriodMonth))
                .OrderBy(p => p.PeriodYear)
                .ThenBy(p => p.PeriodMonth)
                .ToList();
            }
        }

        public void SaveRadarScores(
            string entityType,
            IEnumerable<EntityAnalyticsRadarScoreRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsRadarScoreRow>();
            if (rowList.Count == 0)
                return;

            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string mergeSql = @"
MERGE BTRPD_EntityAnalytics_Radar AS target
USING (SELECT @EntityAnalyticsRadarId AS EntityAnalyticsRadarId,
              @EntityType AS EntityType,
              @EntityId AS EntityId,
              @EntityCode AS EntityCode,
              @PeriodYear AS PeriodYear,
              @PeriodMonth AS PeriodMonth,
              @AxisKpiId AS AxisKpiId,
              @Score AS Score,
              @PeerGroupRuleId AS PeerGroupRuleId,
              @PeerGroupSize AS PeerGroupSize,
              @NormalizationMethod AS NormalizationMethod,
              @GeneratedAt AS GeneratedAt,
              @UpdatedAt AS UpdatedAt,
              @LastRefreshLogId AS LastRefreshLogId) AS source
ON target.EntityType = source.EntityType
   AND target.EntityId = source.EntityId
   AND target.PeriodYear = source.PeriodYear
   AND target.PeriodMonth = source.PeriodMonth
   AND target.AxisKpiId = source.AxisKpiId
WHEN MATCHED THEN
    UPDATE SET Score = source.Score,
               PeerGroupRuleId = source.PeerGroupRuleId,
               PeerGroupSize = source.PeerGroupSize,
               NormalizationMethod = source.NormalizationMethod,
               GeneratedAt = source.GeneratedAt,
               UpdatedAt = source.UpdatedAt,
               LastRefreshLogId = source.LastRefreshLogId
WHEN NOT MATCHED THEN
    INSERT (EntityAnalyticsRadarId, EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth,
            AxisKpiId, Score, PeerGroupRuleId, PeerGroupSize, NormalizationMethod,
            GeneratedAt, UpdatedAt, LastRefreshLogId)
    VALUES (source.EntityAnalyticsRadarId, source.EntityType, source.EntityId, source.EntityCode,
            source.PeriodYear, source.PeriodMonth, source.AxisKpiId, source.Score,
            source.PeerGroupRuleId, source.PeerGroupSize, source.NormalizationMethod,
            source.GeneratedAt, source.UpdatedAt, source.LastRefreshLogId);";

                    foreach (var row in rowList)
                    {
                        conn.Execute(mergeSql, new
                        {
                            EntityAnalyticsRadarId = Ulid.NewUlid().ToString(),
                            EntityType = entityType,
                            row.EntityId,
                            EntityCode = row.EntityCode ?? row.EntityId,
                            row.PeriodYear,
                            row.PeriodMonth,
                            row.AxisKpiId,
                            row.Score,
                            PeerGroupRuleId = row.PeerGroupRuleId ?? string.Empty,
                            row.PeerGroupSize,
                            NormalizationMethod = row.NormalizationMethod ?? RadarNormalizationMethod.PeerPercentile,
                            GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                            UpdatedAt = now,
                            LastRefreshLogId = refreshLogId ?? string.Empty
                        }, trans);
                    }

                    trans.Commit();
                }
            }
        }

        public void ReplaceMonthlyHistoryForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsMonthlyRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsMonthlyRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Monthly
WHERE EntityType = @EntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

                    conn.Execute(deleteSql, new
                    {
                        EntityType = entityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Monthly
    (EntityAnalyticsMonthlyId, EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth,
     KpiId, NumericValue, TextValue, PeriodSemantics, DefinitionVersion, IsClosed,
     GeneratedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsMonthlyId, @EntityType, @EntityId, @EntityCode, @PeriodYear, @PeriodMonth,
     @KpiId, @NumericValue, @TextValue, @PeriodSemantics, @DefinitionVersion, @IsClosed,
     @GeneratedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsMonthlyId = Ulid.NewUlid().ToString(),
                                EntityType = entityType,
                                row.EntityId,
                                row.EntityCode,
                                PeriodYear = periodYear,
                                PeriodMonth = periodMonth,
                                row.KpiId,
                                row.NumericValue,
                                row.TextValue,
                                PeriodSemantics = row.PeriodSemantics ?? string.Empty,
                                DefinitionVersion = row.DefinitionVersion ?? 1,
                                IsClosed = true,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public void ReplaceRankingForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRankingRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsRankingRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Ranking
WHERE EntityType = @EntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

                    conn.Execute(deleteSql, new
                    {
                        EntityType = entityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Ranking
    (EntityAnalyticsRankingId, EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth,
     KpiId, RankPosition, PopulationSize, Percentile, GeneratedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsRankingId, @EntityType, @EntityId, @EntityCode, @PeriodYear, @PeriodMonth,
     @KpiId, @RankPosition, @PopulationSize, @Percentile, @GeneratedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsRankingId = Ulid.NewUlid().ToString(),
                                EntityType = entityType,
                                row.EntityId,
                                EntityCode = row.EntityCode ?? row.EntityId,
                                PeriodYear = periodYear,
                                PeriodMonth = periodMonth,
                                KpiId = row.RankMetricKpiId,
                                row.RankPosition,
                                row.PopulationSize,
                                row.Percentile,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public void ReplaceAttentionForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsAttentionEventRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.Where(r => r != null).ToList() ?? new List<EntityAnalyticsAttentionEventRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Attention
WHERE EntityType = @EntityType
  AND LastSeenYear = @PeriodYear
  AND LastSeenMonth = @PeriodMonth";

                    conn.Execute(deleteSql, new
                    {
                        EntityType = entityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Attention
    (EntityAnalyticsAttentionId, EntityType, EntityId, EntityCode, SignalCode, SignalCategory, SignalTitle,
     FirstSeenYear, FirstSeenMonth, LastSeenYear, LastSeenMonth, ConsecutivePeriods, TotalOccurrences,
     IsActive, GeneratedAt, CreatedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsAttentionId, @EntityType, @EntityId, @EntityCode, @SignalCode, @SignalCategory, @SignalTitle,
     @FirstSeenYear, @FirstSeenMonth, @LastSeenYear, @LastSeenMonth, @ConsecutivePeriods, @TotalOccurrences,
     @IsActive, @GeneratedAt, @CreatedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            var createdAt = row.CreatedAt == default ? now : row.CreatedAt;
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsAttentionId = string.IsNullOrWhiteSpace(row.EntityAnalyticsAttentionId)
                                    ? Ulid.NewUlid().ToString()
                                    : row.EntityAnalyticsAttentionId,
                                EntityType = entityType,
                                row.EntityId,
                                EntityCode = row.EntityCode ?? row.EntityId,
                                row.SignalCode,
                                SignalCategory = row.SignalCategory ?? string.Empty,
                                SignalTitle = row.SignalTitle ?? row.SignalCode,
                                FirstSeenYear = row.FirstSeenPeriodYear,
                                FirstSeenMonth = row.FirstSeenPeriodMonth,
                                LastSeenYear = row.LastSeenPeriodYear,
                                LastSeenMonth = row.LastSeenPeriodMonth,
                                row.ConsecutivePeriods,
                                row.TotalOccurrences,
                                IsActive = row.IsActive ? 1 : 0,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                CreatedAt = createdAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public void ReplaceRelationshipForPeriod(
            string sourceEntityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRelationshipRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(sourceEntityType))
                throw new ArgumentException("SourceEntityType is required.", nameof(sourceEntityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsRelationshipRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Relationship
WHERE SourceEntityType = @SourceEntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

                    conn.Execute(deleteSql, new
                    {
                        SourceEntityType = sourceEntityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Relationship
    (EntityAnalyticsRelationshipId, SourceEntityType, SourceEntityId, SourceEntityCode,
     RelationshipCode, TargetEntityType, TargetEntityId, TargetEntityCode, TargetDisplayName,
     Rank, MetricValue, PeriodYear, PeriodMonth, GeneratedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsRelationshipId, @SourceEntityType, @SourceEntityId, @SourceEntityCode,
     @RelationshipCode, @TargetEntityType, @TargetEntityId, @TargetEntityCode, @TargetDisplayName,
     @Rank, @MetricValue, @PeriodYear, @PeriodMonth, @GeneratedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsRelationshipId = Ulid.NewUlid().ToString(),
                                SourceEntityType = sourceEntityType,
                                row.SourceEntityId,
                                SourceEntityCode = row.SourceEntityCode ?? row.SourceEntityId,
                                row.RelationshipCode,
                                row.TargetEntityType,
                                row.TargetEntityId,
                                TargetEntityCode = row.TargetEntityCode ?? row.TargetEntityId,
                                TargetDisplayName = row.TargetDisplayName ?? row.TargetEntityCode ?? row.TargetEntityId,
                                row.Rank,
                                row.MetricValue,
                                PeriodYear = periodYear,
                                PeriodMonth = periodMonth,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public void ReplaceRadarForPeriod(
            string entityType,
            int periodYear,
            int periodMonth,
            IEnumerable<EntityAnalyticsRadarScoreRow> rows,
            string refreshLogId)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("EntityType is required.", nameof(entityType));

            var rowList = rows?.ToList() ?? new List<EntityAnalyticsRadarScoreRow>();
            var now = DateTime.Now;

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    const string deleteSql = @"
DELETE FROM BTRPD_EntityAnalytics_Radar
WHERE EntityType = @EntityType
  AND PeriodYear = @PeriodYear
  AND PeriodMonth = @PeriodMonth";

                    conn.Execute(deleteSql, new
                    {
                        EntityType = entityType,
                        PeriodYear = periodYear,
                        PeriodMonth = periodMonth
                    }, trans);

                    if (rowList.Count > 0)
                    {
                        const string insertSql = @"
INSERT INTO BTRPD_EntityAnalytics_Radar
    (EntityAnalyticsRadarId, EntityType, EntityId, EntityCode, PeriodYear, PeriodMonth,
     AxisKpiId, Score, PeerGroupRuleId, PeerGroupSize, NormalizationMethod,
     GeneratedAt, UpdatedAt, LastRefreshLogId)
VALUES
    (@EntityAnalyticsRadarId, @EntityType, @EntityId, @EntityCode, @PeriodYear, @PeriodMonth,
     @AxisKpiId, @Score, @PeerGroupRuleId, @PeerGroupSize, @NormalizationMethod,
     @GeneratedAt, @UpdatedAt, @LastRefreshLogId)";

                        foreach (var row in rowList)
                        {
                            conn.Execute(insertSql, new
                            {
                                EntityAnalyticsRadarId = Ulid.NewUlid().ToString(),
                                EntityType = entityType,
                                row.EntityId,
                                EntityCode = row.EntityCode ?? row.EntityId,
                                PeriodYear = periodYear,
                                PeriodMonth = periodMonth,
                                row.AxisKpiId,
                                row.Score,
                                PeerGroupRuleId = row.PeerGroupRuleId ?? string.Empty,
                                row.PeerGroupSize,
                                NormalizationMethod = row.NormalizationMethod ?? RadarNormalizationMethod.PeerPercentile,
                                GeneratedAt = row.GeneratedAt == default ? now : row.GeneratedAt,
                                UpdatedAt = now,
                                LastRefreshLogId = refreshLogId ?? string.Empty
                            }, trans);
                        }
                    }

                    trans.Commit();
                }
            }
        }

        public IReadOnlyList<EntityPopulationRow> GetActivePopulation(string entityType, string dimensionKpiId = null)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                return Array.Empty<EntityPopulationRow>();

            var sql = string.IsNullOrWhiteSpace(dimensionKpiId)
                ? @"
SELECT c.EntityId,
       c.EntityCode,
       CASE WHEN COALESCE(active.NumericValue, 1) > 0 THEN 1 ELSE 0 END AS IsActive,
       NULL AS DimensionValue
FROM (
    SELECT DISTINCT EntityId, EntityCode
    FROM BTRPD_EntityAnalytics_Current
    WHERE SnapshotKey = @SnapshotKey AND EntityType = @EntityType
) c
LEFT JOIN BTRPD_EntityAnalytics_Current active
    ON active.SnapshotKey = @SnapshotKey
   AND active.EntityType = @EntityType
   AND active.EntityId = c.EntityId
   AND active.KpiId = @IsActiveKpiId
WHERE COALESCE(active.NumericValue, 1) > 0"
                : @"
SELECT c.EntityId,
       c.EntityCode,
       CASE WHEN COALESCE(active.NumericValue, 1) > 0 THEN 1 ELSE 0 END AS IsActive,
       dim.TextValue AS DimensionValue
FROM (
    SELECT DISTINCT EntityId, EntityCode, EntityType
    FROM BTRPD_EntityAnalytics_Current
    WHERE SnapshotKey = @SnapshotKey AND EntityType = @EntityType
) c
LEFT JOIN BTRPD_EntityAnalytics_Current active
    ON active.SnapshotKey = @SnapshotKey
   AND active.EntityType = c.EntityType
   AND active.EntityId = c.EntityId
   AND active.KpiId = @IsActiveKpiId
LEFT JOIN BTRPD_EntityAnalytics_Current dim
    ON dim.SnapshotKey = @SnapshotKey
   AND dim.EntityType = c.EntityType
   AND dim.EntityId = c.EntityId
   AND dim.KpiId = @DimensionKpiId
WHERE COALESCE(active.NumericValue, 1) > 0";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityPopulationRow>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    IsActiveKpiId = EntityAnalyticsMetaKpiIds.IsActive,
                    DimensionKpiId = dimensionKpiId
                }).ToList();
            }
        }

        public IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentKpiPopulation(string entityType, string kpiId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(kpiId))
                return Array.Empty<EntityAnalyticsPeriodMetricRow>();

            const string sql = @"
SELECT c.EntityId,
       c.EntityCode,
       c.NumericValue,
       CASE WHEN COALESCE(active.NumericValue, 1) > 0 THEN 1 ELSE 0 END AS IsActive
FROM BTRPD_EntityAnalytics_Current c
LEFT JOIN BTRPD_EntityAnalytics_Current active
    ON active.SnapshotKey = c.SnapshotKey
   AND active.EntityType = c.EntityType
   AND active.EntityId = c.EntityId
   AND active.KpiId = @IsActiveKpiId
WHERE c.SnapshotKey = @SnapshotKey
  AND c.EntityType = @EntityType
  AND c.KpiId = @KpiId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsPeriodMetricRow>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    KpiId = kpiId,
                    IsActiveKpiId = EntityAnalyticsMetaKpiIds.IsActive
                }).ToList();
            }
        }

        public IReadOnlyDictionary<string, int> GetActiveAttentionCounts(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            const string sql = @"
SELECT EntityId, COUNT(*) AS ActiveCount
FROM BTRPD_EntityAnalytics_Attention
WHERE EntityType = @EntityType
  AND IsActive = 1
GROUP BY EntityId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query(sql, new { EntityType = entityType })
                    .ToDictionary(
                        row => (string)row.EntityId,
                        row => (int)row.ActiveCount,
                        StringComparer.OrdinalIgnoreCase);
            }
        }

        public IReadOnlyList<EntityAnalyticsPeriodMetricRow> GetCurrentDimensionPopulation(
            string entityType,
            string dimensionKpiId)
        {
            if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(dimensionKpiId))
                return Array.Empty<EntityAnalyticsPeriodMetricRow>();

            const string sql = @"
SELECT c.EntityId,
       c.EntityCode,
       TRY_CAST(c.TextValue AS DECIMAL(18,4)) AS NumericValue,
       CASE WHEN COALESCE(active.NumericValue, 1) > 0 THEN 1 ELSE 0 END AS IsActive
FROM BTRPD_EntityAnalytics_Current c
LEFT JOIN BTRPD_EntityAnalytics_Current active
    ON active.SnapshotKey = c.SnapshotKey
   AND active.EntityType = c.EntityType
   AND active.EntityId = c.EntityId
   AND active.KpiId = @IsActiveKpiId
WHERE c.SnapshotKey = @SnapshotKey
  AND c.EntityType = @EntityType
  AND c.KpiId = @DimensionKpiId";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsPeriodMetricRow>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    DimensionKpiId = dimensionKpiId,
                    IsActiveKpiId = EntityAnalyticsMetaKpiIds.IsActive
                }).ToList();
            }
        }

        private List<EntityAnalyticsCurrentRow> QueryRows(string entityType, string entityId)
        {
            const string sql = @"
SELECT EntityAnalyticsCurrentId, SnapshotKey, EntityType, EntityId, EntityCode, KpiId,
       NumericValue, TextValue, DefinitionVersion, GeneratedAt, UpdatedAt, LastRefreshLogId
FROM BTRPD_EntityAnalytics_Current
WHERE SnapshotKey = @SnapshotKey
  AND EntityType = @EntityType
  AND (EntityId = @EntityId OR EntityCode = @EntityId)";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                return conn.Query<EntityAnalyticsCurrentRow>(sql, new
                {
                    SnapshotKey = EntityAnalyticsConstants.CurrentSnapshotKey,
                    EntityType = entityType,
                    EntityId = entityId
                }).ToList();
            }
        }

        private static string ReadMetaText(IEnumerable<EntityAnalyticsCurrentRow> rows, string kpiId)
        {
            return rows.FirstOrDefault(r => string.Equals(r.KpiId, kpiId, StringComparison.OrdinalIgnoreCase))?.TextValue;
        }

        private static decimal? ReadMetaNumeric(IEnumerable<EntityAnalyticsCurrentRow> rows, string kpiId)
        {
            return rows.FirstOrDefault(r => string.Equals(r.KpiId, kpiId, StringComparison.OrdinalIgnoreCase))?.NumericValue;
        }
    }
}
