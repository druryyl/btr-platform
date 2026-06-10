CREATE TABLE BTRPD_SalesmanTopOmzet
(
    SalesmanTopOmzetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesmanTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_Rank DEFAULT(0),
    SalesPersonId      VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonId DEFAULT(''),
    SalesPersonCode    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonCode DEFAULT(''),
    SalesPersonName    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_SalesPersonName DEFAULT(''),
    CompletedOmzet     DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopOmzet_CompletedOmzet DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_SalesmanTopOmzet PRIMARY KEY CLUSTERED (SalesmanTopOmzetId),
    CONSTRAINT UX_BTRPD_SalesmanTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
