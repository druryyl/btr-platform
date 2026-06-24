CREATE TABLE BTRPD_EntityAnalytics_Ranking
(
    EntityAnalyticsRankingId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_Id DEFAULT(''),
    EntityType               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_EntityType DEFAULT(''),
    EntityId                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_EntityId DEFAULT(''),
    EntityCode               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_EntityCode DEFAULT(''),
    PeriodYear               INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_PeriodYear DEFAULT(0),
    PeriodMonth              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_PeriodMonth DEFAULT(0),
    KpiId                    VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_KpiId DEFAULT(''),
    RankPosition             INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_RankPosition DEFAULT(0),
    PopulationSize           INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_PopulationSize DEFAULT(0),
    Percentile               DECIMAL(5,2)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_Percentile DEFAULT(0),
    GeneratedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Ranking_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Ranking PRIMARY KEY CLUSTERED (EntityAnalyticsRankingId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Ranking_Entity_Period_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Ranking'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Ranking_Entity_Period_Kpi
    ON BTRPD_EntityAnalytics_Ranking (EntityType, EntityId, PeriodYear, PeriodMonth, KpiId)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Ranking_Entity_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Ranking'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Ranking_Entity_Period
    ON BTRPD_EntityAnalytics_Ranking (EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)
    INCLUDE (KpiId, RankPosition, PopulationSize, Percentile, GeneratedAt)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Ranking_Type_Period_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Ranking'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Ranking_Type_Period_Kpi
    ON BTRPD_EntityAnalytics_Ranking (EntityType, PeriodYear, PeriodMonth, KpiId)
GO
