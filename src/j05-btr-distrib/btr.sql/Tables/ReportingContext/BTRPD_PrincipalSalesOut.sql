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
GO

CREATE INDEX IX_BTRPD_PrincipalSalesOut_SnapshotKey_SortOrder
    ON BTRPD_PrincipalSalesOut (SnapshotKey, SortOrder)
GO
