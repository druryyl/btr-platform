CREATE TABLE BTRPD_EntityAnalytics_Attention
(
    EntityAnalyticsAttentionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_Id DEFAULT(''),
    EntityType                 VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_EntityType DEFAULT(''),
    EntityId                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_EntityId DEFAULT(''),
    EntityCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_EntityCode DEFAULT(''),
    SignalCode                 VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_SignalCode DEFAULT(''),
    SignalCategory             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_SignalCategory DEFAULT(''),
    SignalTitle                VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_SignalTitle DEFAULT(''),
    FirstSeenYear              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_FirstSeenYear DEFAULT(0),
    FirstSeenMonth             INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_FirstSeenMonth DEFAULT(0),
    LastSeenYear               INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_LastSeenYear DEFAULT(0),
    LastSeenMonth              INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_LastSeenMonth DEFAULT(0),
    ConsecutivePeriods         INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_ConsecutivePeriods DEFAULT(0),
    TotalOccurrences           INT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_TotalOccurrences DEFAULT(0),
    IsActive                   BIT            NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_IsActive DEFAULT(0),
    GeneratedAt                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_GeneratedAt DEFAULT('3000-01-01'),
    CreatedAt                  DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_CreatedAt DEFAULT('3000-01-01'),
    UpdatedAt                  DATETIME       NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_UpdatedAt DEFAULT('3000-01-01'),
    LastRefreshLogId           VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_Attention_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_Attention PRIMARY KEY CLUSTERED (EntityAnalyticsAttentionId)
)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_BTRPD_EntityAnalytics_Attention_Entity_Signal' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Attention'))
CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_Attention_Entity_Signal
    ON BTRPD_EntityAnalytics_Attention (EntityType, EntityId, SignalCode)
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_EntityAnalytics_Attention_Entity_Active' AND object_id = OBJECT_ID(N'dbo.BTRPD_EntityAnalytics_Attention'))
CREATE INDEX IX_BTRPD_EntityAnalytics_Attention_Entity_Active
    ON BTRPD_EntityAnalytics_Attention (EntityType, EntityId, IsActive)
    INCLUDE (SignalCode, SignalTitle, FirstSeenYear, FirstSeenMonth, LastSeenYear, LastSeenMonth,
             ConsecutivePeriods, TotalOccurrences, GeneratedAt)
GO
