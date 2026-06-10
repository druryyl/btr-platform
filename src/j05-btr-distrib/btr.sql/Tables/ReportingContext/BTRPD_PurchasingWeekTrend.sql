CREATE TABLE BTRPD_PurchasingWeekTrend
(
    PurchasingWeekTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_PurchasingWeekTrendId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_SnapshotKey DEFAULT('CURRENT'),
    WeekStart             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekStart DEFAULT('3000-01-01'),
    WeekEnd               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekEnd DEFAULT('3000-01-01'),
    WeekLabel             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_WeekLabel DEFAULT(''),
    PurchaseAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingWeekTrend_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingWeekTrend PRIMARY KEY CLUSTERED (PurchasingWeekTrendId)
)
GO

CREATE INDEX IX_BTRPD_PurchasingWeekTrend_SnapshotKey_WeekStart
    ON BTRPD_PurchasingWeekTrend (SnapshotKey, WeekStart)
GO
