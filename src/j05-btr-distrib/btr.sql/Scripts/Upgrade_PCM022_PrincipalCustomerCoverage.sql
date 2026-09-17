-- PCM-022 Principal Customer Coverage snapshot.
-- Stores PRN-CUS-002 computed from the stored BTRPD_CustomerPrincipalRelationship
-- projection population and stored PRN-CUS-001. Does not write projection status
-- and does not write PRN-SALES-001. No dashboard panel in this slice.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalCustomerCoverageKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalCustomerCoverageKpi
    (
        SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_SnapshotKey DEFAULT('CURRENT'),
        CustomerCoverageKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_CustomerCoverageKpiId DEFAULT('PRN-CUS-002'),
        AsOfDate               DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_AsOfDate DEFAULT('3000-01-01'),
        GeneratedAt            DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId       VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverageKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalCustomerCoverageKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalCustomerCoverage', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalCustomerCoverage
    (
        PrincipalCustomerCoverageId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_PrincipalCustomerCoverageId DEFAULT(''),
        SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SnapshotKey DEFAULT('CURRENT'),
        CustomerCoverageKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_CustomerCoverageKpiId DEFAULT('PRN-CUS-002'),
        AsOfDate                    DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_AsOfDate DEFAULT('3000-01-01'),
        SupplierId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SupplierId DEFAULT(''),
        SupplierName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SupplierName DEFAULT(''),
        ActiveCustomerCount         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_ActiveCustomerCount DEFAULT(0),
        TotalCustomerCount          INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_TotalCustomerCount DEFAULT(0),
        CoveragePercentage          DECIMAL(18,6) NULL,
        SortOrder                   INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_SortOrder DEFAULT(0),
        GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId            VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalCustomerCoverage_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalCustomerCoverage PRIMARY KEY CLUSTERED (PrincipalCustomerCoverageId),
        CONSTRAINT UX_BTRPD_PrincipalCustomerCoverage_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalCustomerCoverage_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalCustomerCoverage'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalCustomerCoverage_SnapshotKey_SortOrder
        ON BTRPD_PrincipalCustomerCoverage (SnapshotKey, SortOrder)
END
GO
