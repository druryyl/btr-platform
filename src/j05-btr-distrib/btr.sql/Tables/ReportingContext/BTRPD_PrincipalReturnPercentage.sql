CREATE TABLE BTRPD_PrincipalReturnPercentage
(
    PrincipalReturnPercentageId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_PrincipalReturnPercentageId DEFAULT(''),
    SnapshotKey                 VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SnapshotKey DEFAULT('CURRENT'),
    ReturnPercentageKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_ReturnPercentageKpiId DEFAULT('PRN-RET-004'),
    SalesOutKpiId               VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    TotalReturnKpiId            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear                  INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_PeriodYear DEFAULT(0),
    PeriodMonth                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_PeriodMonth DEFAULT(0),
    SupplierId                  VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SupplierId DEFAULT(''),
    SupplierName                VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SupplierName DEFAULT(''),
    TotalReturnAmount           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_TotalReturnAmount DEFAULT(0),
    SalesOutAmount              DECIMAL(18,2) NULL,
    ReturnPercentage            DECIMAL(18,6) NULL,
    SortOrder                   INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_SortOrder DEFAULT(0),
    GeneratedAt                 DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId            VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnPercentage_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnPercentage PRIMARY KEY CLUSTERED (PrincipalReturnPercentageId),
    CONSTRAINT UX_BTRPD_PrincipalReturnPercentage_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalReturnPercentage_SnapshotKey_SortOrder
    ON BTRPD_PrincipalReturnPercentage (SnapshotKey, SortOrder)
GO
