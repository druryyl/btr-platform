CREATE TABLE BTRPD_PrincipalMomGrowthKpi
(
    SnapshotKey              VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_SnapshotKey DEFAULT('CURRENT'),
    MomGrowthKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_MomGrowthKpiId DEFAULT('PRN-GRW-001'),
    SalesOutKpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PeriodYear DEFAULT(0),
    PeriodMonth              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PeriodMonth DEFAULT(0),
    PriorYear                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PriorYear DEFAULT(0),
    PriorMonth               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PriorMonth DEFAULT(0),
    GeneratedAt              DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalMomGrowthKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
