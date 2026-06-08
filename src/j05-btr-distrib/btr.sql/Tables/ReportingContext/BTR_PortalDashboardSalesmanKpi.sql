CREATE TABLE BTR_PortalDashboardSalesmanKpi
(
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_PeriodMonth DEFAULT(0),
    TotalTeamOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_TotalTeamOmzet DEFAULT(0),
    TotalPiutang                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_TotalPiutang DEFAULT(0),
    ActiveSalesmanCount         INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_ActiveSalesmanCount DEFAULT(0),
    BelowTargetCount            INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_BelowTargetCount DEFAULT(0),
    NoTargetCount               INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_NoTargetCount DEFAULT(0),
    HighOverdueExposureCount    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_HighOverdueExposureCount DEFAULT(0),
    HighPiutangExposureCount    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_HighPiutangExposureCount DEFAULT(0),
    CustomerConcentrationCount  INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_CustomerConcentrationCount DEFAULT(0),
    DormantPortfolioCount       INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_DormantPortfolioCount DEFAULT(0),
    TopOmzetSalesmanPercent     DECIMAL(9,4)  NULL,
    TopPiutangSalesmanPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId            VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardSalesmanKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
