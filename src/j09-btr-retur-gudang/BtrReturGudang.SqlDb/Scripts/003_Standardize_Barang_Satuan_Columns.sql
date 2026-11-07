-- ============================================================================
-- Script: 003_Standardize_Barang_Satuan_Columns.sql
-- Description: Upgrade BTRG_Barang table to standardize packaging unit columns
--              from Satuan1, Satuan2, Satuan3 to SatBesar and SatKecil.
-- ============================================================================

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'BTRG_Barang' AND type = 'U')
BEGIN
    -- 1. Add SatBesar if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'SatBesar')
    BEGIN
        ALTER TABLE BTRG_Barang ADD SatBesar VARCHAR(7) NULL;
    END;

    -- 2. Add SatKecil if missing
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'SatKecil')
    BEGIN
        ALTER TABLE BTRG_Barang ADD SatKecil VARCHAR(7) NULL;
    END;

    -- 3. Copy legacy data if legacy columns exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan1')
    BEGIN
        EXEC('UPDATE BTRG_Barang SET SatBesar = Satuan1 WHERE SatBesar IS NULL AND Satuan1 IS NOT NULL;');
    END;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan2')
    BEGIN
        EXEC('UPDATE BTRG_Barang SET SatKecil = Satuan2 WHERE SatKecil IS NULL AND Satuan2 IS NOT NULL;');
    END;

    -- 4. Drop legacy columns
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan1')
    BEGIN
        ALTER TABLE BTRG_Barang DROP COLUMN Satuan1;
    END;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan2')
    BEGIN
        ALTER TABLE BTRG_Barang DROP COLUMN Satuan2;
    END;

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BTRG_Barang') AND name = 'Satuan3')
    BEGIN
        ALTER TABLE BTRG_Barang DROP COLUMN Satuan3;
    END;
END;
