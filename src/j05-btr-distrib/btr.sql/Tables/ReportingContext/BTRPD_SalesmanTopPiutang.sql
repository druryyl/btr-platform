CREATE TABLE BTRPD_SalesmanTopPiutang
(
    SalesmanTopPiutangId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesmanTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_Rank DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_SalesPersonName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,
    IsActive             BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopPiutang_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanTopPiutang PRIMARY KEY CLUSTERED (SalesmanTopPiutangId),
    CONSTRAINT UX_BTRPD_SalesmanTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
