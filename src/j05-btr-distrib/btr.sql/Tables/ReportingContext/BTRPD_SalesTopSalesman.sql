CREATE TABLE BTRPD_SalesTopSalesman
(
    SalesTopSalesmanId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SalesTopSalesmanId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_Rank DEFAULT(0),
    SalesPersonName    VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesTopSalesman_CompletedOmzet DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesTopSalesman PRIMARY KEY CLUSTERED (SalesTopSalesmanId),
    CONSTRAINT UX_BTRPD_SalesTopSalesman_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
