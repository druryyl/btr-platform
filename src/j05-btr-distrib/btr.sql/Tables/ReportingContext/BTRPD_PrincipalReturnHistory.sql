CREATE TABLE BTRPD_PrincipalReturnHistory
(
    PrincipalReturnHistoryId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_PrincipalReturnHistoryId DEFAULT(''),
    GoodReturnKpiId          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_GoodReturnKpiId DEFAULT('PRN-RET-001'),
    BrokenReturnKpiId        VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_BrokenReturnKpiId DEFAULT('PRN-RET-002'),
    TotalReturnKpiId         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_TotalReturnKpiId DEFAULT('PRN-RET-003'),
    PeriodYear               INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_PeriodYear DEFAULT(0),
    PeriodMonth              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_PeriodMonth DEFAULT(0),
    SupplierId               VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_SupplierId DEFAULT(''),
    SupplierName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_SupplierName DEFAULT(''),
    GoodReturnAmount         DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_GoodReturnAmount DEFAULT(0),
    BrokenReturnAmount       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_BrokenReturnAmount DEFAULT(0),
    TotalReturnAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_TotalReturnAmount DEFAULT(0),
    LineCount                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_LineCount DEFAULT(0),
    SortOrder                INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_SortOrder DEFAULT(0),
    GeneratedAt              DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId         VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalReturnHistory_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalReturnHistory PRIMARY KEY CLUSTERED (PrincipalReturnHistoryId),
    CONSTRAINT UX_BTRPD_PrincipalReturnHistory_Period_SupplierId UNIQUE (PeriodYear, PeriodMonth, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalReturnHistory_SupplierId_Period
    ON BTRPD_PrincipalReturnHistory (SupplierId, PeriodYear, PeriodMonth)
GO
