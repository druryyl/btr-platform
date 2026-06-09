CREATE TABLE BTR_PortalDashboardLocationTopWilayahSales
(
    LocationTopWilayahSalesId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWilayahSales_LocationTopWilayahSalesId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWilayahSales_SnapshotKey DEFAULT('CURRENT'),
    Rank                      INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWilayahSales_Rank DEFAULT(0),
    WilayahId                 VARCHAR(5)    NULL,
    WilayahName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWilayahSales_WilayahName DEFAULT(''),
    MtdOmzet                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationTopWilayahSales_MtdOmzet DEFAULT(0),
    PercentOfTotal            DECIMAL(9,4)  NULL,
    DashboardRoute            VARCHAR(100)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardLocationTopWilayahSales PRIMARY KEY CLUSTERED (LocationTopWilayahSalesId),
    CONSTRAINT UX_BTR_PortalDashboardLocationTopWilayahSales_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
