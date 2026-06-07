CREATE TABLE BTR_PortalDashboardPurchasingWeekTrend
(
    PurchasingWeekTrendId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingWeekTrend_PurchasingWeekTrendId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart             DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd               DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingWeekTrend_WeekLabel DEFAULT(''),
    PurchaseAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingWeekTrend_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardPurchasingWeekTrend PRIMARY KEY CLUSTERED (PurchasingWeekTrendId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardPurchasingWeekTrend_SnapshotKey_WeekStart
    ON BTR_PortalDashboardPurchasingWeekTrend (SnapshotKey, WeekStart)
GO
