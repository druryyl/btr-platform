CREATE TABLE BTRPD_SalesKpi
(
    SnapshotKey        VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear         INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PeriodYear DEFAULT(0),
    PeriodMonth        INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PeriodMonth DEFAULT(0),
    TotalOmzet         DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalOmzet DEFAULT(0),
    TotalFaktur        INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalFaktur DEFAULT(0),
    TotalCustomer      INT            NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalCustomer DEFAULT(0),
    TotalTarget        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalTarget DEFAULT(0),
    TotalAchievement   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_TotalAchievement DEFAULT(0),
    AchievementPercent DECIMAL(9,4)   NULL,
    CompletedOmzet     DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_CompletedOmzet DEFAULT(0),
    PipelineOmzet      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_PipelineOmzet DEFAULT(0),
    LastRefreshLogId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_SalesKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
