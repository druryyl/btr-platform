-- PCM-004 Principal Sales-Out current snapshot.
-- Stores PRN-SALES-001 only. Does not store return KPIs.
-- Does not add SupplierId to BTR_FakturItem.
-- Does not alter BTRPD_SalesmanPrincipalAchievement.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOutKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalSalesOutKpi
    (
        SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_SnapshotKey DEFAULT('CURRENT'),
        KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_KpiId DEFAULT('PRN-SALES-001'),
        PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_PeriodYear DEFAULT(0),
        PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_PeriodMonth DEFAULT(0),
        GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalSalesOutKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOut', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalSalesOut
    (
        PrincipalSalesOutId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_PrincipalSalesOutId DEFAULT(''),
        SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SnapshotKey DEFAULT('CURRENT'),
        KpiId               VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_KpiId DEFAULT('PRN-SALES-001'),
        PeriodYear          INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_PeriodYear DEFAULT(0),
        PeriodMonth         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_PeriodMonth DEFAULT(0),
        SupplierId          VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SupplierId DEFAULT(''),
        SupplierName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SupplierName DEFAULT(''),
        SalesOutAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SalesOutAmount DEFAULT(0),
        LineCount           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_LineCount DEFAULT(0),
        SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_SortOrder DEFAULT(0),
        GeneratedAt         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOut_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalSalesOut PRIMARY KEY CLUSTERED (PrincipalSalesOutId),
        CONSTRAINT UX_BTRPD_PrincipalSalesOut_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalSalesOut_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOut'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalSalesOut_SnapshotKey_SortOrder
        ON BTRPD_PrincipalSalesOut (SnapshotKey, SortOrder)
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOutDataQuality', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey_ExceptionCode'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOutDataQuality'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalSalesOutDataQuality_SnapshotKey_ExceptionCode
        ON BTRPD_PrincipalSalesOutDataQuality (SnapshotKey, ExceptionCode)
END
GO
