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
GO

CREATE INDEX IX_BTRPD_PrincipalSalesOutHistory_SupplierId_Period
    ON BTRPD_PrincipalSalesOutHistory (SupplierId, PeriodYear, PeriodMonth)
GO
