CREATE TABLE BTRPD_CustomerTopPiutang
(
    CustomerTopPiutangId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_Rank DEFAULT(0),
    CustomerCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerCode DEFAULT(''),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CustomerTopPiutang PRIMARY KEY CLUSTERED (CustomerTopPiutangId),
    CONSTRAINT UX_BTRPD_CustomerTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
