CREATE TABLE [dbo].[BTRADE_Location]
(
    LocationId   VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_Location_LocationId   DEFAULT(''),
    LocationName VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_Location_LocationName DEFAULT(''),
    ServerId     VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Location_ServerId     DEFAULT(''),

    CONSTRAINT PK_BTRADE_Location PRIMARY KEY CLUSTERED (LocationId)
)
