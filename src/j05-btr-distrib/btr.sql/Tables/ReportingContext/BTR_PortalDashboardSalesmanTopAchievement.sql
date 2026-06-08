CREATE TABLE BTR_PortalDashboardSalesmanTopAchievement
(
    SalesmanTopAchievementId VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_SalesmanTopAchievementId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_Rank DEFAULT(0),
    SalesPersonId            VARCHAR(13)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_SalesPersonId DEFAULT(''),
    SalesPersonCode          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_SalesPersonCode DEFAULT(''),
    SalesPersonName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_SalesPersonName DEFAULT(''),
    TargetAmount             DECIMAL(18,2) NULL,
    CompletedOmzet           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_PortalDashboardSalesmanTopAchievement_CompletedOmzet DEFAULT(0),
    AchievementPercent       DECIMAL(9,4)  NULL,
    PercentOfTotal           DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTR_PortalDashboardSalesmanTopAchievement PRIMARY KEY CLUSTERED (SalesmanTopAchievementId),
    CONSTRAINT UX_BTR_PortalDashboardSalesmanTopAchievement_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
