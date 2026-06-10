CREATE TABLE BTRPD_LocationTopWilayahSales

(

    LocationTopWilayahSalesId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_LocationTopWilayahSalesId DEFAULT(''),

    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_SnapshotKey DEFAULT('CURRENT'),

    Rank                      INT           NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_Rank DEFAULT(0),

    WilayahId                 VARCHAR(5)    NULL,

    WilayahName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_WilayahName DEFAULT(''),

    MtdOmzet                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_LocationTopWilayahSales_MtdOmzet DEFAULT(0),

    PercentOfTotal            DECIMAL(9,4)  NULL,

    DashboardRoute            VARCHAR(100)  NULL,



    CONSTRAINT PK_BTRPD_LocationTopWilayahSales PRIMARY KEY CLUSTERED (LocationTopWilayahSalesId),

    CONSTRAINT UX_BTRPD_LocationTopWilayahSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)

)

GO

