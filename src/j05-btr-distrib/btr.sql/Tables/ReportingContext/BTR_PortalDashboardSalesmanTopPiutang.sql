CREATE TABLE BTR_PortalDashboardSalesmanTopPiutang
(
    SalesmanTopPiutangId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_SalesmanTopPiutangId DEFAULT(''),
    SnapshotKey          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_SnapshotKey DEFAULT('CURRENT'),
    Rank                 INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_Rank DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_SalesPersonName DEFAULT(''),
    OutstandingBalance   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopPiutang_OutstandingBalance DEFAULT(0),
    PercentOfTotal       DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardSalesmanTopPiutang PRIMARY KEY CLUSTERED (SalesmanTopPiutangId),
    CONSTRAINT UX_BTR_PortalDashboardSalesmanTopPiutang_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
