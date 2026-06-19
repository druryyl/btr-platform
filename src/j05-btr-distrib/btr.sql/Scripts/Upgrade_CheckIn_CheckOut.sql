-- Upgrade_CheckIn_CheckOut.sql (idempotent)
IF COL_LENGTH('dbo.BTR_CheckIn', 'CheckOutTime') IS NULL
    ALTER TABLE dbo.BTR_CheckIn ADD CheckOutTime VARCHAR(8) NOT NULL
        CONSTRAINT DF_BTR_CheckIn_CheckOutTime DEFAULT('') WITH VALUES;
GO

IF COL_LENGTH('dbo.BTR_CheckIn', 'CheckOutLatitude') IS NULL
    ALTER TABLE dbo.BTR_CheckIn ADD CheckOutLatitude FLOAT NOT NULL
        CONSTRAINT DF_BTR_CheckIn_CheckOutLatitude DEFAULT(0) WITH VALUES;
GO

IF COL_LENGTH('dbo.BTR_CheckIn', 'CheckOutLongitude') IS NULL
    ALTER TABLE dbo.BTR_CheckIn ADD CheckOutLongitude FLOAT NOT NULL
        CONSTRAINT DF_BTR_CheckIn_CheckOutLongitude DEFAULT(0) WITH VALUES;
GO

IF COL_LENGTH('dbo.BTR_CheckIn', 'CheckOutAccuracy') IS NULL
    ALTER TABLE dbo.BTR_CheckIn ADD CheckOutAccuracy FLOAT NOT NULL
        CONSTRAINT DF_BTR_CheckIn_CheckOutAccuracy DEFAULT(0) WITH VALUES;
GO

IF COL_LENGTH('dbo.BTR_CheckIn', 'CheckOutMode') IS NULL
    ALTER TABLE dbo.BTR_CheckIn ADD CheckOutMode VARCHAR(10) NOT NULL
        CONSTRAINT DF_BTR_CheckIn_CheckOutMode DEFAULT('') WITH VALUES;
GO
