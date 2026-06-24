CREATE TABLE BTRPD_SalesDailyPace
(
    SalesDailyPaceId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_SalesDailyPaceId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_SnapshotKey DEFAULT('CURRENT'),
    PaceDate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_PaceDate DEFAULT('3000-01-01'),
    DayOfMonth           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_DayOfMonth DEFAULT(0),
    IsElapsed            BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_IsElapsed DEFAULT(0),
    ActualAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_ActualAmount DEFAULT(0),
    ProjectedDailyAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesDailyPace_ProjectedDailyAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesDailyPace PRIMARY KEY CLUSTERED (SalesDailyPaceId)
)
GO

CREATE INDEX IX_BTRPD_SalesDailyPace_SnapshotKey_PaceDate
    ON BTRPD_SalesDailyPace (SnapshotKey, PaceDate)
GO
