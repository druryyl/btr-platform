CREATE TABLE BTRPD_InventoryForecastRisk
(
    InventoryForecastRiskId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_Id DEFAULT(''),
    SnapshotKey             VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SnapshotKey DEFAULT('CURRENT'),
    SortOrder               INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SortOrder DEFAULT(0),
    SignalKey               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SignalKey DEFAULT(''),
    SignalLabel             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SignalLabel DEFAULT(''),
    BrgId                   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgId DEFAULT(''),
    BrgCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgCode DEFAULT(''),
    BrgName                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_BrgName DEFAULT(''),
    SupplierName            VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_SupplierName DEFAULT(''),
    DaysOfSupply            DECIMAL(18,2)  NULL,
    StockOutDate            DATETIME       NULL,
    ValueAmount             DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_ValueAmount DEFAULT(0),
    Urgency                 VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_Urgency DEFAULT(''),
    RuleExplanation         VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_RuleExplanation DEFAULT(''),
    ReportRoute             VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_ReportRoute DEFAULT(''),
    EntityCode              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRisk_EntityCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastRisk PRIMARY KEY CLUSTERED (InventoryForecastRiskId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastRisk_SnapshotKey_SortOrder
    ON BTRPD_InventoryForecastRisk (SnapshotKey, SortOrder)
GO
