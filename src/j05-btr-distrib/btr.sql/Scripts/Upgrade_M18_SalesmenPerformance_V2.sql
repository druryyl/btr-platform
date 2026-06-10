-- M18 Salesmen Performance V2 — idempotent schema upgrade.
-- Renames KPI column, adds IsActive to child tables, creates principal + history tables.
-- Next Salesman refresh populates new columns/tables; no data backfill required.
SET NOCOUNT ON;
GO

-- BTRPD_SalesmanKpi — rename NoTargetCount
IF COL_LENGTH(N'dbo.BTRPD_SalesmanKpi', N'MissingTargetSetupCount') IS NULL
   AND COL_LENGTH(N'dbo.BTRPD_SalesmanKpi', N'NoTargetCount') IS NOT NULL
    EXEC sp_rename 'BTRPD_SalesmanKpi.NoTargetCount', 'MissingTargetSetupCount', 'COLUMN';
GO

-- BTRPD_SalesmanTopOmzet — IsActive
IF COL_LENGTH(N'dbo.BTRPD_SalesmanTopOmzet', N'IsActive') IS NULL
    ALTER TABLE BTRPD_SalesmanTopOmzet ADD IsActive BIT NOT NULL
        CONSTRAINT DF_BTRPD_SalesmanTopOmzet_IsActive DEFAULT(0);
GO

-- BTRPD_SalesmanTopAchievement — IsActive
IF COL_LENGTH(N'dbo.BTRPD_SalesmanTopAchievement', N'IsActive') IS NULL
    ALTER TABLE BTRPD_SalesmanTopAchievement ADD IsActive BIT NOT NULL
        CONSTRAINT DF_BTRPD_SalesmanTopAchievement_IsActive DEFAULT(0);
GO

-- BTRPD_SalesmanTopPiutang — IsActive
IF COL_LENGTH(N'dbo.BTRPD_SalesmanTopPiutang', N'IsActive') IS NULL
    ALTER TABLE BTRPD_SalesmanTopPiutang ADD IsActive BIT NOT NULL
        CONSTRAINT DF_BTRPD_SalesmanTopPiutang_IsActive DEFAULT(0);
GO

-- BTRPD_SalesmanAttention — IsActive
IF COL_LENGTH(N'dbo.BTRPD_SalesmanAttention', N'IsActive') IS NULL
    ALTER TABLE BTRPD_SalesmanAttention ADD IsActive BIT NOT NULL
        CONSTRAINT DF_BTRPD_SalesmanAttention_IsActive DEFAULT(0);
GO

-- BTRPD_SalesmanPrincipalAchievement
IF OBJECT_ID(N'dbo.BTRPD_SalesmanPrincipalAchievement', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanPrincipalAchievement
(
    SalesmanPrincipalAchievementId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesmanPrincipalAchievementId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SnapshotKey DEFAULT('CURRENT'),
    SalesPersonId                  VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonId DEFAULT(''),
    SalesPersonCode                VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonCode DEFAULT(''),
    SalesPersonName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonName DEFAULT(''),
    SupplierId                     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SupplierId DEFAULT(''),
    SupplierName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SupplierName DEFAULT(''),
    TargetAmount                   DECIMAL(18,2) NULL,
    CompletedOmzet                 DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_CompletedOmzet DEFAULT(0),
    AchievementPercent             DECIMAL(9,4)  NULL,
    SortOrder                      INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanPrincipalAchievement_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanPrincipalAchievement PRIMARY KEY CLUSTERED (SalesmanPrincipalAchievementId),
    CONSTRAINT UX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId_SupplierId UNIQUE (SnapshotKey, SalesPersonId, SupplierId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId' AND object_id = OBJECT_ID(N'dbo.BTRPD_SalesmanPrincipalAchievement'))
CREATE INDEX IX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId
    ON BTRPD_SalesmanPrincipalAchievement (SnapshotKey, SalesPersonId)
GO

-- BTRPD_SalesmanRepHistory
IF OBJECT_ID(N'dbo.BTRPD_SalesmanRepHistory', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_SalesmanRepHistory
(
    SalesmanRepHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesmanRepHistoryId DEFAULT(''),
    PeriodYear           INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_PeriodYear DEFAULT(0),
    PeriodMonth          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_PeriodMonth DEFAULT(0),
    SalesPersonId        VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonId DEFAULT(''),
    SalesPersonCode      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonCode DEFAULT(''),
    SalesPersonName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_SalesPersonName DEFAULT(''),
    TargetAmount         DECIMAL(18,2) NULL,
    CompletedOmzet       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_CompletedOmzet DEFAULT(0),
    AchievementPercent   DECIMAL(9,4)  NULL,
    AchievementBand      VARCHAR(20)   NULL,
    OpenBalance          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_OpenBalance DEFAULT(0),
    IsActive             BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_IsActive DEFAULT(0),
    LastRefreshLogId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_LastRefreshLogId DEFAULT(''),
    UpdatedAt            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_SalesmanRepHistory_UpdatedAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_SalesmanRepHistory PRIMARY KEY CLUSTERED (SalesmanRepHistoryId),
    CONSTRAINT UX_BTRPD_SalesmanRepHistory_PeriodYear_PeriodMonth_SalesPersonId UNIQUE (PeriodYear, PeriodMonth, SalesPersonId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_SalesmanRepHistory_SalesPersonId' AND object_id = OBJECT_ID(N'dbo.BTRPD_SalesmanRepHistory'))
CREATE INDEX IX_BTRPD_SalesmanRepHistory_SalesPersonId
    ON BTRPD_SalesmanRepHistory (SalesPersonId, PeriodYear DESC, PeriodMonth DESC)
GO
