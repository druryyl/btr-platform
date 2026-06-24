CREATE TABLE BTRPD_EntityAnalytics_Relationship
(
    EntityAnalyticsRelationshipId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_Id DEFAULT(''),
    SourceEntityType              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_SourceEntityType DEFAULT(''),
    SourceEntityId                VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_SourceEntityId DEFAULT(''),
    SourceEntityCode              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_SourceEntityCode DEFAULT(''),
    RelationshipCode              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_RelationshipCode DEFAULT(''),
    TargetEntityType              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetEntityType DEFAULT(''),
    TargetEntityId                VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetEntityId DEFAULT(''),
    TargetEntityCode              VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetEntityCode DEFAULT(''),
    TargetDisplayName             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_TargetDisplayName DEFAULT(''),
    Rank                          INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_Rank DEFAULT(0),
    MetricValue                   DECIMAL(18,2)  NULL,
    PeriodYear                    INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_PeriodYear DEFAULT(0),
    PeriodMonth                   INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_PeriodMonth DEFAULT(0),
    GeneratedAt                   DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                     DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId              VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Relationship_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Relationship PRIMARY KEY CLUSTERED (EntityAnalyticsRelationshipId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Relationship_Source_Relationship_Period_Rank' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Relationship'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Relationship_Source_Relationship_Period_Rank
    ON BTRPD_EntityAnalytics_Relationship (SourceEntityType, SourceEntityId, RelationshipCode, PeriodYear, PeriodMonth, Rank)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Relationship_Source_Relationship' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Relationship'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Relationship_Source_Relationship
    ON BTRPD_EntityAnalytics_Relationship (SourceEntityType, SourceEntityId, RelationshipCode)
    INCLUDE (TargetEntityType, TargetEntityId, TargetEntityCode, TargetDisplayName, Rank, MetricValue,
             PeriodYear, PeriodMonth, GeneratedAt)
GO
