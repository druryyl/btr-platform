CREATE TABLE BTRPD_CollectionTopOverdueSalesman
(
    CollectionTopOverdueSalesmanId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank                           INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_Rank DEFAULT(0),
    SalesPersonId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonId DEFAULT(''),
    SalesPersonCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonCode DEFAULT(''),
    SalesPersonName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonName DEFAULT(''),
    OverdueBalance                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueSalesman_OverdueBalance DEFAULT(0),
    PercentOfTotal                 DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueSalesman PRIMARY KEY CLUSTERED (CollectionTopOverdueSalesmanId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
