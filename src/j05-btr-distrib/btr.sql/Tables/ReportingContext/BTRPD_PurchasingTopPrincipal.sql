CREATE TABLE BTRPD_PurchasingTopPrincipal
(
    PurchasingTopPrincipalId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PurchasingTopPrincipalId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_Rank DEFAULT(0),
    PrincipalName            VARCHAR(100)  NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PrincipalName DEFAULT(''),
    PurchaseAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingTopPrincipal_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingTopPrincipal PRIMARY KEY CLUSTERED (PurchasingTopPrincipalId),
    CONSTRAINT UX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO

CREATE INDEX IX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank
    ON BTRPD_PurchasingTopPrincipal (SnapshotKey, Rank)
GO
