CREATE TABLE BTRPD_PurchasingManagementKpi
(
    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                         INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PeriodYear DEFAULT(0),
    PeriodMonth                        INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PeriodMonth DEFAULT(0),
    QualifiedBacklogCount              INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogCount DEFAULT(0),
    QualifiedBacklogValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogValue DEFAULT(0),
    PendingPostingValue                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PendingPostingValue DEFAULT(0),
    PostedPercent                      DECIMAL(9,4)  NULL,
    Top1PrincipalPercent               DECIMAL(9,4)  NULL,
    Top3PrincipalPercent               DECIMAL(9,4)  NULL,
    Top1SupplierInventoryPercent       DECIMAL(9,4)  NULL,
    CompoundDependencyCount            INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_CompoundDependencyCount DEFAULT(0),
    PrincipalInventoryNoPurchaseCount  INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PrincipalInventoryNoPurchaseCount DEFAULT(0),
    UnknownPrincipalCount              INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_UnknownPrincipalCount DEFAULT(0),
    PurchasingInactivityFlag           BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PurchasingInactivityFlag DEFAULT(0),
    QualifiedBacklogPrincipalCount     INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogPrincipalCount DEFAULT(0),
    PrincipalAtRiskExposureCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_PrincipalAtRiskExposureCount DEFAULT(0),
    LastRefreshLogId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingManagementKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
