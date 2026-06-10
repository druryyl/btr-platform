-- Create BTR_SalesPersonPrincipalTarget for Sales Person–Principal monthly targets.
-- Idempotent: skips objects that already exist.
-- Source of truth: btr.sql/Tables/SalesContext/BTR_SalesPersonPrincipalTarget.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTR_SalesPersonPrincipalTarget', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTR_SalesPersonPrincipalTarget]
(
	SalesPersonId VARCHAR(5)  NOT NULL,
	SupplierId    VARCHAR(5)  NOT NULL,
	TargetYear    INT         NOT NULL,
	TargetMonth   INT         NOT NULL,
	TargetAmount  DECIMAL(18, 2) NOT NULL
		CONSTRAINT DF_BTR_SalesPersonPrincipalTarget_TargetAmount DEFAULT (0),
	UpdatedDate   DATETIME    NOT NULL
		CONSTRAINT DF_BTR_SalesPersonPrincipalTarget_UpdatedDate DEFAULT (GETDATE()),

	CONSTRAINT PK_BTR_SalesPersonPrincipalTarget
		PRIMARY KEY CLUSTERED (SalesPersonId, SupplierId, TargetYear, TargetMonth),

	CONSTRAINT FK_BTR_SalesPersonPrincipalTarget_SalesPerson
		FOREIGN KEY (SalesPersonId) REFERENCES BTR_SalesPerson (SalesPersonId),

	CONSTRAINT FK_BTR_SalesPersonPrincipalTarget_Supplier
		FOREIGN KEY (SupplierId) REFERENCES BTR_Supplier (SupplierId),

	CONSTRAINT CK_BTR_SalesPersonPrincipalTarget_TargetMonth
		CHECK (TargetMonth BETWEEN 1 AND 12),

	CONSTRAINT CK_BTR_SalesPersonPrincipalTarget_TargetAmount
		CHECK (TargetAmount >= 0)
)
END
GO

IF NOT EXISTS (
	SELECT 1 FROM sys.indexes
	WHERE name = N'IX_BTR_SalesPersonPrincipalTarget_YearMonth'
	  AND object_id = OBJECT_ID(N'dbo.BTR_SalesPersonPrincipalTarget'))
BEGIN
CREATE INDEX IX_BTR_SalesPersonPrincipalTarget_YearMonth
	ON BTR_SalesPersonPrincipalTarget (TargetYear, TargetMonth)
	INCLUDE (SalesPersonId, SupplierId, TargetAmount)
END
GO

IF NOT EXISTS (
	SELECT 1 FROM sys.indexes
	WHERE name = N'IX_BTR_SalesPersonPrincipalTarget_SalesPerson_Period'
	  AND object_id = OBJECT_ID(N'dbo.BTR_SalesPersonPrincipalTarget'))
BEGIN
CREATE INDEX IX_BTR_SalesPersonPrincipalTarget_SalesPerson_Period
	ON BTR_SalesPersonPrincipalTarget (SalesPersonId, TargetYear, TargetMonth)
	INCLUDE (SupplierId, TargetAmount)
END
GO
