CREATE TABLE BTRPD_PrincipalContributionException
(
    PrincipalContributionExceptionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_PrincipalContributionExceptionId DEFAULT(''),
    SnapshotKey                      VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SnapshotKey DEFAULT('CURRENT'),
    PeriodYear                       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_PeriodYear DEFAULT(0),
    PeriodMonth                      INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_PeriodMonth DEFAULT(0),
    SupplierId                       VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SupplierId DEFAULT(''),
    SupplierName                     VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SupplierName DEFAULT(''),
    SalesPersonId                    VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SalesPersonId DEFAULT(''),
    SalesPersonCode                  VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SalesPersonCode DEFAULT(''),
    SalesPersonName                  VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SalesPersonName DEFAULT(''),
    ContributionAmount               DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_ContributionAmount DEFAULT(0),
    LineCount                        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_LineCount DEFAULT(0),
    TargetYear                       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_TargetYear DEFAULT(0),
    TargetMonth                      INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_TargetMonth DEFAULT(0),
    SortOrder                        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_SortOrder DEFAULT(0),
    GeneratedAt                      DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId                 VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionException_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalContributionException PRIMARY KEY CLUSTERED (PrincipalContributionExceptionId),
    CONSTRAINT UX_BTRPD_PrincipalContributionException_SnapshotKey_SupplierId_SalesPersonId UNIQUE (SnapshotKey, SupplierId, SalesPersonId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalContributionException_SnapshotKey_SortOrder
    ON BTRPD_PrincipalContributionException (SnapshotKey, SortOrder)
GO
