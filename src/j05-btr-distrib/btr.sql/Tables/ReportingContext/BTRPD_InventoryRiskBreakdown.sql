CREATE TABLE BTRPD_InventoryRiskBreakdown
(
    InventoryRiskBreakdownId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_InventoryRiskBreakdownId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_DimensionType DEFAULT(''),
    Name                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_Name DEFAULT(''),
    AtRiskValue              DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_AtRiskValue DEFAULT(0),
    ItemCount                INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_ItemCount DEFAULT(0),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_Rank DEFAULT(0),
    PercentOfAtRisk          DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_InventoryRiskBreakdown PRIMARY KEY CLUSTERED (InventoryRiskBreakdownId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_InventoryRiskBreakdown_SnapshotKey_DimensionType_Rank
    ON BTRPD_InventoryRiskBreakdown (SnapshotKey, DimensionType, Rank)
GO
