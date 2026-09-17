-- create table for the Driver reference projection (Architecture §6.3, GAP-013)
CREATE TABLE BTRADE_Driver(
    DriverId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Driver_DriverId   DEFAULT(''),
    DriverName VARCHAR(20) NOT NULL CONSTRAINT DF_BTRADE_Driver_DriverName DEFAULT(''),
    IsAktif    BIT         NOT NULL CONSTRAINT DF_BTRADE_Driver_IsAktif    DEFAULT(1),
    ServerId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Driver_ServerId   DEFAULT(''),

    CONSTRAINT PK_BTRADE_Driver PRIMARY KEY CLUSTERED (DriverId, ServerId)
)
GO
