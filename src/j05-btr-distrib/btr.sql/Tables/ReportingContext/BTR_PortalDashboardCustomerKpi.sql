CREATE TABLE BTR_PortalDashboardCustomerKpi
(
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_PeriodYear DEFAULT(0),
    PeriodMonth               INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_PeriodMonth DEFAULT(0),
    TotalOmzet                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_TotalOmzet DEFAULT(0),
    TotalPiutang              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_TotalPiutang DEFAULT(0),
    ActiveCustomerCount       INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_ActiveCustomerCount DEFAULT(0),
    DormantCustomerCount      INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_DormantCustomerCount DEFAULT(0),
    OverdueCustomerCount      INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_OverdueCustomerCount DEFAULT(0),
    PlafondBreachCount        INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_PlafondBreachCount DEFAULT(0),
    SuspendedWithSalesCount   INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_SuspendedWithSalesCount DEFAULT(0),
    AgingOver90Amount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_AgingOver90Amount DEFAULT(0),
    TopOmzetCustomerPercent   DECIMAL(9,4)  NULL,
    TopPiutangCustomerPercent DECIMAL(9,4)  NULL,
    LastRefreshLogId          VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardCustomerKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
