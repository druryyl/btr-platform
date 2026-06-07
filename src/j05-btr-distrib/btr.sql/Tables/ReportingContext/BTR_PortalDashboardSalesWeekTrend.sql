CREATE TABLE BTR_PortalDashboardSalesWeekTrend
(
    SalesWeekTrendId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesWeekTrend_SalesWeekTrendId DEFAULT(''),
    SnapshotKey      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart        DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd          DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel        VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesWeekTrend_WeekLabel DEFAULT(''),
    RecognizedAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesWeekTrend_RecognizedAmount DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardSalesWeekTrend PRIMARY KEY CLUSTERED (SalesWeekTrendId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardSalesWeekTrend_SnapshotKey_WeekStart
    ON BTR_PortalDashboardSalesWeekTrend (SnapshotKey, WeekStart)
GO
