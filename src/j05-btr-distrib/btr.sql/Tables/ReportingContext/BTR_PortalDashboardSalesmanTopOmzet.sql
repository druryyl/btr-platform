CREATE TABLE BTR_PortalDashboardSalesmanTopOmzet
(
    SalesmanTopOmzetId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_SalesmanTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_Rank DEFAULT(0),
    SalesPersonId      VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_SalesPersonId DEFAULT(''),
    SalesPersonCode    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_SalesPersonCode DEFAULT(''),
    SalesPersonName    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopOmzet_CompletedOmzet DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardSalesmanTopOmzet PRIMARY KEY CLUSTERED (SalesmanTopOmzetId),
    CONSTRAINT UX_BTR_PortalDashboardSalesmanTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
