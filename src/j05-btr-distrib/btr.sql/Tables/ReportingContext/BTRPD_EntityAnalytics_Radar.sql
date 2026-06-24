CREATE TABLE BTRPD_EntityAnalytics_Radar
(
    EntityAnalyticsRadarId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_Id DEFAULT(''),
    EntityType             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_EntityType DEFAULT(''),
    EntityId               VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_EntityId DEFAULT(''),
    EntityCode             VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_EntityCode DEFAULT(''),
    PeriodYear             INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeriodYear DEFAULT(0),
    PeriodMonth            INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeriodMonth DEFAULT(0),
    AxisKpiId              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_AxisKpiId DEFAULT(''),
    Score                  DECIMAL(5,2)   NULL,
    PeerGroupRuleId        VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeerGroupRuleId DEFAULT(''),
    PeerGroupSize          INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_PeerGroupSize DEFAULT(0),
    NormalizationMethod    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_NormalizationMethod DEFAULT(''),
    GeneratedAt            DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Radar_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Radar PRIMARY KEY CLUSTERED (EntityAnalyticsRadarId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Radar_Entity_Period_Axis' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Radar'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Radar_Entity_Period_Axis
    ON BTRPD_EntityAnalytics_Radar (EntityType, EntityId, PeriodYear, PeriodMonth, AxisKpiId)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Radar_Entity_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Radar'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Radar_Entity_Period
    ON BTRPD_EntityAnalytics_Radar (EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)
    INCLUDE (AxisKpiId, Score, PeerGroupRuleId, PeerGroupSize, NormalizationMethod, GeneratedAt)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Radar_Type_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Radar'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Radar_Type_Period
    ON BTRPD_EntityAnalytics_Radar (EntityType, PeriodYear, PeriodMonth)
GO
