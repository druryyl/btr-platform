CREATE TABLE BTRPD_PrincipalContributionKpi
(
    SnapshotKey          VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_SnapshotKey DEFAULT('CURRENT'),
    SourceSalesOutKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_SourceSalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear           INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_PeriodYear DEFAULT(0),
    PeriodMonth          INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_PeriodMonth DEFAULT(0),
    GeneratedAt          DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId     VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalContributionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
