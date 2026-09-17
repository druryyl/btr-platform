CREATE TABLE BTRPD_PrincipalActiveCustomerKpi
(
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_SnapshotKey DEFAULT('CURRENT'),
    ActiveCustomerKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_ActiveCustomerKpiId DEFAULT('PRN-CUS-001'),
    AsOfDate               DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_AsOfDate DEFAULT('3000-01-01'),
    GeneratedAt            DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalActiveCustomerKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
