CREATE TABLE BTRPD_CollectionTopOverdueCustomer
(
    CollectionTopOverdueCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CollectionTopOverdueCustomerId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                             INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_Rank DEFAULT(0),
    CustomerCode                     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_CustomerName DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueCustomer_OverdueBalance DEFAULT(0),
    PercentOfTotal                   DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueCustomer PRIMARY KEY CLUSTERED (CollectionTopOverdueCustomerId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
