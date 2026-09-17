CREATE TABLE BTRPD_PrincipalContribution
(
    PrincipalContributionId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_PrincipalContributionId DEFAULT(''),
    SnapshotKey             VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SnapshotKey DEFAULT('CURRENT'),
    SourceSalesOutKpiId     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SourceSalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_PeriodYear DEFAULT(0),
    PeriodMonth             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_PeriodMonth DEFAULT(0),
    SupplierId              VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SupplierId DEFAULT(''),
    SupplierName            VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SupplierName DEFAULT(''),
    SalesPersonId           VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SalesPersonId DEFAULT(''),
    SalesPersonCode         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SalesPersonCode DEFAULT(''),
    SalesPersonName         VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SalesPersonName DEFAULT(''),
    ContributionAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_ContributionAmount DEFAULT(0),
    LineCount               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_LineCount DEFAULT(0),
    HasTargetResponsibility BIT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_HasTargetResponsibility DEFAULT(0),
    SortOrder               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_SortOrder DEFAULT(0),
    GeneratedAt             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId        VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalContribution_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalContribution PRIMARY KEY CLUSTERED (PrincipalContributionId),
    CONSTRAINT UX_BTRPD_PrincipalContribution_SnapshotKey_SupplierId_SalesPersonId UNIQUE (SnapshotKey, SupplierId, SalesPersonId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalContribution_SnapshotKey_SortOrder
    ON BTRPD_PrincipalContribution (SnapshotKey, SortOrder)
GO

CREATE INDEX IX_BTRPD_PrincipalContribution_SnapshotKey_SupplierId
    ON BTRPD_PrincipalContribution (SnapshotKey, SupplierId)
GO
