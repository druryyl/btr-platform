CREATE TABLE BTR_PortalDashboardPiutangKpi
(
    SnapshotKey      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt      DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangKpi_GeneratedAt DEFAULT('3000-01-01'),
    TotalPiutang     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangKpi_TotalPiutang DEFAULT(0),
    TotalCustomer    INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangKpi_TotalCustomer DEFAULT(0),
    OverdueCustomer  INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangKpi_OverdueCustomer DEFAULT(0),
    LastRefreshLogId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardPiutangKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
