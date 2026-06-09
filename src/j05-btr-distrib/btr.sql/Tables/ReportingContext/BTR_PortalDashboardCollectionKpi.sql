CREATE TABLE BTR_PortalDashboardCollectionKpi
(
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                   DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_PeriodYear DEFAULT(0),
    PeriodMonth                   INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_PeriodMonth DEFAULT(0),
    OverdueExposure               DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_OverdueExposure DEFAULT(0),
    AgingOver90Exposure           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_AgingOver90Exposure DEFAULT(0),
    OverdueConcentrationPercent   DECIMAL(9,4)  NULL,
    CashCollectedMtd              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_CashCollectedMtd DEFAULT(0),
    MonthCollections              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_MonthCollections DEFAULT(0),
    MonthFakturOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_MonthFakturOmzet DEFAULT(0),
    RecoveryVsBillingPercent      DECIMAL(9,4)  NULL,
    PaymentMixCashAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_PaymentMixCashAmount DEFAULT(0),
    PaymentMixGiroAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_PaymentMixGiroAmount DEFAULT(0),
    PaymentMixAdjustmentAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_PaymentMixAdjustmentAmount DEFAULT(0),
    PaymentMixCashPercent         DECIMAL(9,4)  NULL,
    PaymentMixGiroPercent         DECIMAL(9,4)  NULL,
    PaymentMixAdjustmentPercent   DECIMAL(9,4)  NULL,
    LegacyDebtCount               INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_LegacyDebtCount DEFAULT(0),
    ChronicOverdueCount           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_ChronicOverdueCount DEFAULT(0),
    WilayahHotspotCount           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_WilayahHotspotCount DEFAULT(0),
    LowRecoveryVsBillingCount     INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_LowRecoveryVsBillingCount DEFAULT(0),
    LastRefreshLogId              VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardCollectionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
