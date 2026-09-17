CREATE TABLE BTR_BrgBarcode(
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
GO

CREATE UNIQUE INDEX UX_BTR_BrgBarcode_BarcodeValueKey
    ON BTR_BrgBarcode(BarcodeValueKey)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_BrgBarcode_BrgId
    ON BTR_BrgBarcode(BrgId, BrgBarcodeId)
    WITH(FILLFACTOR=75)
GO

CREATE INDEX IX_BTR_BrgBarcode_RowVer
    ON BTR_BrgBarcode(RowVer)
GO

ALTER TABLE BTR_BrgBarcode
    ADD CONSTRAINT FK_BTR_BrgBarcode_BTR_Brg
    FOREIGN KEY (BrgId) REFERENCES BTR_Brg(BrgId)
GO
