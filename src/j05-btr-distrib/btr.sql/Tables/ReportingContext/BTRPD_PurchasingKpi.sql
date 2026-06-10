CREATE TABLE BTRPD_PurchasingKpi
(
    SnapshotKey                VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                 INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PeriodYear DEFAULT(0),
    PeriodMonth                INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PeriodMonth DEFAULT(0),
    GrandTotalPurchase         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_GrandTotalPurchase DEFAULT(0),
    TotalInvoice               INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_TotalInvoice DEFAULT(0),
    PendingPostingInvoiceCount INT            NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_PendingPostingInvoiceCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_PurchasingKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
