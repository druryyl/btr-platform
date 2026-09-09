CREATE TABLE BTRPD_PrincipalMomGrowth
(
    PrincipalMomGrowthId   VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PrincipalMomGrowthId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SnapshotKey DEFAULT('CURRENT'),
    MomGrowthKpiId         VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_MomGrowthKpiId DEFAULT('PRN-GRW-001'),
    SalesOutKpiId          VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SalesOutKpiId DEFAULT('PRN-SALES-001'),
    PeriodYear             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PeriodYear DEFAULT(0),
    PeriodMonth            INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PeriodMonth DEFAULT(0),
    PriorYear              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PriorYear DEFAULT(0),
    PriorMonth             INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_PriorMonth DEFAULT(0),
    SupplierId             VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SupplierId DEFAULT(''),
    SupplierName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SupplierName DEFAULT(''),
    CurrentSalesOutAmount  DECIMAL(18,2) NULL,
    PriorSalesOutAmount    DECIMAL(18,2) NULL,
    MomGrowthPercentage    DECIMAL(18,6) NULL,
    SortOrder              INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_SortOrder DEFAULT(0),
    GeneratedAt            DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId       VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalMomGrowth_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalMomGrowth PRIMARY KEY CLUSTERED (PrincipalMomGrowthId),
    CONSTRAINT UX_BTRPD_PrincipalMomGrowth_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalMomGrowth_SnapshotKey_SortOrder
    ON BTRPD_PrincipalMomGrowth (SnapshotKey, SortOrder)
GO
