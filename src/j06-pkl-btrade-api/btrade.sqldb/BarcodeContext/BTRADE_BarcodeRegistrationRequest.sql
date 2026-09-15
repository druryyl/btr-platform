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
GO

CREATE INDEX IX_BTRADE_BRR_ServerId_Status
    ON [dbo].[BTRADE_BarcodeRegistrationRequest] (ServerId, Status)
GO
