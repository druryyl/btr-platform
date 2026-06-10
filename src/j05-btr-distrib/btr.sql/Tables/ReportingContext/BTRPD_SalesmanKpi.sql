CREATE TABLE BTRPD_SalesmanKpi
(
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_PeriodMonth DEFAULT(0),
    TotalTeamOmzet              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_TotalTeamOmzet DEFAULT(0),
    TotalPiutang                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_TotalPiutang DEFAULT(0),
    ActiveSalesmanCount         INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_ActiveSalesmanCount DEFAULT(0),
    BelowTargetCount            INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_BelowTargetCount DEFAULT(0),
    MissingTargetSetupCount     INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_MissingTargetSetupCount DEFAULT(0),
    HighOverdueExposureCount    INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_HighOverdueExposureCount DEFAULT(0),
    HighPiutangExposureCount    INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_HighPiutangExposureCount DEFAULT(0),
    CustomerConcentrationCount  INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_CustomerConcentrationCount DEFAULT(0),
    DormantPortfolioCount       INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_DormantPortfolioCount DEFAULT(0),
    TopOmzetSalesmanPercent     DECIMAL(9,4)  NULL,
    TopPiutangSalesmanPercent   DECIMAL(9,4)  NULL,
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesmanKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
