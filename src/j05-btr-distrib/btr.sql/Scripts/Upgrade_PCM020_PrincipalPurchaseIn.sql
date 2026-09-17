-- PCM-020 Principal Purchase-In snapshot.
-- Stores PRN-PUR-001 only, calculated from Purchase Detail (BTR_InvoiceItem).
-- Does not write PRN-SALES-001 and does not alter PU-KPI-001.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalPurchaseInKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalPurchaseInKpi
    (
        SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_SnapshotKey DEFAULT('CURRENT'),
        KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_KpiId DEFAULT('PRN-PUR-001'),
        PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_PeriodYear DEFAULT(0),
        PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_PeriodMonth DEFAULT(0),
        GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseInKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalPurchaseInKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalPurchaseIn', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalPurchaseIn
    (
        PrincipalPurchaseInId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PrincipalPurchaseInId DEFAULT(''),
        SnapshotKey           VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SnapshotKey DEFAULT('CURRENT'),
        KpiId                 VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_KpiId DEFAULT('PRN-PUR-001'),
        PeriodYear            INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PeriodYear DEFAULT(0),
        PeriodMonth           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PeriodMonth DEFAULT(0),
        SupplierId            VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SupplierId DEFAULT(''),
        SupplierName          VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SupplierName DEFAULT(''),
        PurchaseInAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_PurchaseInAmount DEFAULT(0),
        LineCount             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_LineCount DEFAULT(0),
        SortOrder             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_SortOrder DEFAULT(0),
        GeneratedAt           DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId      VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalPurchaseIn_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalPurchaseIn PRIMARY KEY CLUSTERED (PrincipalPurchaseInId),
        CONSTRAINT UX_BTRPD_PrincipalPurchaseIn_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalPurchaseIn_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalPurchaseIn'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalPurchaseIn_SnapshotKey_SortOrder
        ON BTRPD_PrincipalPurchaseIn (SnapshotKey, SortOrder)
END
GO
