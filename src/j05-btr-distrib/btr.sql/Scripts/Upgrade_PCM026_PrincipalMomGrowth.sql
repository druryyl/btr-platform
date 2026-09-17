-- PCM-026 Principal Month-over-Month Growth snapshot.
-- Stores PRN-GRW-001 from stored PRN-SALES-001 month history.
-- Does not write or update PRN-SALES-001 and does not write PRN-GRW-002.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalMomGrowthKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalMomGrowthKpi
    (
        SnapshotKey              VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_SnapshotKey DEFAULT('CURRENT'),
        MomGrowthKpiId           VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_MomGrowthKpiId DEFAULT('PRN-GRW-001'),
        SalesOutKpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_SalesOutKpiId DEFAULT('PRN-SALES-001'),
        PeriodYear               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PeriodYear DEFAULT(0),
        PeriodMonth              INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PeriodMonth DEFAULT(0),
        PriorYear                INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PriorYear DEFAULT(0),
        PriorMonth               INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_PriorMonth DEFAULT(0),
        GeneratedAt              DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId         VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowthKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalMomGrowthKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalMomGrowth', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalMomGrowth
    (
        PrincipalMomGrowthId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PrincipalMomGrowthId DEFAULT(''),
        SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SnapshotKey DEFAULT('CURRENT'),
        MomGrowthKpiId         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_MomGrowthKpiId DEFAULT('PRN-GRW-001'),
        SalesOutKpiId          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SalesOutKpiId DEFAULT('PRN-SALES-001'),
        PeriodYear             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PeriodYear DEFAULT(0),
        PeriodMonth            INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PeriodMonth DEFAULT(0),
        PriorYear              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PriorYear DEFAULT(0),
        PriorMonth             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PriorMonth DEFAULT(0),
        SupplierId             VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SupplierId DEFAULT(''),
        SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SupplierName DEFAULT(''),
        CurrentSalesOutAmount  DECIMAL(18,2) NULL,
        PriorSalesOutAmount    DECIMAL(18,2) NULL,
        MomGrowthPercentage    DECIMAL(18,6) NULL,
        SortOrder              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SortOrder DEFAULT(0),
        GeneratedAt            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId       VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalMomGrowth PRIMARY KEY CLUSTERED (PrincipalMomGrowthId),
        CONSTRAINT UX_BTRPD_PrincipalMomGrowth_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalMomGrowth_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalMomGrowth'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalMomGrowth_SnapshotKey_SortOrder
        ON BTRPD_PrincipalMomGrowth (SnapshotKey, SortOrder)
END
GO
