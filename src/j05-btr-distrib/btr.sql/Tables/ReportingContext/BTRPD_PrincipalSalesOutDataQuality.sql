CREATE TABLE BTRPD_PrincipalSalesOutDataQuality
(
    PrincipalSalesOutDataQualityId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_PrincipalSalesOutDataQualityId DEFAULT(''),
    SnapshotKey                    VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey DEFAULT('CURRENT'),
    PeriodYear                     INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_PeriodYear DEFAULT(0),
    PeriodMonth                    INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_PeriodMonth DEFAULT(0),
    ExceptionCode                  VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_ExceptionCode DEFAULT(''),
    Amount                         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_Amount DEFAULT(0),
    LineCount                      INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_LineCount DEFAULT(0),
    GeneratedAt                    DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId               VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutDataQuality_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalSalesOutDataQuality PRIMARY KEY CLUSTERED (PrincipalSalesOutDataQualityId),
    CONSTRAINT UX_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey_ExceptionCode UNIQUE (SnapshotKey, ExceptionCode)
)
GO

CREATE INDEX IX_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey_ExceptionCode
    ON BTRPD_PrincipalSalesOutDataQuality (SnapshotKey, ExceptionCode)
GO
