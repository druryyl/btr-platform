CREATE TABLE BTRPD_PrincipalPurchaseInKpi
(
    SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_KpiId DEFAULT('PRN-PUR-001'),
    PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_PeriodYear DEFAULT(0),
    PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_PeriodMonth DEFAULT(0),
    GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalPurchaseInKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
