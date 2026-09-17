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
GO
