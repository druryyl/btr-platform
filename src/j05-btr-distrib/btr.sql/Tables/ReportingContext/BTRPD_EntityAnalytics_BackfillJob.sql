CREATE TABLE BTRPD_EntityAnalytics_BackfillJob
(
    BackfillJobId       VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_Id DEFAULT(''),
    EntityTypeScope     VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_EntityTypeScope DEFAULT(''),
    FromPeriodYear      INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_FromPeriodYear DEFAULT(0),
    FromPeriodMonth     INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_FromPeriodMonth DEFAULT(0),
    ToPeriodYear        INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_ToPeriodYear DEFAULT(0),
    ToPeriodMonth       INT          NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_ToPeriodMonth DEFAULT(0),
    Layers              VARCHAR(50)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_Layers DEFAULT(''),
    OptionsJson         VARCHAR(2000) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_OptionsJson DEFAULT(''),
    Status              VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_Status DEFAULT(''),
    StartedAt           DATETIME     NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_StartedAt DEFAULT('3000-01-01'),
    CompletedAt         DATETIME     NULL,
    TriggeredBy         VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_TriggeredBy DEFAULT(''),
    MachineName         VARCHAR(100) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_MachineName DEFAULT(''),
    LastError           VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_EntityAnalytics_BackfillJob_LastError DEFAULT(''),

    CONSTRAINT PK_BTRPD_EntityAnalytics_BackfillJob PRIMARY KEY CLUSTERED (BackfillJobId)
)
GO

CREATE INDEX IX_BTRPD_EntityAnalytics_BackfillJob_Status_StartedAt
    ON BTRPD_EntityAnalytics_BackfillJob (Status, StartedAt DESC)
GO
