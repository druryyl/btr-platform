CREATE TABLE BTRPD_CustomerRiskForecastKpi
(
    SnapshotKey                         VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_SnapshotKey DEFAULT('CURRENT'),
    GeneratedAt                         DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_GeneratedAt DEFAULT('3000-01-01'),
    BusinessDate                        DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_BusinessDate DEFAULT('3000-01-01'),
    HorizonDays                         INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HorizonDays DEFAULT(0),
    CustomersForecastedAtRisk           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CustomersForecastedAtRisk DEFAULT(0),
    HighRiskCustomerCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HighRiskCustomerCount DEFAULT(0),
    CriticalCustomerCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CriticalCustomerCount DEFAULT(0),
    ElevatedRiskReceivable              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ElevatedRiskReceivable DEFAULT(0),
    ElevatedRiskReceivablePercent       DECIMAL(9,4)   NULL,
    PortfolioHealthScore                DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PortfolioHealthScore DEFAULT(0),
    TotalPiutang                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_TotalPiutang DEFAULT(0),
    ForecastConfidence                  VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ForecastConfidence DEFAULT(''),
    PaymentDelaySignalCount             INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PaymentDelaySignalCount DEFAULT(0),
    CreditLimitSignalCount              INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CreditLimitSignalCount DEFAULT(0),
    InactivitySignalCount               INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_InactivitySignalCount DEFAULT(0),
    PurchaseDeclineSignalCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_PurchaseDeclineSignalCount DEFAULT(0),
    CollectionRiskSignalCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CollectionRiskSignalCount DEFAULT(0),
    HealthyCount                        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HealthyCount DEFAULT(0),
    WatchCount                          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_WatchCount DEFAULT(0),
    AttentionCount                      INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_AttentionCount DEFAULT(0),
    HighRiskCount                       INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_HighRiskCount DEFAULT(0),
    CriticalCount                       INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_CriticalCount DEFAULT(0),
    ExecutiveSummaryText                VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_ExecutiveSummaryText DEFAULT(''),
    LastRefreshLogId                    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastKpi_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastKpi PRIMARY KEY CLUSTERED (SnapshotKey)
)
GO
