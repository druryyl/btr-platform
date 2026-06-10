CREATE TABLE BTRPD_LocationTopWarehousePurchasing

(

    LocationTopWarehousePurchasingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_LocationTopWarehousePurchasingId DEFAULT(''),

    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_SnapshotKey DEFAULT('CURRENT'),

    Rank                             INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_Rank DEFAULT(0),

    WarehouseId                      VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_WarehouseId DEFAULT(''),

    WarehouseName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_WarehouseName DEFAULT(''),

    MtdPurchaseAmount                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehousePurchasing_MtdPurchaseAmount DEFAULT(0),

    PercentOfTotal                   DECIMAL(9,4)  NULL,

    ReportRoute                      VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehousePurchasing PRIMARY KEY CLUSTERED (LocationTopWarehousePurchasingId),

    CONSTRAINT UX_BTRPD_LocationTopWarehousePurchasing_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO

