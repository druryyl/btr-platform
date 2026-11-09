-- Alter_BTR_ReturJual_Add_ReturnOrderId.sql
-- Add ReturnOrderId column and filtered index to BTR_ReturJual.
-- Idempotent: safe to run multiple times.

IF COL_LENGTH('dbo.BTR_ReturJual', 'ReturnOrderId') IS NULL
BEGIN
    ALTER TABLE dbo.BTR_ReturJual
        ADD ReturnOrderId VARCHAR(26) NOT NULL
            CONSTRAINT DF_BTR_ReturJual_ReturnOrderId DEFAULT('');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_ReturJual_ReturnOrderId'
      AND object_id = OBJECT_ID(N'dbo.BTR_ReturJual')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_BTR_ReturJual_ReturnOrderId
        ON dbo.BTR_ReturJual(ReturnOrderId, VoidDate)
        WHERE ReturnOrderId <> ''
        WITH (FILLFACTOR = 75);
END
GO
