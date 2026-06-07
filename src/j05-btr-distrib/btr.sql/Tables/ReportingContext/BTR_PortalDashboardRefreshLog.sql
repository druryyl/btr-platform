CREATE TABLE BTR_PortalDashboardRefreshLog
(
    RefreshLogId   VARCHAR(13)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_RefreshLogId DEFAULT(''),
    Domain         VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_Domain DEFAULT(''),
    StartedAt      DATETIME     NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_StartedAt DEFAULT('3000-01-01'),
    CompletedAt    DATETIME     NULL,
    Status         VARCHAR(10)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_Status DEFAULT(''),
    DurationMs     INT          NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_DurationMs DEFAULT(0),
    ErrorMessage   VARCHAR(500) NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_ErrorMessage DEFAULT(''),
    TriggeredBy    VARCHAR(20)  NOT NULL CONSTRAINT DF_BTR_PortalDashboardRefreshLog_TriggeredBy DEFAULT(''),

    CONSTRAINT PK_BTR_PortalDashboardRefreshLog PRIMARY KEY CLUSTERED (RefreshLogId)
)
GO

CREATE INDEX IX_BTR_PortalDashboardRefreshLog_Domain_CompletedAt
    ON BTR_PortalDashboardRefreshLog (Domain, CompletedAt DESC)
GO
