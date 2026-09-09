CREATE TABLE BTRPD_PrincipalReturnPercentageKpi
(
    SnapshotKey             VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_SnapshotKey DEFAULT('CURRENT'),
    ReturnPercentageKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_ReturnPercentageKpiId DEFAULT('PRN-RET-004'),
    SalesOutKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    TotalReturnKpiId        VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_PeriodYear DEFAULT(0),
    PeriodMonth             INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_PeriodMonth DEFAULT(0),
    GeneratedAt             DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId        VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentageKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnPercentageKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
