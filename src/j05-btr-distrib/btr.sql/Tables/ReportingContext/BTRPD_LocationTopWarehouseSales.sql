CREATE TABLE BTRPD_LocationTopWarehouseSales

(

    LocationTopWarehouseSalesId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_LocationTopWarehouseSalesId DEFAULT(''),

    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_SnapshotKey DEFAULT('CURRENT'),

    Rank                        INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_Rank DEFAULT(0),

    WarehouseId                 VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_WarehouseId DEFAULT(''),

    WarehouseName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_WarehouseName DEFAULT(''),

    MtdOmzet                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWarehouseSales_MtdOmzet DEFAULT(0),

    PercentOfTotal              DECIMAL(9,4)  NULL,

    ReportRoute                 VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWarehouseSales PRIMARY KEY CLUSTERED (LocationTopWarehouseSalesId),

    CONSTRAINT UX_BTRPD_LocationTopWarehouseSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO

