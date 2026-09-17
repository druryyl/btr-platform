-- PCM-051: add SupplierId to inventory dashboard snapshot breakdown tables
-- so IN01 / IN02 supplier (Principal) exposures can cross-link to SA04
-- (Principal Performance) for the same Principal.
-- Idempotent: safe to re-run when column already exists.

IF COL_LENGTH('BTRPD_InventoryBreakdown', 'SupplierId') IS NULL
BEGIN
    ALTER TABLE BTRPD_InventoryBreakdown
        ADD SupplierId VARCHAR(5) NOT NULL
            CONSTRAINT DF_BTRPD_InventoryBreakdown_SupplierId DEFAULT('');
END
GO

IF COL_LENGTH('BTRPD_InventoryRiskBreakdown', 'SupplierId') IS NULL
BEGIN
    ALTER TABLE BTRPD_InventoryRiskBreakdown
        ADD SupplierId VARCHAR(5) NOT NULL
            CONSTRAINT DF_BTRPD_InventoryRiskBreakdown_SupplierId DEFAULT('');
END
GO