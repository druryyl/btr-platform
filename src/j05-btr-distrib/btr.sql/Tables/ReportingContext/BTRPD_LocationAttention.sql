CREATE TABLE BTRPD_LocationAttention

(

    LocationAttentionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_LocationAttentionId DEFAULT(''),

    SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SnapshotKey DEFAULT('CURRENT'),

    EntityType          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_EntityType DEFAULT(''),

    EntityCode          VARCHAR(5)    NULL,

    EntityName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_EntityName DEFAULT(''),

    SignalKey           VARCHAR(40)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SignalKey DEFAULT(''),

    SignalLabel         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SignalLabel DEFAULT(''),

    ValueAmount         DECIMAL(18,2) NULL,

    ValueText           VARCHAR(100)  NULL,

    ReportRoute         VARCHAR(100)  NULL,

    SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_LocationAttention_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_LocationAttention PRIMARY KEY CLUSTERED (LocationAttentionId)

)

GO



CREATE INDEX IX_BTRPD_LocationAttention_SnapshotKey_SortOrder

    ON BTRPD_LocationAttention (SnapshotKey, SortOrder)

GO

