CREATE TABLE BTRPD_PrincipalReturnHistoryKpi
(
    SnapshotKey         VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_SnapshotKey DEFAULT('HISTORY'),
    GoodReturnKpiId     VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    GeneratedAt         DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId    VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistoryKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnHistoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
