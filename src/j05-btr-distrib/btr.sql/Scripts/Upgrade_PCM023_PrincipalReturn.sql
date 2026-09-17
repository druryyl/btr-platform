-- PCM-023 Principal return amount snapshot.
-- Stores PRN-RET-001, PRN-RET-002, and PRN-RET-003 from Return Item evidence.
-- Does not write PRN-SALES-001 or PRN-RET-004.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalReturnKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalReturnKpi
    (
        SnapshotKey        VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_SnapshotKey DEFAULT('CURRENT'),
        GoodReturnKpiId    VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_GoodReturnKpiId DEFAULT('PRN-RET-001'),
        BrokenReturnKpiId  VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
        TotalReturnKpiId   VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_TotalReturnKpiId DEFAULT('PRN-RET-003'),
        PeriodYear         INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_PeriodYear DEFAULT(0),
        PeriodMonth        INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_PeriodMonth DEFAULT(0),
        GeneratedAt        DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId   VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalReturnKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalReturn', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalReturn
    (
        PrincipalReturnId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_PrincipalReturnId DEFAULT(''),
        SnapshotKey         VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SnapshotKey DEFAULT('CURRENT'),
        GoodReturnKpiId     VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_GoodReturnKpiId DEFAULT('PRN-RET-001'),
        BrokenReturnKpiId   VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
        TotalReturnKpiId    VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_TotalReturnKpiId DEFAULT('PRN-RET-003'),
        PeriodYear          INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_PeriodYear DEFAULT(0),
        PeriodMonth         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_PeriodMonth DEFAULT(0),
        SupplierId          VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SupplierId DEFAULT(''),
        SupplierName        VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SupplierName DEFAULT(''),
        GoodReturnAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_GoodReturnAmount DEFAULT(0),
        BrokenReturnAmount  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_BrokenReturnAmount DEFAULT(0),
        TotalReturnAmount   DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_TotalReturnAmount DEFAULT(0),
        LineCount           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_LineCount DEFAULT(0),
        SortOrder           INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_SortOrder DEFAULT(0),
        GeneratedAt         DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId    VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturn_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalReturn PRIMARY KEY CLUSTERED (PrincipalReturnId),
        CONSTRAINT UX_BTRPD_PrincipalReturn_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalReturn_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalReturn'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalReturn_SnapshotKey_SortOrder
        ON BTRPD_PrincipalReturn (SnapshotKey, SortOrder)
END
GO
