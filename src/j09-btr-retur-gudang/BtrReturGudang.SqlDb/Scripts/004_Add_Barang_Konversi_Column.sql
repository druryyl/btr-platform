-- ============================================================================
-- Script: 004_Add_Barang_Konversi_Column.sql
-- Description: Upgrade BTRG_Barang table to add Konversi column for auto-populating
--              conversion multiplier in barcode registration dialogs.
-- Idempotent: This script can be executed multiple times without errors.
-- ============================================================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barang' AND type = 'U')
BEGIN
    -- 1. Add Konversi column with default constraint if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Konversi')
    BEGIN
        ALTER TABLE BTRG_Barang
        ADD Konversi INT NOT NULL CONSTRAINT DF_BTRG_Barang_Konversi DEFAULT 1;
    END;

    -- 2. Backfill existing rows with invalid, zero, or null conversion
    UPDATE BTRG_Barang
    SET Konversi = 1
    WHERE Konversi <= 0 OR Konversi IS NULL;
END;