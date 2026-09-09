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
GO

CREATE INDEX IX_BTRPD_CustomerPrincipalRelationship_SupplierId_Status
    ON BTRPD_CustomerPrincipalRelationship (SupplierId, RelationshipStatus)
GO

CREATE INDEX IX_BTRPD_CustomerPrincipalRelationship_CustomerId
    ON BTRPD_CustomerPrincipalRelationship (CustomerId)
GO
