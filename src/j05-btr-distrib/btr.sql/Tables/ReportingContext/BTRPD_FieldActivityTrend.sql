CREATE TABLE BTRPD_FieldActivityTrend
(
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_FieldActivityTrend_SnapshotKey DEFAULT('CURRENT'),
    TrendDate               DATE          NOT NULL,
    VisitExecutionPercent   DECIMAL(9,4)  NULL,
    EffectiveCallRate       DECIMAL(9,4)  NULL,
    OrdersCount             INT           NOT NULL CONSTRAINT DF_BTRPD_FieldActivityTrend_OrdersCount DEFAULT(0),
    OmzetAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_FieldActivityTrend_OmzetAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_FieldActivityTrend PRIMARY KEY CLUSTERED (SnapshotKey, TrendDate)
)
GO
