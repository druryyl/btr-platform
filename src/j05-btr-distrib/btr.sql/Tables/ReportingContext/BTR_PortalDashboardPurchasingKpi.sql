CREATE TABLE BTR_PortalDashboardPurchasingKpi
(
    SnapshotKey                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                DATETIME       NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                 INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_PeriodYear DEFAULT(0),
    PeriodMonth                INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_PeriodMonth DEFAULT(0),
    GrandTotalPurchase         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_GrandTotalPurchase DEFAULT(0),
    TotalInvoice               INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_TotalInvoice DEFAULT(0),
    PendingPostingInvoiceCount INT            NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_PendingPostingInvoiceCount DEFAULT(0),
    LastRefreshLogId           VARCHAR(13)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardPurchasingKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
