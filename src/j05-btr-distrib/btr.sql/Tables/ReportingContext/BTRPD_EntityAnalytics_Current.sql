CREATE TABLE BTRPD_EntityAnalytics_Current
(
    EntityAnalyticsCurrentId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_Id DEFAULT(''),
    SnapshotKey              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_SnapshotKey DEFAULT('CURRENT'),
    EntityType               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_EntityType DEFAULT(''),
    EntityId                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_EntityId DEFAULT(''),
    EntityCode               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_EntityCode DEFAULT(''),
    KpiId                    VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_KpiId DEFAULT(''),
    NumericValue             DECIMAL(18,4)  NULL,
    TextValue                VARCHAR(200)   NULL,
    DefinitionVersion        INT            NULL,
    GeneratedAt              DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_GeneratedAt DEFAULT('3000-01-01'),
    UpdatedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Current_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Current PRIMARY KEY CLUSTERED (EntityAnalyticsCurrentId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Current_Entity_Kpi' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Current'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Current_Entity_Kpi
    ON BTRPD_EntityAnalytics_Current (EntityType, EntityId, KpiId, SnapshotKey)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Current_Entity' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Current'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Current_Entity
    ON BTRPD_EntityAnalytics_Current (EntityType, EntityId)
    INCLUDE (KpiId, NumericValue, TextValue, GeneratedAt)
GO
