-- PCM-027 Principal Year-over-Year Growth snapshot.
-- Stores PRN-GRW-002 from stored PRN-SALES-001 month history.
-- Does not write or update PRN-SALES-001 and does not write PRN-GRW-001.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalYoyGrowthKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalYoyGrowthKpi
    (
        SnapshotKey              VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_SnapshotKey DEFAULT('CURRENT'),
        YoyGrowthKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_YoyGrowthKpiId DEFAULT('PRN-GRW-002'),
        SalesOutKpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
        PeriodYear               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PeriodYear DEFAULT(0),
        PeriodMonth              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PeriodMonth DEFAULT(0),
        PriorYear                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PriorYear DEFAULT(0),
        PriorMonth               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_PriorMonth DEFAULT(0),
        GeneratedAt              DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId         VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowthKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalYoyGrowthKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalYoyGrowth', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalYoyGrowth
    (
        PrincipalYoyGrowthId     VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PrincipalYoyGrowthId DEFAULT(''),
        SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SnapshotKey DEFAULT('CURRENT'),
        YoyGrowthKpiId           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_YoyGrowthKpiId DEFAULT('PRN-GRW-002'),
        SalesOutKpiId            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SalesOutKpiId DEFAULT('PRN-SALES-001'),
        PeriodYear               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PeriodYear DEFAULT(0),
        PeriodMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PeriodMonth DEFAULT(0),
        PriorYear                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PriorYear DEFAULT(0),
        PriorMonth               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_PriorMonth DEFAULT(0),
        SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SupplierId DEFAULT(''),
        SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SupplierName DEFAULT(''),
        CurrentSalesOutAmount    DECIMAL(18,2) NULL,
        PriorSalesOutAmount      DECIMAL(18,2) NULL,
        YoyGrowthPercentage      DECIMAL(18,6) NULL,
        SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_SortOrder DEFAULT(0),
        GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId         VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalYoyGrowth_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalYoyGrowth PRIMARY KEY CLUSTERED (PrincipalYoyGrowthId),
        CONSTRAINT UX_BTRPD_PrincipalYoyGrowth_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalYoyGrowth_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalYoyGrowth'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalYoyGrowth_SnapshotKey_SortOrder
        ON BTRPD_PrincipalYoyGrowth (SnapshotKey, SortOrder)
END
GO
