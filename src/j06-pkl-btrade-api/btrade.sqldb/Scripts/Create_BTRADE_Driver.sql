-- Return Order (S2.5): create BTRADE_Driver (Driver reference projection).
-- Idempotent: skips objects that already exist; safe to re-run.
-- Source of truth: btrade.sqldb/DriverContext/BTRADE_Driver.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRADE_Driver', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[BTRADE_Driver]
(
    DriverId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Driver_DriverId   DEFAULT(''),
    DriverName VARCHAR(20) NOT NULL CONSTRAINT DF_BTRADE_Driver_DriverName DEFAULT(''),
    IsAktif    BIT         NOT NULL CONSTRAINT DF_BTRADE_Driver_IsAktif    DEFAULT(1),
    ServerId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTRADE_Driver_ServerId   DEFAULT(''),

    CONSTRAINT PK_BTRADE_Driver PRIMARY KEY CLUSTERED (DriverId, ServerId)
)
END
GO
