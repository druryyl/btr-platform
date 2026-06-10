CREATE TABLE BTRPD_PurchasingPostingStatus
(
    PurchasingPostingStatusId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_PurchasingPostingStatusId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_SnapshotKey DEFAULT('CURRENT'),
    StatusKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_StatusKey DEFAULT(''),
    StatusLabel               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_StatusLabel DEFAULT(''),
    SortOrder                 INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_SortOrder DEFAULT(0),
    PurchaseAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PurchasingPostingStatus_PurchaseAmount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingPostingStatus PRIMARY KEY CLUSTERED (PurchasingPostingStatusId),
    CONSTRAINT UX_BTRPD_PurchasingPostingStatus_SnapshotKey_StatusKey UNIQUE (SnapshotKey, StatusKey)
)
GO
