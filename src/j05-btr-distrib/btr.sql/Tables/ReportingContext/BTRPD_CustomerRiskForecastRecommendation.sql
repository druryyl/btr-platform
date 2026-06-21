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
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastRecommendation_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastRecommendation (SnapshotKey, SortOrder)
GO
