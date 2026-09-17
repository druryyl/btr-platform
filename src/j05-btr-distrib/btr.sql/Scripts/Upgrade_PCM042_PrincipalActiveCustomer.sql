-- PCM-042 Principal Active Customer snapshot.
-- Stores PRN-CUS-001 counted from stored BTRPD_CustomerPrincipalRelationship
-- rows whose stored status is Active. Does not write projection status
-- and does not write PRN-SALES-001. No dashboard panel in this slice.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalActiveCustomerKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalActiveCustomerKpi
    (
        SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_SnapshotKey DEFAULT('CURRENT'),
        ActiveCustomerKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_ActiveCustomerKpiId DEFAULT('PRN-CUS-001'),
        AsOfDate               DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_AsOfDate DEFAULT('3000-01-01'),
        GeneratedAt            DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId       VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomerKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalActiveCustomerKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalActiveCustomer', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalActiveCustomer
    (
        PrincipalActiveCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_PrincipalActiveCustomerId DEFAULT(''),
        SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SnapshotKey DEFAULT('CURRENT'),
        ActiveCustomerKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_ActiveCustomerKpiId DEFAULT('PRN-CUS-001'),
        AsOfDate                  DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_AsOfDate DEFAULT('3000-01-01'),
        SupplierId                VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SupplierId DEFAULT(''),
        SupplierName              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SupplierName DEFAULT(''),
        ActiveCustomerCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_ActiveCustomerCount DEFAULT(0),
        SortOrder                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SortOrder DEFAULT(0),
        GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId          VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalActiveCustomer PRIMARY KEY CLUSTERED (PrincipalActiveCustomerId),
        CONSTRAINT UX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalActiveCustomer'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SortOrder
        ON BTRPD_PrincipalActiveCustomer (SnapshotKey, SortOrder)
END
GO
