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
GO

CREATE INDEX IX_BTRPD_PrincipalPurchaseIn_SnapshotKey_SortOrder
    ON BTRPD_PrincipalPurchaseIn (SnapshotKey, SortOrder)
GO
