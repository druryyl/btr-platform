CREATE TABLE BTRPD_InventoryForecastLevel
(
    InventoryForecastLevelId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_Id DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_SnapshotKey DEFAULT('CURRENT'),
    HorizonDay               INT           NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_HorizonDay DEFAULT(0),
    ProjectedInventoryValue  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastLevel_ProjectedInventoryValue DEFAULT(0),

    CONSTRAINT PK_BTRPD_InventoryForecastLevel PRIMARY KEY CLUSTERED (InventoryForecastLevelId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastLevel_SnapshotKey_Day
    ON BTRPD_InventoryForecastLevel (SnapshotKey, HorizonDay)
GO
