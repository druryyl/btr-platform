CREATE TABLE BTRPD_EntityAnalytics_BackfillCheckpoint
(
    BackfillCheckpointId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_Id DEFAULT(''),
    BackfillJobId        VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_JobId DEFAULT(''),
    EntityType           VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_EntityType DEFAULT(''),
    PeriodYear           INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_PeriodYear DEFAULT(0),
    PeriodMonth          INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_PeriodMonth DEFAULT(0),
    Status               VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_Status DEFAULT(''),
    LayersCompleted      VARCHAR(50)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_LayersCompleted DEFAULT(''),
    EntityCount          INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_EntityCount DEFAULT(0),
    RowCountsJson        VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_RowCountsJson DEFAULT(''),
    StartedAt            DATETIME     NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_StartedAt DEFAULT('3000-01-01'),
    CompletedAt          DATETIME     NULL,
    LastError            VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_LastError DEFAULT(''),
    LastRefreshLogId     VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillCheckpoint_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_BackfillCheckpoint PRIMARY KEY CLUSTERED (BackfillCheckpointId)
)
GO

CREATE UNIQUE INDEX UX_BTRPD_EntityAnalytics_BackfillCheckpoint_Job_Type_Period
    ON BTRPD_EntityAnalytics_BackfillCheckpoint (BackfillJobId, EntityType, PeriodYear, PeriodMonth)
GO

CREATE INDEX IX_BTRPD_EntityAnalytics_BackfillCheckpoint_Job_Status
    ON BTRPD_EntityAnalytics_BackfillCheckpoint (BackfillJobId, EntityType, Status)
GO
