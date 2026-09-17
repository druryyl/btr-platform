CREATE TABLE BTRPD_PrincipalTargetKpi
(
    SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_KpiId DEFAULT('PRN-TGT-001'),
    PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_PeriodYear DEFAULT(0),
    PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_PeriodMonth DEFAULT(0),
    GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalTargetKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
