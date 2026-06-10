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
GO

CREATE INDEX IX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId
    ON BTRPD_SalesmanPrincipalAchievement (SnapshotKey, SalesPersonId)
GO
