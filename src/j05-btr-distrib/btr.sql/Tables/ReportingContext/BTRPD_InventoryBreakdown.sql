CREATE TABLE BTRPD_InventoryBreakdown
(
    InventoryBreakdownId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_InventoryBreakdownId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_SnapshotKey DEFAULT('CURRENT'),
    DimensionType        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_DimensionType DEFAULT(''),
    Name                 VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_Name DEFAULT(''),
    InventoryValue       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_InventoryValue DEFAULT(0),
    IsTop10              BIT           NOT NULL CONSTRAINT DF_BTRPD_InventoryBreakdown_IsTop10 DEFAULT(0),
    Top10Rank            INT           NULL,

    CONSTRAINT PK_BTRPD_InventoryBreakdown PRIMARY KEY CLUSTERED (InventoryBreakdownId)
)
GO

CREATE INDEX IX_BTRPD_InventoryBreakdown_SnapshotKey_DimensionType
    ON BTRPD_InventoryBreakdown (SnapshotKey, DimensionType)
GO
