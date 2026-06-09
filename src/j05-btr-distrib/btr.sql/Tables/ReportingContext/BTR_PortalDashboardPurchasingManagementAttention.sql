CREATE TABLE BTR_PortalDashboardPurchasingManagementAttention
(
    PurchasingManagementAttentionId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_PurchasingManagementAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType                        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_EntityType DEFAULT(''),
    EntityName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_EntityName DEFAULT(''),
    SignalKey                         VARCHAR(40)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_SignalLabel DEFAULT(''),
    ValueAmount                       DECIMAL(18,2) NULL,
    ValueText                         VARCHAR(100)  NULL,
    ReportRoute                       VARCHAR(100)  NULL,
    SortOrder                         INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardPurchasingManagementAttention PRIMARY KEY CLUSTERED (PurchasingManagementAttentionId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardPurchasingManagementAttention_SnapshotKey_SortOrder
    ON BTR_PortalDashboardPurchasingManagementAttention (SnapshotKey, SortOrder)
GO
