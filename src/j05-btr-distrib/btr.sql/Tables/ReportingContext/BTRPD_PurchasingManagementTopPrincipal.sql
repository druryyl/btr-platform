CREATE TABLE BTRPD_PurchasingManagementTopPrincipal
(
    PurchasingManagementTopPrincipalId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                                 INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_Rank DEFAULT(0),
    PrincipalName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_PrincipalName DEFAULT(''),
    MtdPurchaseAmount                    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_MtdPurchaseAmount DEFAULT(0),
    PercentOfPurchase                    DECIMAL(9,4)  NULL,
    InventoryValue                       DECIMAL(18,2) NULL,
    PercentOfInventory                   DECIMAL(9,4)  NULL,
    AtRiskValue                          DECIMAL(18,2) NULL,
    PercentOfAtRisk                      DECIMAL(9,4)  NULL,
    IsCompoundDependency                 BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_IsCompoundDependency DEFAULT(0),
    IsInventoryNoPurchase                BIT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_IsInventoryNoPurchase DEFAULT(0),
    ReportRoute                          VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementTopPrincipal_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_PurchasingManagementTopPrincipal PRIMARY KEY CLUSTERED (PurchasingManagementTopPrincipalId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey_Rank
    ON BTRPD_PurchasingManagementTopPrincipal (SnapshotKey, Rank)
GO
