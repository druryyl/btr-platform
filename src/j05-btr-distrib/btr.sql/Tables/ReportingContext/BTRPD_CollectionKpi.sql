CREATE TABLE BTRPD_CollectionKpi
(
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                   DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                    INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PeriodYear DEFAULT(0),
    PeriodMonth                   INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PeriodMonth DEFAULT(0),
    OverdueExposure               DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_OverdueExposure DEFAULT(0),
    AgingOver90Exposure           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_AgingOver90Exposure DEFAULT(0),
    OverdueConcentrationPercent   DECIMAL(9,4)  NULL,
    CashCollectedMtd              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_CashCollectedMtd DEFAULT(0),
    MonthCollections              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_MonthCollections DEFAULT(0),
    MonthFakturOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_MonthFakturOmzet DEFAULT(0),
    RecoveryVsBillingPercent      DECIMAL(9,4)  NULL,
    PaymentMixCashAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixCashAmount DEFAULT(0),
    PaymentMixGiroAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixGiroAmount DEFAULT(0),
    PaymentMixAdjustmentAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_PaymentMixAdjustmentAmount DEFAULT(0),
    PaymentMixCashPercent         DECIMAL(9,4)  NULL,
    PaymentMixGiroPercent         DECIMAL(9,4)  NULL,
    PaymentMixAdjustmentPercent   DECIMAL(9,4)  NULL,
    LegacyDebtCount               INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LegacyDebtCount DEFAULT(0),
    ChronicOverdueCount           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_ChronicOverdueCount DEFAULT(0),
    WilayahHotspotCount           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_WilayahHotspotCount DEFAULT(0),
    LowRecoveryVsBillingCount     INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LowRecoveryVsBillingCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
