CREATE TABLE BTR_PortalDashboardCustomerTopOmzet
(
    CustomerTopOmzetId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopOmzet_CustomerTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopOmzet_Rank DEFAULT(0),
    CustomerCode       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopOmzet_CustomerCode DEFAULT(''),
    CustomerName       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopOmzet_CustomerName DEFAULT(''),
    OmzetAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerTopOmzet_OmzetAmount DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardCustomerTopOmzet PRIMARY KEY CLUSTERED (CustomerTopOmzetId),
    CONSTRAINT UX_BTR_PortalDashboardCustomerTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
