CREATE TABLE BTR_PortalDashboardInventoryRiskAttention
(
    InventoryRiskAttentionId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_InventoryRiskAttentionId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_SnapshotKey DEFAULT('CURRENT'),
    BrgId                    VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_BrgId DEFAULT(''),
    BrgCode                  VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_BrgCode DEFAULT(''),
    BrgName                  VARCHAR(100)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_BrgName DEFAULT(''),
    KategoriName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_KategoriName DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_SupplierName DEFAULT(''),
    Qty                      INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_Qty DEFAULT(0),
    InventoryValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur      INT           NULL,
    SignalKey                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_SignalKey DEFAULT(''),
    SignalLabel              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_SignalLabel DEFAULT(''),
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardInventoryRiskAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardInventoryRiskAttention PRIMARY KEY CLUSTERED (InventoryRiskAttentionId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardInventoryRiskAttention_SnapshotKey_SortOrder
    ON BTR_PortalDashboardInventoryRiskAttention (SnapshotKey, SortOrder)
GO
