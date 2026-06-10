CREATE TABLE BTRPD_PiutangCustomerAging
(
    PiutangCustomerAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_PiutangCustomerAgingId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_SnapshotKey DEFAULT('CURRENT'),
    CustomerId             VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerId DEFAULT(''),
    CustomerCode           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerCode DEFAULT(''),
    CustomerName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerName DEFAULT(''),
    CurrentAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CurrentAmount DEFAULT(0),
    Aging30Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging30Amount DEFAULT(0),
    Aging60Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging60Amount DEFAULT(0),
    Aging90Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging90Amount DEFAULT(0),
    AgingOver90Amount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_AgingOver90Amount DEFAULT(0),
    LastUpdate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_LastUpdate DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_PiutangCustomerAging PRIMARY KEY CLUSTERED (PiutangCustomerAgingId),
    CONSTRAINT UX_BTRPD_PiutangCustomerAging_SnapshotKey_CustomerId UNIQUE (SnapshotKey, CustomerId)
)
GO

CREATE INDEX IX_BTRPD_PiutangCustomerAging_SnapshotKey
    ON BTRPD_PiutangCustomerAging (SnapshotKey)
GO
