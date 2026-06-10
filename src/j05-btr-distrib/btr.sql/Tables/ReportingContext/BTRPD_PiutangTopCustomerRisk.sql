CREATE TABLE BTRPD_PiutangTopCustomerRisk
(
    PiutangTopCustomerRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_PiutangTopCustomerRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Rank DEFAULT(0),
    CustomerId               VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerId DEFAULT(''),
    CustomerCode             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerCode DEFAULT(''),
    CustomerName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerName DEFAULT(''),
    TotalPiutang             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_TotalPiutang DEFAULT(0),
    CurrentAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CurrentAmount DEFAULT(0),
    Aging30Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging30Amount DEFAULT(0),
    Aging60Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging60Amount DEFAULT(0),
    Aging90Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging90Amount DEFAULT(0),
    AgingOver90Amount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_AgingOver90Amount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomerRisk PRIMARY KEY CLUSTERED (PiutangTopCustomerRiskId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomerRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
