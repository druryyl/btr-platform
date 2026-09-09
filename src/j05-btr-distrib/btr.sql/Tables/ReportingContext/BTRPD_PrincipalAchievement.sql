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
GO

CREATE INDEX IX_BTRPD_PrincipalAchievement_SnapshotKey_SortOrder
    ON BTRPD_PrincipalAchievement (SnapshotKey, SortOrder)
GO
