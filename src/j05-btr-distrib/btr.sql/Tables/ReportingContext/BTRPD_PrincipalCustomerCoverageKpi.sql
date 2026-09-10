CREATE TABLE BTRPD_PrincipalCustomerCoverageKpi
(
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_SnapshotKey DEFAULT('CURRENT'),
    CustomerCoverageKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_CustomerCoverageKpiId DEFAULT('PRN-CUS-002'),
    AsOfDate               DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_AsOfDate DEFAULT('3000-01-01'),
    GeneratedAt            DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalCustomerCoverageKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
