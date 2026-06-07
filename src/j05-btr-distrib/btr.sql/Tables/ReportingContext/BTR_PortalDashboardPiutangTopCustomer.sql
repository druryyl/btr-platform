CREATE TABLE BTR_PortalDashboardPiutangTopCustomer
(
    PiutangTopCustomerId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangTopCustomer_PiutangTopCustomerId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangTopCustomer_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangTopCustomer_Rank DEFAULT(0),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangTopCustomer_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPiutangTopCustomer_OutstandingBalance DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardPiutangTopCustomer PRIMARY KEY CLUSTERED (PiutangTopCustomerId),
    CONSTRAINT UX_BTR_PortalDashboardPiutangTopCustomer_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
