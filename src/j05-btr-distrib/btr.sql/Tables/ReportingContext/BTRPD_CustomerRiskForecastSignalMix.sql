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
GO

CREATE INDEX IX_BTRPD_CustomerRiskForecastSignalMix_SnapshotKey_SortOrder
    ON BTRPD_CustomerRiskForecastSignalMix (SnapshotKey, SortOrder)
GO
