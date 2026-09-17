-- PCM-005 Principal Sales-Out monthly history.
-- Stores PRN-SALES-001 only. Does not store return history.
-- Does not add SupplierId to BTR_FakturItem.
-- Does not read Purchasing Management SalesOutAmount.
-- Item Principal is current Item master; historical reconstruction may be limited.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOutHistoryKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalSalesOutHistoryKpi
    (
        SnapshotKey           VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_SnapshotKey DEFAULT('HISTORY'),
        KpiId                 VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_KpiId DEFAULT('PRN-SALES-001'),
        HistoricalLimitation  VARCHAR(300) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_HistoricalLimitation DEFAULT(''),
        GeneratedAt           DATETIME     NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId      VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistoryKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalSalesOutHistoryKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOutHistory', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalSalesOutHistory
    (
        PrincipalSalesOutHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_PrincipalSalesOutHistoryId DEFAULT(''),
        KpiId                      VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_KpiId DEFAULT('PRN-SALES-001'),
        PeriodYear                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_PeriodYear DEFAULT(0),
        PeriodMonth                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_PeriodMonth DEFAULT(0),
        SupplierId                 VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SupplierId DEFAULT(''),
        SupplierName               VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SupplierName DEFAULT(''),
        SalesOutAmount             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SalesOutAmount DEFAULT(0),
        LineCount                  INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_LineCount DEFAULT(0),
        SortOrder                  INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_SortOrder DEFAULT(0),
        GeneratedAt                DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId           VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalSalesOutHistory_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalSalesOutHistory PRIMARY KEY CLUSTERED (PrincipalSalesOutHistoryId),
        CONSTRAINT UX_BTRPD_PrincipalSalesOutHistory_Period_SupplierId UNIQUE (PeriodYear, PeriodMonth, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalSalesOutHistory_SupplierId_Period'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalSalesOutHistory'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalSalesOutHistory_SupplierId_Period
        ON BTRPD_PrincipalSalesOutHistory (SupplierId, PeriodYear, PeriodMonth)
END
GO
