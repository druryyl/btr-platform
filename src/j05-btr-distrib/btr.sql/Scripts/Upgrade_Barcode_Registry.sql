-- Barcode Registry (S1.1): create BTR_BrgBarcode, its indexes, and its FK.
-- Idempotent: skips objects that already exist; safe to re-run.
-- Source of truth: btr.sql/Tables/BrgContext/BTR_BrgBarcode.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTR_BrgBarcode', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTR_BrgBarcode]
(
    BrgBarcodeId   VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BrgBarcodeId   DEFAULT(''),
    BarcodeValue   VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BarcodeValue   DEFAULT(''),
    BarcodeValueKey AS UPPER(LTRIM(RTRIM(BarcodeValue))) PERSISTED,
    BrgId          VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_BrgBarcode_BrgId          DEFAULT(''),
    Satuan         VARCHAR(7)  NOT NULL CONSTRAINT DF_BTR_BrgBarcode_Satuan         DEFAULT(''),
    IsAktif        BIT         NOT NULL CONSTRAINT DF_BTR_BrgBarcode_IsAktif        DEFAULT(0),

    CreatedBy      VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_CreatedBy      DEFAULT(''),
    CreatedDate    DATETIME    NOT NULL CONSTRAINT DF_BTR_BrgBarcode_CreatedDate    DEFAULT('3000-01-01'),
    ModifiedBy     VARCHAR(50) NOT NULL CONSTRAINT DF_BTR_BrgBarcode_ModifiedBy     DEFAULT(''),
    ModifiedDate   DATETIME    NOT NULL CONSTRAINT DF_BTR_BrgBarcode_ModifiedDate   DEFAULT('3000-01-01'),

    RowVer         ROWVERSION     NOT NULL,

    CONSTRAINT PK_BTR_BrgBarcode PRIMARY KEY CLUSTERED (BrgBarcodeId)
)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_BTR_BrgBarcode_BarcodeValueKey'
      AND object_id = OBJECT_ID(N'dbo.BTR_BrgBarcode'))
BEGIN
CREATE UNIQUE INDEX UX_BTR_BrgBarcode_BarcodeValueKey
    ON BTR_BrgBarcode(BarcodeValueKey)
    WITH(FILLFACTOR=75)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_BrgBarcode_BrgId'
      AND object_id = OBJECT_ID(N'dbo.BTR_BrgBarcode'))
BEGIN
CREATE INDEX IX_BTR_BrgBarcode_BrgId
    ON BTR_BrgBarcode(BrgId, BrgBarcodeId)
    WITH(FILLFACTOR=75)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_BrgBarcode_RowVer'
      AND object_id = OBJECT_ID(N'dbo.BTR_BrgBarcode'))
BEGIN
CREATE INDEX IX_BTR_BrgBarcode_RowVer
    ON BTR_BrgBarcode(RowVer)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_BTR_BrgBarcode_BTR_Brg'
      AND parent_object_id = OBJECT_ID(N'dbo.BTR_BrgBarcode'))
BEGIN
ALTER TABLE BTR_BrgBarcode
    ADD CONSTRAINT FK_BTR_BrgBarcode_BTR_Brg
    FOREIGN KEY (BrgId) REFERENCES BTR_Brg(BrgId)
END
GO
