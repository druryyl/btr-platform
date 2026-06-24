CREATE TABLE BTRPD_SalesForecastKpi
(
    SnapshotKey                 VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                 DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    PeriodYear                  INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_PeriodYear DEFAULT(0),
    PeriodMonth                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_PeriodMonth DEFAULT(0),
    BusinessDate                DATETIME       NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    DaysInMonth                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysInMonth DEFAULT(0),
    DaysElapsed                 INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysElapsed DEFAULT(0),
    DaysRemaining               INT            NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DaysRemaining DEFAULT(0),
    CurrentSales                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_CurrentSales DEFAULT(0),
    TotalTarget                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_TotalTarget DEFAULT(0),
    CurrentAchievementPercent   DECIMAL(9,4)   NULL,
    DailyAverageSales           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_DailyAverageSales DEFAULT(0),
    ForecastSales               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastSales DEFAULT(0),
    ForecastAchievementPercent  DECIMAL(9,4)   NULL,
    RequiredDailySales          DECIMAL(18,2)  NULL,
    TargetGap                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_TargetGap DEFAULT(0),
    ForecastVariance            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastVariance DEFAULT(0),
    BestCaseSales               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_BestCaseSales DEFAULT(0),
    WorstCaseSales              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_WorstCaseSales DEFAULT(0),
    ForecastConfidence          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastConfidence DEFAULT(''),
    ForecastRiskBand              VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_ForecastRiskBand DEFAULT(''),
    LastRefreshLogId            VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_SalesForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_SalesForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
