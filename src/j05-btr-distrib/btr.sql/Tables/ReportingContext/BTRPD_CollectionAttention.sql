CREATE TABLE BTRPD_CollectionAttention
(
    CollectionAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_CollectionAttentionId DEFAULT(''),
    SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SnapshotKey DEFAULT('CURRENT'),
    EntityType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityType DEFAULT(''),
    EntityId              VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityId DEFAULT(''),
    EntityCode            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityCode DEFAULT(''),
    EntityName            VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_EntityName DEFAULT(''),
    SignalKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SignalKey DEFAULT(''),
    SignalLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SignalLabel DEFAULT(''),
    ValueAmount           DECIMAL(18,2) NULL,
    ValueText             VARCHAR(100)  NULL,
    WilayahName           VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_WilayahName DEFAULT(''),
    ReportRoute           VARCHAR(100)  NULL,
    SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionAttention_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionAttention PRIMARY KEY CLUSTERED (CollectionAttentionId)
)
GO

CREATE INDEX IX_BTRPD_CollectionAttention_SnapshotKey_SortOrder
    ON BTRPD_CollectionAttention (SnapshotKey, SortOrder)
GO
