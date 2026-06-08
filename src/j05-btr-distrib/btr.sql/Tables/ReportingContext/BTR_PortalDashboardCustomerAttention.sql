CREATE TABLE BTR_PortalDashboardCustomerAttention
(
    CustomerAttentionId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_CustomerAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_SnapshotKey DEFAULT('CURRENT'),
    CustomerCode        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_CustomerCode DEFAULT(''),
    CustomerName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_CustomerName DEFAULT(''),
    SignalKey           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(50)   NULL,
    WilayahName         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_WilayahName DEFAULT(''),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCustomerAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardCustomerAttention PRIMARY KEY CLUSTERED (CustomerAttentionId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardCustomerAttention_SnapshotKey_SortOrder
    ON BTR_PortalDashboardCustomerAttention (SnapshotKey, SortOrder)
GO
