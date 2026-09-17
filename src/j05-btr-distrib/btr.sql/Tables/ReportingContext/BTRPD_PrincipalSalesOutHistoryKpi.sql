CREATE TABLE BTRPD_PrincipalSalesOutHistoryKpi
(
    SnapshotKey           VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_SnapshotKey DEFAULT('HISTORY'),
    KpiId                 VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_KpiId DEFAULT('PRN-SALES-001'),
    HistoricalLimitation  VARCHAR(300) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_HistoricalLimitation DEFAULT(''),
    GeneratedAt           DATETIME     NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId      VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutHistoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
