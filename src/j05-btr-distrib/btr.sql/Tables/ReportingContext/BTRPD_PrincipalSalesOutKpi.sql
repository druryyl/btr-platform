CREATE TABLE BTRPD_PrincipalSalesOutKpi
(
    SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_KpiId DEFAULT('PRN-SALES-001'),
    PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_PeriodYear DEFAULT(0),
    PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_PeriodMonth DEFAULT(0),
    GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
