CREATE TABLE BTRPD_InventoryForecastRecommendation
(
    InventoryForecastRecommendationId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_Id DEFAULT(''),
    SnapshotKey                       VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                         INT            NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SortOrder DEFAULT(0),
    BrgId                             VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgId DEFAULT(''),
    BrgCode                           VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgCode DEFAULT(''),
    BrgName                           VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_BrgName DEFAULT(''),
    SupplierName                      VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_SupplierName DEFAULT(''),
    ReorderDate                       DATETIME       NULL,
    RecommendedPurchaseQty            DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_RecommendedPurchaseQty DEFAULT(0),
    AverageDailyConsumption           DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_AverageDailyConsumption DEFAULT(0),
    CurrentQty                        DECIMAL(18,4)  NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_CurrentQty DEFAULT(0),
    DaysOfSupply                      DECIMAL(18,2)  NULL,
    Urgency                           VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_Urgency DEFAULT(''),
    ReportRoute                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_ReportRoute DEFAULT(''),
    EntityCode                        VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_InventoryForecastRecommendation_EntityCode DEFAULT(''),

    CONSTRAINT PK_BTRPD_InventoryForecastRecommendation PRIMARY KEY CLUSTERED (InventoryForecastRecommendationId)
)
GO

CREATE INDEX IX_BTRPD_InventoryForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_InventoryForecastRecommendation (SnapshotKey, SortOrder)
GO
