CREATE TABLE BTRPD_RefreshLog
(
    RefreshLogId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_RefreshLogId DEFAULT(''),
    Domain         VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_Domain DEFAULT(''),
    StartedAt      DATETIME     NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_StartedAt DEFAULT('3000-01-01'),
    CompletedAt    DATETIME     NULL,
    Status         VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_Status DEFAULT(''),
    DurationMs     INT          NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_DurationMs DEFAULT(0),
    ErrorMessage   VARCHAR(500) NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_ErrorMessage DEFAULT(''),
    TriggeredBy    VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_RefreshLog_TriggeredBy DEFAULT(''),

    CONSTRAINT PK_BTRPD_RefreshLog PRIMARY KEY CLUSTERED (RefreshLogId)
)
GO

CREATE INDEX IX_BTRPD_RefreshLog_Domain_CompletedAt
    ON BTRPD_RefreshLog (Domain, CompletedAt DESC)
GO
