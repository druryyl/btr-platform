CREATE TABLE BTRPD_InventoryRiskTopDead
(
    InventoryRiskTopDeadId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_InventoryRiskTopDeadId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_SnapshotKey DEFAULT('CURRENT'),
    Rank                   INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_Rank DEFAULT(0),
    BrgId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgId DEFAULT(''),
    BrgCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgCode DEFAULT(''),
    BrgName                VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_BrgName DEFAULT(''),
    KategoriName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_KategoriName DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_SupplierName DEFAULT(''),
    Qty                    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_Qty DEFAULT(0),
    InventoryValue         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_InventoryValue DEFAULT(0),
    DaysSinceLastFaktur    INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskTopDead_DaysSinceLastFaktur DEFAULT(0),
    PercentOfAtRisk        DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskTopDead PRIMARY KEY CLUSTERED (InventoryRiskTopDeadId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskTopDead_SnapshotKey_Rank
    ON BTRPD_InventoryRiskTopDead (SnapshotKey, Rank)
GO
