-- Create BTR_SalesPersonSupplier junction table for Sales Person–Principal assignment.
-- Idempotent: skips objects that already exist.
-- Source of truth: btr.sql/Tables/SalesContext/BTR_SalesPersonSupplier.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTR_SalesPersonSupplier', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTR_SalesPersonSupplier]
(
	SalesPersonId VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesPersonSupplier_SalesPersonId DEFAULT(''),
	SupplierId    VARCHAR(5) NOT NULL CONSTRAINT DF_BTR_SalesPersonSupplier_SupplierId DEFAULT(''),

	CONSTRAINT PK_BTR_SalesPersonSupplier
		PRIMARY KEY CLUSTERED (SalesPersonId, SupplierId),
	CONSTRAINT FK_BTR_SalesPersonSupplier_SalesPerson
		FOREIGN KEY (SalesPersonId) REFERENCES BTR_SalesPerson (SalesPersonId),
	CONSTRAINT FK_BTR_SalesPersonSupplier_Supplier
		FOREIGN KEY (SupplierId) REFERENCES BTR_Supplier (SupplierId)
)
END
GO
