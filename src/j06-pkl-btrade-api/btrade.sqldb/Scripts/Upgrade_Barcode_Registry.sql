-- Barcode Registry (S3.1): create BTRADE_BrgBarcode, BTRADE_BarcodeRegistrationRequest,
-- BTRADE_Location (with seed data), and BTRADE_User, their indexes and constraints.
-- Idempotent: skips objects that already exist; safe to re-run.
-- Source of truth: btrade.sqldb/BarcodeContext/*.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRADE_BrgBarcode', N'U') IS NULL
BEGIN
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
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTRADE_BrgBarcode_ServerId_BrgId'
      AND object_id = OBJECT_ID(N'dbo.BTRADE_BrgBarcode'))
BEGIN
CREATE INDEX IX_BTRADE_BrgBarcode_ServerId_BrgId
    ON [dbo].[BTRADE_BrgBarcode] (ServerId, BrgId)
END
GO

IF OBJECT_ID(N'dbo.BTRADE_BarcodeRegistrationRequest', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTRADE_BarcodeRegistrationRequest]
(
    BarcodeRegistrationId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRADE_BRR_Id DEFAULT(''),
    ClientRequestId       VARCHAR(36) NOT NULL CONSTRAINT DF_BTRADE_BRR_ClientRequestId DEFAULT(''),
    ServerId              VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_BRR_ServerId DEFAULT(''),
    BarcodeValue          VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_BRR_BarcodeValue DEFAULT(''),
    BrgId                 VARCHAR(6)  NOT NULL CONSTRAINT DF_BTRADE_BRR_BrgId DEFAULT(''),
    Satuan                VARCHAR(7)  NOT NULL CONSTRAINT DF_BTRADE_BRR_Satuan DEFAULT(''),
    RequestedBy           VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_BRR_RequestedBy DEFAULT(''),
    RequestedAt           DATETIME    NOT NULL,
    Status                VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_BRR_Status DEFAULT('PENDING'),
    ProcessedAt           DATETIME    NULL,
    ProcessedNote         VARCHAR(200) NOT NULL CONSTRAINT DF_BTRADE_BRR_ProcessedNote DEFAULT(''),

    CONSTRAINT PK_BTRADE_BarcodeRegistrationRequest PRIMARY KEY CLUSTERED (BarcodeRegistrationId),
    CONSTRAINT UX_BTRADE_BRR_ServerId_ClientRequestId UNIQUE (ServerId, ClientRequestId)
)
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTRADE_BRR_ServerId_Status'
      AND object_id = OBJECT_ID(N'dbo.BTRADE_BarcodeRegistrationRequest'))
BEGIN
CREATE INDEX IX_BTRADE_BRR_ServerId_Status
    ON [dbo].[BTRADE_BarcodeRegistrationRequest] (ServerId, Status)
END
GO

IF OBJECT_ID(N'dbo.BTRADE_Location', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTRADE_Location]
(
    LocationId   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_Location_LocationId   DEFAULT(''),
    LocationName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_Location_LocationName DEFAULT(''),
    ServerId     VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Location_ServerId     DEFAULT(''),

    CONSTRAINT PK_BTRADE_Location PRIMARY KEY CLUSTERED (LocationId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM BTRADE_Location WHERE LocationId = 'GAMPING')
    INSERT INTO BTRADE_Location (LocationId, LocationName, ServerId) VALUES ('GAMPING', 'Gudang Gamping', 'JOGJA');
GO

IF NOT EXISTS (SELECT 1 FROM BTRADE_Location WHERE LocationId = 'CONCAT')
    INSERT INTO BTRADE_Location (LocationId, LocationName, ServerId) VALUES ('CONCAT', 'Gudang Concat', 'JOGJA');
GO

IF NOT EXISTS (SELECT 1 FROM BTRADE_Location WHERE LocationId = 'MAGELANG')
    INSERT INTO BTRADE_Location (LocationId, LocationName, ServerId) VALUES ('MAGELANG', 'Gudang Magelang', 'MGL');
GO

IF OBJECT_ID(N'dbo.BTRADE_User', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTRADE_User]
(
    UserId   VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_User_UserId   DEFAULT(''),
    UserName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_User_UserName DEFAULT(''),
    Password VARCHAR(64) NOT NULL CONSTRAINT DF_BTRADE_User_Password DEFAULT(''),
    RoleId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_User_RoleId   DEFAULT(''),
    IsAktif  BIT         NOT NULL CONSTRAINT DF_BTRADE_User_IsAktif  DEFAULT(0),
    ServerId VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_User_ServerId DEFAULT(''),

    CONSTRAINT PK_BTRADE_User PRIMARY KEY CLUSTERED (UserId)
)
END
GO
