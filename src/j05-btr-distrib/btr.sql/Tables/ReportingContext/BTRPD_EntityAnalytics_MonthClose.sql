CREATE TABLE BTRPD_EntityAnalytics_MonthClose
(
    EntityAnalyticsMonthCloseId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_Id DEFAULT(''),
    EntityType                  VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_EntityType DEFAULT(''),
    PeriodYear                  INT         NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_PeriodYear DEFAULT(0),
    PeriodMonth                 INT         NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_PeriodMonth DEFAULT(0),
    ClosedAt                    DATETIME    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_ClosedAt DEFAULT('3000-01-01'),
    LastRefreshLogId            VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_MonthClose_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_MonthClose PRIMARY KEY CLUSTERED (EntityAnalyticsMonthCloseId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_MonthClose_Type_Period' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_MonthClose'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_MonthClose_Type_Period
    ON BTRPD_EntityAnalytics_MonthClose (EntityType, PeriodYear, PeriodMonth)
GO
