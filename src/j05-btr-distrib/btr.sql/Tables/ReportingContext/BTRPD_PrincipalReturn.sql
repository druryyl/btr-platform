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
GO

CREATE INDEX IX_BTRPD_PrincipalReturn_SnapshotKey_SortOrder
    ON BTRPD_PrincipalReturn (SnapshotKey, SortOrder)
GO
