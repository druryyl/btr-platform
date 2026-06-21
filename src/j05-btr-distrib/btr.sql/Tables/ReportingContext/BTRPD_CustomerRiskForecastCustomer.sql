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
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastCustomer_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastCustomer (SnapshotKey, SortOrder)
GO
