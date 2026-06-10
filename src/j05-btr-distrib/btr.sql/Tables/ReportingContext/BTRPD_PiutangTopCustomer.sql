CREATE TABLE BTRPD_PiutangTopCustomer
(
    PiutangTopCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_PiutangTopCustomerId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_Rank DEFAULT(0),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomer_OutstandingBalance DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomer PRIMARY KEY CLUSTERED (PiutangTopCustomerId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
