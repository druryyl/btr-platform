CREATE TABLE BTR_PortalDashboardPurchasingPostingStatus
(
    PurchasingPostingStatusId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingPostingStatus_PurchasingPostingStatusId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingPostingStatus_SnapshotKey DEFAULT('CURRENT'),
    StatusKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingPostingStatus_StatusKey DEFAULT(''),
    StatusLabel               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingPostingStatus_StatusLabel DEFAULT(''),
    SortOrder                 INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingPostingStatus_SortOrder DEFAULT(0),
    PurchaseAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardPurchasingPostingStatus_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardPurchasingPostingStatus PRIMARY KEY CLUSTERED (PurchasingPostingStatusId),
    CONSTRAINT UX_BTR_PortalDashboardPurchasingPostingStatus_SnapshotKey_StatusKey UNIQUE (SnapshotKey, StatusKey)
)
GO
