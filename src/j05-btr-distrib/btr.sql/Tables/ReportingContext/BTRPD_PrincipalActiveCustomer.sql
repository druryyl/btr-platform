CREATE TABLE BTRPD_PrincipalActiveCustomer
(
    PrincipalActiveCustomerId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_PrincipalActiveCustomerId DEFAULT(''),
    SnapshotKey               VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SnapshotKey DEFAULT('CURRENT'),
    ActiveCustomerKpiId       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_ActiveCustomerKpiId DEFAULT('PRN-CUS-001'),
    AsOfDate                  DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_AsOfDate DEFAULT('3000-01-01'),
    SupplierId                VARCHAR(5)    NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SupplierId DEFAULT(''),
    SupplierName              VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SupplierName DEFAULT(''),
    ActiveCustomerCount       INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_ActiveCustomerCount DEFAULT(0),
    SortOrder                 INT           NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_SortOrder DEFAULT(0),
    GeneratedAt               DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_GeneratedAt DEFAULT('3000-01-01'),
    LastRefreshLogId          VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PrincipalActiveCustomer_LastRefreshLogId DEFAULT(''),

    CONSTRAINT PK_BTRPD_PrincipalActiveCustomer PRIMARY KEY CLUSTERED (PrincipalActiveCustomerId),
    CONSTRAINT UX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SupplierId UNIQUE (SnapshotKey, SupplierId)
)
GO

CREATE INDEX IX_BTRPD_PrincipalActiveCustomer_SnapshotKey_SortOrder
    ON BTRPD_PrincipalActiveCustomer (SnapshotKey, SortOrder)
GO
