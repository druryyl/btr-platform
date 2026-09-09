-- PCM-006 Principal Target snapshot.
-- Stores PRN-TGT-001 only, derived from BTR_SalesPersonPrincipalTarget.
-- Does not write an independent Principal Target record.
-- Does not alter BTR_SalesPersonPrincipalTarget or BTR_SalesPersonSupplier.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalTargetKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalTargetKpi
    (
        SnapshotKey      VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_SnapshotKey DEFAULT('CURRENT'),
        KpiId            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_KpiId DEFAULT('PRN-TGT-001'),
        PeriodYear       INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_PeriodYear DEFAULT(0),
        PeriodMonth      INT         NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_PeriodMonth DEFAULT(0),
        GeneratedAt      DATETIME    NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTargetKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalTargetKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_PrincipalTarget', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_PrincipalTarget
    (
        PrincipalTargetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PrincipalTargetId DEFAULT(''),
        SnapshotKey       VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SnapshotKey DEFAULT('CURRENT'),
        KpiId             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_KpiId DEFAULT('PRN-TGT-001'),
        PeriodYear        INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PeriodYear DEFAULT(0),
        PeriodMonth       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_PeriodMonth DEFAULT(0),
        SupplierId        VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SupplierId DEFAULT(''),
        SupplierName      VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SupplierName DEFAULT(''),
        TargetAmount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_TargetAmount DEFAULT(0),
        SourceCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SourceCount DEFAULT(0),
        SortOrder         INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_SortOrder DEFAULT(0),
        GeneratedAt       DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId  VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalTarget_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_PrincipalTarget PRIMARY KEY CLUSTERED (PrincipalTargetId),
        CONSTRAINT UX_BTRPD_PrincipalTarget_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_PrincipalTarget_SnapshotKey_SortOrder'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_PrincipalTarget'))
BEGIN
    CREATE INDEX IX_BTRPD_PrincipalTarget_SnapshotKey_SortOrder
        ON BTRPD_PrincipalTarget (SnapshotKey, SortOrder)
END
GO
