CREATE TABLE BTR_PortalDashboardPurchasingTopPrincipal
(
    PurchasingTopPrincipalId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingTopPrincipal_PurchasingTopPrincipalId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingTopPrincipal_Rank DEFAULT(0),
    PrincipalName            VARCHAR(100)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingTopPrincipal_PrincipalName DEFAULT(''),
    PurchaseAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingTopPrincipal_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardPurchasingTopPrincipal PRIMARY KEY CLUSTERED (PurchasingTopPrincipalId),
    CONSTRAINT UX_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO

CREATE INDEX IX_BTR_PortalDashboardPurchasingTopPrincipal_SnapshotKey_Rank
    ON BTR_PortalDashboardPurchasingTopPrincipal (SnapshotKey, Rank)
GO
