-- PCM-013 Customer-Principal relationship projection.
-- Stores pair identity, first and last transaction dates, Active or Dormant status,
-- and pair-attributed PRN-SALES-001. Does not store PRN-CUS-001, PRN-CUS-002, or PRN-RET-*.
-- Does not add a Customer-Principal master table or Entity Analytics entity type.
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRPD_CustomerPrincipalRelationshipKpi', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_CustomerPrincipalRelationshipKpi
    (
        SnapshotKey          VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_SnapshotKey DEFAULT('CURRENT'),
        KpiId                VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_KpiId DEFAULT('PRN-SALES-001'),
        AsOfDate             DATETIME     NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_AsOfDate DEFAULT('3000-01-01'),
        HistoricalLimitation VARCHAR(300) NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_HistoricalLimitation DEFAULT(''),
        GeneratedAt          DATETIME     NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId     VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationshipKpi_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_CustomerPrincipalRelationshipKpi PRIMARY KEY CLUSTERED (SnapshotKey)
    )
END
GO

IF OBJECT_ID(N'dbo.BTRPD_CustomerPrincipalRelationship', N'U') IS NULL
BEGIN
    CREATE TABLE BTRPD_CustomerPrincipalRelationship
    (
        CustomerPrincipalRelationshipId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_CustomerPrincipalRelationshipId DEFAULT(''),
        CustomerId                      VARCHAR(6)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_CustomerId DEFAULT(''),
        CustomerName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_CustomerName DEFAULT(''),
        SupplierId                      VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_SupplierId DEFAULT(''),
        SupplierName                    VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_SupplierName DEFAULT(''),
        FirstTransactionDate            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_FirstTransactionDate DEFAULT('3000-01-01'),
        LastTransactionDate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_LastTransactionDate DEFAULT('3000-01-01'),
        RelationshipStatus              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_RelationshipStatus DEFAULT(''),
        KpiId                           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_KpiId DEFAULT('PRN-SALES-001'),
        SalesOutAmount                  DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_SalesOutAmount DEFAULT(0),
        LineCount                       INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_LineCount DEFAULT(0),
        AsOfDate                        DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_AsOfDate DEFAULT('3000-01-01'),
        GeneratedAt                     DATETIME      NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_GeneratedAt DEFAULT('3000-01-01'),
        LastRefreshLogId                VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPrincipalRelationship_LastRefreshLogId DEFAULT(''),

        CONSTRAINT PK_BTRPD_CustomerPrincipalRelationship PRIMARY KEY CLUSTERED (CustomerPrincipalRelationshipId),
        CONSTRAINT UX_BTRPD_CustomerPrincipalRelationship_CustomerId_SupplierId UNIQUE (CustomerId, SupplierId)
    )
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_CustomerPrincipalRelationship_SupplierId_Status'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerPrincipalRelationship'))
BEGIN
    CREATE INDEX IX_BTRPD_CustomerPrincipalRelationship_SupplierId_Status
        ON BTRPD_CustomerPrincipalRelationship (SupplierId, RelationshipStatus)
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_BTRPD_CustomerPrincipalRelationship_CustomerId'
      AND object_id = OBJECT_ID(N'dbo.BTRPD_CustomerPrincipalRelationship'))
BEGIN
    CREATE INDEX IX_BTRPD_CustomerPrincipalRelationship_CustomerId
        ON BTRPD_CustomerPrincipalRelationship (CustomerId)
END
GO
