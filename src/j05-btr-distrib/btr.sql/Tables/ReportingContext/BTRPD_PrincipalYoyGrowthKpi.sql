CREATE TABLE BTRPD_PrincipalYoyGrowthKpi
(
    SnapshotKey              VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_SnapshotKey DEFAULT('CURRENT'),
    YoyGrowthKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_YoyGrowthKpiId DEFAULT('PRN-GRW-002'),
    SalesOutKpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PeriodYear DEFAULT(0),
    PeriodMonth              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PeriodMonth DEFAULT(0),
    PriorYear                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PriorYear DEFAULT(0),
    PriorMonth               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PriorMonth DEFAULT(0),
    GeneratedAt              DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalYoyGrowthKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
