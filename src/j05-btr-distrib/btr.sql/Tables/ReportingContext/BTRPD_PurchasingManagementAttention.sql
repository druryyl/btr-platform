CREATE TABLE BTRPD_PurchasingManagementAttention
(
    PurchasingManagementAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_PurchasingManagementAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType                        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_EntityType DEFAULT(''),
    EntityName                        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_EntityName DEFAULT(''),
    SignalKey                         VARCHAR(40)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SignalLabel DEFAULT(''),
    ValueAmount                       DECIMAL(18,2) NULL,
    ValueText                         VARCHAR(100)  NULL,
    ReportRoute                       VARCHAR(100)  NULL,
    SortOrder                         INT           NOT NULL CONSTRAINT DF_BTRPD_PurchasingManagementAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_PurchasingManagementAttention PRIMARY KEY CLUSTERED (PurchasingManagementAttentionId)
)
GO

CREATE INDEX IX_BTRPD_PurchasingManagementAttention_SnapshotKey_SortOrder
    ON BTRPD_PurchasingManagementAttention (SnapshotKey, SortOrder)
GO
