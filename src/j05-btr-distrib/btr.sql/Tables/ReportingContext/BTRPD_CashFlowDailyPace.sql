CREATE TABLE BTRPD_CashFlowDailyPace
(
    CashFlowDailyPaceId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_CashFlowDailyPaceId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_SnapshotKey DEFAULT('CURRENT'),
    PaceDate                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_PaceDate DEFAULT('3000-01-01'),
    DayOfMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_DayOfMonth DEFAULT(0),
    IsElapsed                BIT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_IsElapsed DEFAULT(0),
    ActualCashAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ActualCashAmount DEFAULT(0),
    ActualCollectionAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ActualCollectionAmount DEFAULT(0),
    ProjectedDailyCashAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowDailyPace_ProjectedDailyCashAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_CashFlowDailyPace PRIMARY KEY CLUSTERED (CashFlowDailyPaceId)
)
GO

CREATE INDEX IX_BTRPD_CashFlowDailyPace_SnapshotKey_PaceDate
    ON BTRPD_CashFlowDailyPace (SnapshotKey, PaceDate)
GO
