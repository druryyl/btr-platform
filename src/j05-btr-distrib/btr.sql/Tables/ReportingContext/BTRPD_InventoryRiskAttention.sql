CREATE TABLE BTRPD_InventoryRiskAttention
(
    InventoryRiskAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_InventoryRiskAttentionId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SnapshotKey DEFAULT('CURRENT'),
    BrgId                    VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgId DEFAULT(''),
    BrgCode                  VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgCode DEFAULT(''),
    BrgName                  VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_BrgName DEFAULT(''),
    KategoriName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_KategoriName DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SupplierName DEFAULT(''),
    Qty                      INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_Qty DEFAULT(0),
    InventoryValue           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur      INT           NULL,
    SignalKey                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SignalKey DEFAULT(''),
    SignalLabel              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SignalLabel DEFAULT(''),
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryRiskAttention PRIMARY KEY CLUSTERED (InventoryRiskAttentionId)
)
GO

CREATE INDEX IX_BTRPD_InventoryRiskAttention_SnapshotKey_SortOrder
    ON BTRPD_InventoryRiskAttention (SnapshotKey, SortOrder)
GO
