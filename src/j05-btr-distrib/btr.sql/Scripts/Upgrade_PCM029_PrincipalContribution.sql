-- PCM-029 Principal x Salesman contribution snapshot.
-- Stores Principal x Salesman commercial contribution decomposed from PRN-SALES-001
-- Faktur Item evidence by Faktur.SalesPersonId, plus missing-target responsibility
-- exceptions. Contribution is not a registry KPI. Does not write PRN-SALES-001.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalContributionKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalContributionKpi
    (
        SnapshotKey          VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_SnapshotKey DEFAULT('CURRENT'),
        SourceSalesOutKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_SourceSalesOutKpiId DEFAULT('PRN-SALES-001'),
        PeriodYear           INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_PeriodYear DEFAULT(0),
        PeriodMonth          INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_PeriodMonth DEFAULT(0),
        GeneratedAt          DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId     VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalContributionKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalContributionKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalContribution', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalContribution_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalContribution'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalContribution_SnapshotKey_SortOrder
        ON BTRPD_PrincipalContribution (SnapshotKey, SortOrder)
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalContribution_SnapshotKey_SupplierId'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalContribution'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalContribution_SnapshotKey_SupplierId
        ON BTRPD_PrincipalContribution (SnapshotKey, SupplierId)
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalContributionException', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalContributionException_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalContributionException'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalContributionException_SnapshotKey_SortOrder
        ON BTRPD_PrincipalContributionException (SnapshotKey, SortOrder)
END
GO
