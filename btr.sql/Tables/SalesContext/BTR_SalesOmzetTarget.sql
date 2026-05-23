CREATE TABLE BTR_SalesOmzetTarget
(
    SalesPersonId VARCHAR(5)  NOT NULL,
    TargetYear    INT         NOT NULL,
    TargetMonth   INT         NOT NULL,
    TargetAmount  DECIMAL(18, 2) NOT NULL CONSTRAINT DF_BTR_SalesOmzetTarget_TargetAmount DEFAULT (0),

    CONSTRAINT PK_BTR_SalesOmzetTarget PRIMARY KEY CLUSTERED (SalesPersonId, TargetYear, TargetMonth),
    CONSTRAINT FK_BTR_SalesOmzetTarget_SalesPerson
        FOREIGN KEY (SalesPersonId) REFERENCES BTR_SalesPerson (SalesPersonId)
)
GO

CREATE INDEX IX_BTR_SalesOmzetTarget_YearMonth
    ON BTR_SalesOmzetTarget (TargetYear, TargetMonth)
GO
