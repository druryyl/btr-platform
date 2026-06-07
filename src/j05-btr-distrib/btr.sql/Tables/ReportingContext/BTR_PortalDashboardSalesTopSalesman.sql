CREATE TABLE BTR_PortalDashboardSalesTopSalesman
(
    SalesTopSalesmanId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesTopSalesman_SalesTopSalesmanId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesTopSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesTopSalesman_Rank DEFAULT(0),
    SalesPersonName    VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesTopSalesman_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesTopSalesman_CompletedOmzet DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardSalesTopSalesman PRIMARY KEY CLUSTERED (SalesTopSalesmanId),
    CONSTRAINT UX_BTR_PortalDashboardSalesTopSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
