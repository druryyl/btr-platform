CREATE TABLE [dbo].[BTRADE_Customer]
(
    CustomerId VARCHAR(6) NOT NULL CONSTRAINT DF_BTRADE_Customer_CustomerId DEFAULT(''), 
    CustomerCode VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_Customer_CustomerCode DEFAULT(''),
    CustomerName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_Customer_CustomerName DEFAULT(''), 
    Alamat VARCHAR(60) NOT NULL CONSTRAINT DF_BTRADE_Customer_Alamat DEFAULT(''), 
    Wilayah VARCHAR(20) NOT NULL CONSTRAINT DF_BTRADE_Customer_Wilayah DEFAULT(''),

    Latitude FLOAT NOT NULL CONSTRAINT DF_BTRADE_Customer_Latitude DEFAULT(0),
    Longitude FLOAT NOT NULL CONSTRAINT DF_BTRADE_Customer_Longitude DEFAULT(0),
    Accuracy FLOAT NOT NULL CONSTRAINT DF_BTRADE_Customer_Accuracy DEFAULT(0),
    CoordinateTimeStamp BIGINT NOT NULL CONSTRAINT DF_BTRADE_Customer_CoordinateTimeStampe DEFAULT(0),
    CoordinateUser VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_Customer_CoordinateUser DEFAULT(''),
    IsUpdated BIT NOT NULL CONSTRAINT DF_BTRADE_Customer_IsUpdated DEFAULT(0),

    ServerId VARCHAR(5) NOT NULL CONSTRAINT DF_BTRADE_Customer_ServerId DEFAULT(''),

    CONSTRAINT PK_BTRADE_Customer PRIMARY KEY CLUSTERED (CustomerId, ServerId)
)
