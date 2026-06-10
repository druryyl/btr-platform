CREATE TABLE BTRPD_InventoryRiskTopSlow
(
    InventoryRiskTopSlowId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_InventoryRiskTopSlowId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopSlow_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskTopSlow PRIMARY KEY CLUSTERED (InventoryRiskTopSlowId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskTopSlow_SnapshotKey_Rank
    ON BTRPD_InventoryRiskTopSlow (SnapshotKey, Rank)
GO
