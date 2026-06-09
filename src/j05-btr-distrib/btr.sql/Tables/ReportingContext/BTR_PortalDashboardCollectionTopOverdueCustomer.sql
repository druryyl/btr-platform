CREATE TABLE BTR_PortalDashboardCollectionTopOverdueCustomer
(
    CollectionTopOverdueCustomerId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueCustomer_CollectionTopOverdueCustomerId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                             INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueCustomer_Rank DEFAULT(0),
    CustomerCode                     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueCustomer_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueCustomer_CustomerName DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionTopOverdueCustomer_OverdueBalance DEFAULT(0),
    PercentOfTotal                   DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardCollectionTopOverdueCustomer PRIMARY KEY CLUSTERED (CollectionTopOverdueCustomerId),
    CONSTRAINT UX_BTR_PortalDashboardCollectionTopOverdueCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
