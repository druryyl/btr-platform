CREATE TABLE BTRPD_CustomerPrincipalRelationshipKpi
(
    SnapshotKey          VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_SnapshotKey DEFAULT('CURRENT'),
    KpiId                VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_KpiId DEFAULT('PRN-SALES-001'),
    AsOfDate             DATETIME     NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_AsOfDate DEFAULT('3000-01-01'),
    HistoricalLimitation VARCHAR(300) NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_HistoricalLimitation DEFAULT(''),
    GeneratedAt          DATETIME     NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId     VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerPrincipalRelationshipKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
