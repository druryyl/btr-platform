CREATE TABLE BTRPD_EntityAnalytics_Monthly
(
    EntityAnalyticsMonthlyId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_Id DEFAULT(''),
    EntityType               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_EntityType DEFAULT(''),
    EntityId                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_EntityId DEFAULT(''),
    EntityCode               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_EntityCode DEFAULT(''),
    PeriodYear               INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_PeriodYear DEFAULT(0),
    PeriodMonth              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_PeriodMonth DEFAULT(0),
    KpiId                    VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_KpiId DEFAULT(''),
    NumericValue             DECIMAL(18,4)  NULL,
    TextValue                VARCHAR(200)   NULL,
    PeriodSemantics          VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_PeriodSemantics DEFAULT(''),
    DefinitionVersion        INT            NULL,
    IsClosed                 BIT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_IsClosed DEFAULT(0),
    GeneratedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Monthly_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Monthly PRIMARY KEY CLUSTERED (EntityAnalyticsMonthlyId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Monthly_Entity_Period_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Monthly'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Monthly_Entity_Period_Kpi
    ON BTRPD_EntityAnalytics_Monthly (EntityType, EntityId, PeriodYear, PeriodMonth, KpiId)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Monthly_Entity_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Monthly'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Monthly_Entity_Period
    ON BTRPD_EntityAnalytics_Monthly (EntityType, EntityId, PeriodYear DESC, PeriodMonth DESC)
    INCLUDE (KpiId, NumericValue, TextValue, PeriodSemantics, IsClosed, GeneratedAt)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Monthly_Type_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Monthly'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Monthly_Type_Period
    ON BTRPD_EntityAnalytics_Monthly (EntityType, PeriodYear, PeriodMonth)
GO
