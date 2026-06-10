CREATE TABLE BTRPD_SalesmanTopAchievement
(
    SalesmanTopAchievementId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesmanTopAchievementId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_Rank DEFAULT(0),
    SalesPersonId            VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonId DEFAULT(''),
    SalesPersonCode          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonCode DEFAULT(''),
    SalesPersonName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_SalesPersonName DEFAULT(''),
    TargetAmount             DECIMAL(18,2) NULL,
    CompletedOmzet           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_CompletedOmzet DEFAULT(0),
    AchievementPercent       DECIMAL(9,4)  NULL,
    PercentOfTotal           DECIMAL(9,4)  NULL,
    IsActive                 BIT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanTopAchievement_IsActive DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanTopAchievement PRIMARY KEY CLUSTERED (SalesmanTopAchievementId),
    CONSTRAINT UX_BTRPD_SalesmanTopAchievement_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
