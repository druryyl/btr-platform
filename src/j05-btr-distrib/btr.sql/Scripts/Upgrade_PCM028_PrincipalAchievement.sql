-- PCM-028 Principal Achievement snapshot.
-- Stores PRN-TGT-002 and PRN-TGT-003 from stored PRN-SALES-001 and PRN-TGT-001.
-- Does not write or update PRN-SALES-001 or PRN-TGT-001.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalAchievementKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalAchievementKpi
    (
        SnapshotKey                VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_SnapshotKey DEFAULT('CURRENT'),
        AchievementAmountKpiId     VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_AchievementAmountKpiId DEFAULT('PRN-TGT-002'),
        AchievementPercentageKpiId VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_AchievementPercentageKpiId DEFAULT('PRN-TGT-003'),
        SalesOutKpiId              VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
        TargetKpiId                VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_TargetKpiId DEFAULT('PRN-TGT-001'),
        PeriodYear                 INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_PeriodYear DEFAULT(0),
        PeriodMonth                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_PeriodMonth DEFAULT(0),
        GeneratedAt                DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId           VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievementKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalAchievementKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalAchievement', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalAchievement
    (
        PrincipalAchievementId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_PrincipalAchievementId DEFAULT(''),
        SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SnapshotKey DEFAULT('CURRENT'),
        AchievementAmountKpiId   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_AchievementAmountKpiId DEFAULT('PRN-TGT-002'),
        AchievementPercentageKpiId VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_AchievementPercentageKpiId DEFAULT('PRN-TGT-003'),
        SalesOutKpiId            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SalesOutKpiId DEFAULT('PRN-SALES-001'),
        TargetKpiId              VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_TargetKpiId DEFAULT('PRN-TGT-001'),
        PeriodYear               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_PeriodYear DEFAULT(0),
        PeriodMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_PeriodMonth DEFAULT(0),
        SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SupplierId DEFAULT(''),
        SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SupplierName DEFAULT(''),
        SalesOutAmount           DECIMAL(18,2) NULL,
        TargetAmount             DECIMAL(18,2) NULL,
        AchievementAmount        DECIMAL(18,2) NULL,
        AchievementPercentage    DECIMAL(18,6) NULL,
        SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_SortOrder DEFAULT(0),
        GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId         VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalAchievement_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalAchievement PRIMARY KEY CLUSTERED (PrincipalAchievementId),
        CONSTRAINT UX_BTRPD_PrincipalAchievement_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalAchievement_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalAchievement'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalAchievement_SnapshotKey_SortOrder
        ON BTRPD_PrincipalAchievement (SnapshotKey, SortOrder)
END
GO
