CREATE TABLE BTRPD_SalesWeekTrend
(
    SalesWeekTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_SalesWeekTrendId DEFAULT(''),
    SnapshotKey      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd          DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel        VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_WeekLabel DEFAULT(''),
    RecognizedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesWeekTrend_RecognizedAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesWeekTrend PRIMARY KEY CLUSTERED (SalesWeekTrendId)
)
GO

CREATE INDEX IX_BTRPD_SalesWeekTrend_SnapshotKey_WeekStart
    ON BTRPD_SalesWeekTrend (SnapshotKey, WeekStart)
GO
