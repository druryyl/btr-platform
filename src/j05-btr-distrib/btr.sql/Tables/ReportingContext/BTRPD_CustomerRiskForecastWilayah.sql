CREATE TABLE BTRPD_CustomerRiskForecastWilayah
(
    CustomerRiskForecastWilayahId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_CustomerRiskForecastWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SnapshotKey DEFAULT('CURRENT'),
    WilayahName                   VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_WilayahName DEFAULT(''),
    ElevatedRiskCustomerCount     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_ElevatedRiskCustomerCount DEFAULT(0),
    SortOrder                     INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerRiskForecastWilayah_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerRiskForecastWilayah PRIMARY KEY CLUSTERED (CustomerRiskForecastWilayahId)
)
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastWilayah_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastWilayah (SnapshotKey, SortOrder)
GO
