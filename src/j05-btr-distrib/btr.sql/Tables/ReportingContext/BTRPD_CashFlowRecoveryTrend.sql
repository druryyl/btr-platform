CREATE TABLE BTRPD_CashFlowRecoveryTrend
(
    CashFlowRecoveryTrendId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CashFlowRecoveryTrendId DEFAULT(''),
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_SnapshotKey DEFAULT('CURRENT'),
    TrendDate               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_TrendDate DEFAULT('3000-01-01'),
    DayOfMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_DayOfMonth DEFAULT(0),
    IsElapsed               BIT           NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_IsElapsed DEFAULT(0),
    CumulativeCollections   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CumulativeCollections DEFAULT(0),
    CumulativeBilling       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CashFlowRecoveryTrend_CumulativeBilling DEFAULT(0),

    CONSTRAINT PK_BTRPD_CashFlowRecoveryTrend PRIMARY KEY CLUSTERED (CashFlowRecoveryTrendId)
)
GO

CREATE INDEX IX_BTRPD_CashFlowRecoveryTrend_SnapshotKey_TrendDate
    ON BTRPD_CashFlowRecoveryTrend (SnapshotKey, TrendDate)
GO
