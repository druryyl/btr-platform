CREATE TABLE BTR_PortalDashboardCollectionTopOverdueSalesman
(
    CollectionTopOverdueSalesmanId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank                           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_Rank DEFAULT(0),
    SalesPersonId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SalesPersonId DEFAULT(''),
    SalesPersonCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SalesPersonCode DEFAULT(''),
    SalesPersonName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_SalesPersonName DEFAULT(''),
    OverdueBalance                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueSalesman_OverdueBalance DEFAULT(0),
    PercentOfTotal                 DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardCollectionTopOverdueSalesman PRIMARY KEY CLUSTERED (CollectionTopOverdueSalesmanId),
    CONSTRAINT UX_BTR_PortalDashboardCollectionTopOverdueSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
