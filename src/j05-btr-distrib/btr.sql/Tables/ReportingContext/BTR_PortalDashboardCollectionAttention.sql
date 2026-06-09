CREATE TABLE BTR_PortalDashboardCollectionAttention
(
    CollectionAttentionId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_CollectionAttentionId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_EntityType DEFAULT(''),
    EntityId              VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_EntityId DEFAULT(''),
    EntityCode            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_EntityCode DEFAULT(''),
    EntityName            VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_EntityName DEFAULT(''),
    SignalKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_SignalKey DEFAULT(''),
    SignalLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_SignalLabel DEFAULT(''),
    ValueAmount           DECIMAL(18,2) NULL,
    ValueText             VARCHAR(100)  NULL,
    WilayahName           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_WilayahName DEFAULT(''),
    ReportRoute           VARCHAR(100)  NULL,
    SortOrder             INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardCollectionAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTR_PortalDashboardCollectionAttention PRIMARY KEY CLUSTERED (CollectionAttentionId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardCollectionAttention_SnapshotKey_SortOrder
    ON BTR_PortalDashboardCollectionAttention (SnapshotKey, SortOrder)
GO
