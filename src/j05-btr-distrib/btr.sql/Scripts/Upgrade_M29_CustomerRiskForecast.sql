-- M29 Customer Risk Forecast Dashboard — idempotent schema upgrade.
-- Adds 7 snapshot tables for customer risk forecast materialized dashboard.
-- Next Customer domain refresh populates tables; no data backfill required.
SET NOCOUNT ON;
GO

-- BTRPD_CustomerRiskForecastKpi
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastKpi', N'U') IS NULL
BEGIN
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
END
GO

-- BTRPD_CustomerRiskForecastDist
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastDist', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastDist
(
    CustomerRiskForecastDistId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CustomerRiskForecastDistId DEFAULT(''),
    SnapshotKey                VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_SnapshotKey DEFAULT('CURRENT'),
    Category                   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_Category DEFAULT(''),
    CategoryLabel              VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CategoryLabel DEFAULT(''),
    CustomerCount              INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_CustomerCount DEFAULT(0),
    SortOrder                  INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastDist PRIMARY KEY CLUSTERED (CustomerRiskForecastDistId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastDist_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastDist'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastDist_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastDist (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastWilayah
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastWilayah', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastWilayah
(
    CustomerRiskForecastWilayahId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_CustomerRiskForecastWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SnapshotKey DEFAULT('CURRENT'),
    WilayahName                   VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_WilayahName DEFAULT(''),
    ElevatedRiskCustomerCount     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_ElevatedRiskCustomerCount DEFAULT(0),
    SortOrder                     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastWilayah PRIMARY KEY CLUSTERED (CustomerRiskForecastWilayahId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastWilayah_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastWilayah'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastWilayah_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastWilayah (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastSignalMix
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastSignalMix', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastSignalMix
(
    CustomerRiskForecastSignalMixId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_CustomerRiskForecastSignalMixId DEFAULT(''),
    SnapshotKey                     VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey DEFAULT('CURRENT'),
    SignalFamilyKey                 VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SignalFamilyKey DEFAULT(''),
    SignalFamilyLabel               VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SignalFamilyLabel DEFAULT(''),
    CustomerCount                   INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_CustomerCount DEFAULT(0),
    SortOrder                       INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastSignalMix_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastSignalMix PRIMARY KEY CLUSTERED (CustomerRiskForecastSignalMixId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastSignalMix'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastSignalMix (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastCustomer
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastCustomer', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastCustomer
(
    CustomerRiskForecastCustomerId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerRiskForecastCustomerId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                      INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SortOrder DEFAULT(0),
    RiskPriorityScore              INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RiskPriorityScore DEFAULT(0),
    Category                       VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_Category DEFAULT(''),
    CategoryLabel                  VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CategoryLabel DEFAULT(''),
    CustomerCode                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerCode DEFAULT(''),
    CustomerName                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_CustomerName DEFAULT(''),
    WilayahName                    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_WilayahName DEFAULT(''),
    SalesPersonName                VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_SalesPersonName DEFAULT(''),
    OpenBalance                    DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_OpenBalance DEFAULT(0),
    OverdueBalance                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_OverdueBalance DEFAULT(0),
    DueWithinHorizon               DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_DueWithinHorizon DEFAULT(0),
    Plafond                        DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_Plafond DEFAULT(0),
    ProjectedOpenBalance           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ProjectedOpenBalance DEFAULT(0),
    MtdOmzet                       DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_MtdOmzet DEFAULT(0),
    PriorMonthOmzet                DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PriorMonthOmzet DEFAULT(0),
    DeclineRatio                   DECIMAL(9,4)   NULL,
    DaysSinceLastFaktur            INT            NULL,
    AvgPaymentLagDays              DECIMAL(9,2)   NULL,
    PrimarySignalKey               VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PrimarySignalKey DEFAULT(''),
    PrimarySignalLabel             VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_PrimarySignalLabel DEFAULT(''),
    ReasonText                     VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ReasonText DEFAULT(''),
    RecommendationKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RecommendationKey DEFAULT(''),
    RecommendationLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_RecommendationLabel DEFAULT(''),
    ReportRoute                    VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_ReportRoute DEFAULT(''),
    DrillDownRoute                 VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastCustomer_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastCustomer PRIMARY KEY CLUSTERED (CustomerRiskForecastCustomerId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastCustomer_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastCustomer'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastCustomer_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastCustomer (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastAttention
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastAttention', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastAttention
(
    CustomerRiskForecastAttentionId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerRiskForecastAttentionId DEFAULT(''),
    SnapshotKey                       VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                         INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SortOrder DEFAULT(0),
    CustomerCode                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerCode DEFAULT(''),
    CustomerName                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_CustomerName DEFAULT(''),
    SignalKey                         VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SignalKey DEFAULT(''),
    SignalLabel                       VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_SignalLabel DEFAULT(''),
    Severity                          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Severity DEFAULT(''),
    Amount                            DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Amount DEFAULT(0),
    HorizonText                       VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_HorizonText DEFAULT(''),
    RuleId                            VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_RuleId DEFAULT(''),
    Explanation                       VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_Explanation DEFAULT(''),
    ReportRoute                       VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastAttention_ReportRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastAttention PRIMARY KEY CLUSTERED (CustomerRiskForecastAttentionId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastAttention_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastAttention'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastAttention (SnapshotKey, SortOrder)
GO

-- BTRPD_CustomerRiskForecastRecommendation
IF OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastRecommendation', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_CustomerRiskForecastRecommendation
(
    CustomerRiskForecastRecommendationId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerRiskForecastRecommendationId DEFAULT(''),
    SnapshotKey                          VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                            INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_SortOrder DEFAULT(0),
    RecommendationKey                    VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RecommendationKey DEFAULT(''),
    RecommendationLabel                  VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RecommendationLabel DEFAULT(''),
    CustomerCode                         VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerCode DEFAULT(''),
    CustomerName                         VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_CustomerName DEFAULT(''),
    Category                             VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_Category DEFAULT(''),
    ReasonText                           VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_ReasonText DEFAULT(''),
    RuleId                               VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_RuleId DEFAULT(''),
    ReportRoute                          VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_ReportRoute DEFAULT(''),
    DrillDownRoute                       VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastRecommendation_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastRecommendation PRIMARY KEY CLUSTERED (CustomerRiskForecastRecommendationId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey_SortOrder' AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerRiskForecastRecommendation'))
CREATE INDEX IX_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastRecommendation (SnapshotKey, SortOrder)
GO
