CREATE TABLE BTRPD_PrincipalReturnKpi
(
    SnapshotKey        VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_SnapshotKey DEFAULT('CURRENT'),
    GoodReturnKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear         INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_PeriodYear DEFAULT(0),
    PeriodMonth        INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_PeriodMonth DEFAULT(0),
    GeneratedAt        DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId   VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
