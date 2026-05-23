CREATE TABLE BTR_SalesOmzet
(
    SalesOmzetId     VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SalesOmzetId DEFAULT(''),
    OrderId          VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OrderId DEFAULT(''),
    FakturId         VARCHAR(13) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturId DEFAULT(''),
    SaleKind         VARCHAR(15) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SaleKind DEFAULT(''),

    SalesDate        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SalesDate DEFAULT('3000-01-01'),
    OmzetDate        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OmzetDate DEFAULT('3000-01-01'),

    SalesPersonName  VARCHAR(30) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_SalesPersonName DEFAULT(''),
    OrderDate        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OrderDate DEFAULT('3000-01-01'),
    OrderTotal       DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OrderTotal DEFAULT(0),
    FakturCode       VARCHAR(11) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturCode DEFAULT(''),
    FakturDate       DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturDate DEFAULT('3000-01-01'),
    FakturTotal      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_FakturTotal DEFAULT(0),
    CustomerName     VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_CustomerName DEFAULT(''),
    Code             VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_Code DEFAULT(''),
    Alamat           VARCHAR(60) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_Alamat DEFAULT(''),
    OmzetStatus      VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_SalesOmzet_OmzetStatus DEFAULT(''),

    CreatedAt        DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_CreatedAt DEFAULT('3000-01-01'),
    LastReconciledAt DATETIME NOT NULL CONSTRAINT DF_BTR_SalesOmzet_LastReconciledAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTR_SalesOmzet PRIMARY KEY CLUSTERED (SalesOmzetId)
)
GO

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_OrderId
    ON BTR_SalesOmzet (OrderId)
    WHERE OrderId <> ''
GO

CREATE UNIQUE INDEX UX_BTR_SalesOmzet_FakturId
    ON BTR_SalesOmzet (FakturId)
    WHERE FakturId <> ''
GO

CREATE INDEX IX_BTR_SalesOmzet_SalesDate
    ON BTR_SalesOmzet (SalesDate, SalesOmzetId)
GO

CREATE INDEX IX_BTR_SalesOmzet_OmzetDate
    ON BTR_SalesOmzet (OmzetDate, SalesOmzetId)
GO
