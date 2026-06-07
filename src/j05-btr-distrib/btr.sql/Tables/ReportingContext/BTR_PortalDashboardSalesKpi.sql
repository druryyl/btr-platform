CREATE TABLE BTR_PortalDashboardSalesKpi
(
    SnapshotKey        VARCHAR(10)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt        DATETIME       NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear         INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_PeriodYear DEFAULT(0),
    PeriodMonth        INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_PeriodMonth DEFAULT(0),
    TotalOmzet         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_TotalOmzet DEFAULT(0),
    TotalFaktur        INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_TotalFaktur DEFAULT(0),
    TotalCustomer      INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_TotalCustomer DEFAULT(0),
    TotalTarget        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_TotalTarget DEFAULT(0),
    TotalAchievement   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_TotalAchievement DEFAULT(0),
    AchievementPercent DECIMAL(9,4)   NULL,
    CompletedOmzet     DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_CompletedOmzet DEFAULT(0),
    PipelineOmzet      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_PipelineOmzet DEFAULT(0),
    LastRefreshLogId   VARCHAR(13)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardSalesKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
