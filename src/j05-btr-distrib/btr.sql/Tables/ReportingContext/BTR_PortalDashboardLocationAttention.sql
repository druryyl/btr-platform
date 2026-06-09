CREATE TABLE BTR_PortalDashboardLocationAttention
(
    LocationAttentionId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_LocationAttentionId DEFAULT(''),
    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_EntityType DEFAULT(''),
    EntityCode          VARCHAR(5)    NULL,
    EntityName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_EntityName DEFAULT(''),
    SignalKey           VARCHAR(40)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_SignalKey DEFAULT(''),
    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_SignalLabel DEFAULT(''),
    ValueAmount         DECIMAL(18,2) NULL,
    ValueText           VARCHAR(100)  NULL,
    ReportRoute         VARCHAR(100)  NULL,
    SortOrder           INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardLocationAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardLocationAttention PRIMARY KEY CLUSTERED (LocationAttentionId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardLocationAttention_SnapshotKey_SortOrder
    ON BTR_PortalDashboardLocationAttention (SnapshotKey, SortOrder)
GO
