CREATE TABLE BTRPD_SalesmanRepHistory
(
    SalesmanRepHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesmanRepHistoryId DEFAULT(''),
    PeriodYear           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_PeriodYear DEFAULT(0),
    PeriodMonth          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_PeriodMonth DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonName DEFAULT(''),
    TargetAmount         DECIMAL(18,2) NULL,
    CompletedOmzet       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_CompletedOmzet DEFAULT(0),
    AchievementPercent   DECIMAL(9,4)  NULL,
    AchievementBand      VARCHAR(20)   NULL,
    OpenBalance          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_OpenBalance DEFAULT(0),
    IsActive             BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_IsActive DEFAULT(0),
    LastRefreshLogId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_LastRefreshLogId DEFAULT(''),
    UpdatedAt            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_UpdatedAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_SalesmanRepHistory PRIMARY KEY CLUSTERED (SalesmanRepHistoryId),
    CONSTRAINT UX_BTRPD_SalesmanRepHistory_PeriodYear_PeriodMonth_SalesPersonId UNIQUE (PeriodYear, PeriodMonth, SalesPersonId)
)
GO

CREATE INDEX IX_BTRPD_SalesmanRepHistory_SalesPersonId
    ON BTRPD_SalesmanRepHistory (SalesPersonId, PeriodYear DESC, PeriodMonth DESC)
GO
