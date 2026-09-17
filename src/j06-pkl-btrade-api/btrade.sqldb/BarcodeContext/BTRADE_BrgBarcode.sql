CREATE TABLE [dbo].[BTRADE_BrgBarcode]
(
    BrgBarcodeId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgBarcodeId DEFAULT(''),
    BarcodeValue VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BarcodeValue DEFAULT(''),
    BrgId        VARCHAR(6)  NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgId        DEFAULT(''),
    BrgCode      VARCHAR(20) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgCode      DEFAULT(''),
    BrgName      VARCHAR(60) NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_BrgName      DEFAULT(''),
    Satuan       VARCHAR(7)  NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_Satuan       DEFAULT(''),
    ServerId     VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_BrgBarcode_ServerId     DEFAULT(''),

    CONSTRAINT PK_BTRADE_BrgBarcode PRIMARY KEY CLUSTERED (BrgBarcodeId, ServerId),
    CONSTRAINT UX_BTRADE_BrgBarcode_ServerId_BarcodeValue UNIQUE (ServerId, BarcodeValue)
)
GO

CREATE INDEX IX_BTRADE_BrgBarcode_ServerId_BrgId
    ON [dbo].[BTRADE_BrgBarcode] (ServerId, BrgId)
GO
