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
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastAttention_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastAttention (SnapshotKey, SortOrder)
GO
