CREATE TABLE BTR_PortalDashboardPurchasingManagementTopPrincipal
(
    PurchasingManagementTopPrincipalId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                                 INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_Rank DEFAULT(0),
    PrincipalName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_PrincipalName DEFAULT(''),
    MtdPurchaseAmount                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_MtdPurchaseAmount DEFAULT(0),
    PercentOfPurchase                    DECIMAL(9,4)  NULL,
    InventoryValue                       DECIMAL(18,2) NULL,
    PercentOfInventory                   DECIMAL(9,4)  NULL,
    AtRiskValue                          DECIMAL(18,2) NULL,
    PercentOfAtRisk                      DECIMAL(9,4)  NULL,
    IsCompoundDependency                 BIT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_IsCompoundDependency DEFAULT(0),
    IsInventoryNoPurchase                BIT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_IsInventoryNoPurchase DEFAULT(0),
    ReportRoute                          VARCHAR(100)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingManagementTopPrincipal_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardPurchasingManagementTopPrincipal PRIMARY KEY CLUSTERED (PurchasingManagementTopPrincipalId)
)
GO

CREATE UNIQUE INDEX UX_BTR_PortalDashboardPurchasingManagementTopPrincipal_SnapshotKey_Rank
    ON BTR_PortalDashboardPurchasingManagementTopPrincipal (SnapshotKey, Rank)
GO
