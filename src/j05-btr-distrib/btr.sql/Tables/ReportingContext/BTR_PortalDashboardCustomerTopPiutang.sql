CREATE TABLE BTR_PortalDashboardCustomerTopPiutang
(
    CustomerTopPiutangId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopPiutang_CustomerTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopPiutang_Rank DEFAULT(0),
    CustomerCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopPiutang_CustomerCode DEFAULT(''),
    CustomerName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopPiutang_CustomerName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardCustomerTopPiutang PRIMARY KEY CLUSTERED (CustomerTopPiutangId),
    CONSTRAINT UX_BTR_PortalDashboardCustomerTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
