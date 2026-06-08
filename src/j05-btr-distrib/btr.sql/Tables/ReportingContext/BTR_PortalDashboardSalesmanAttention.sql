CREATE TABLE BTR_PortalDashboardSalesmanAttention
(
    SalesmanAttentionId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SalesmanAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId       VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SalesPersonId DEFAULT(''),
    SalesPersonCode     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SalesPersonCode DEFAULT(''),
    SalesPersonName     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SalesPersonName DEFAULT(''),
    SignalKey           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(100)  NULL,
    WilayahName         VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_WilayahName DEFAULT(''),
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardSalesmanAttention PRIMARY KEY CLUSTERED (SalesmanAttentionId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardSalesmanAttention_SnapshotKey_SortOrder
    ON BTR_PortalDashboardSalesmanAttention (SnapshotKey, SortOrder)
GO
