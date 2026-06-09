CREATE TABLE BTR_PortalDashboardLocationTopWarehousePurchasing
(
    LocationTopWarehousePurchasingId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehousePurchasing_LocationTopWarehousePurchasingId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehousePurchasing_SnapshotKey DEFAULT('CURRENT'),
    Rank                             INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehousePurchasing_Rank DEFAULT(0),
    WarehouseId                      VARCHAR(5)    NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehousePurchasing_WarehouseId DEFAULT(''),
    WarehouseName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehousePurchasing_WarehouseName DEFAULT(''),
    MtdPurchaseAmount                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWarehousePurchasing_MtdPurchaseAmount DEFAULT(0),
    PercentOfTotal                   DECIMAL(9,4)  NULL,
    ReportRoute                      VARCHAR(100)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardLocationTopWarehousePurchasing PRIMARY KEY CLUSTERED (LocationTopWarehousePurchasingId),
    CONSTRAINT UX_BTR_PortalDashboardLocationTopWarehousePurchasing_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
