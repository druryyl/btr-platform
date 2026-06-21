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
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastDist_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastDist (SnapshotKey, SortOrder)
GO
