-- BGUD-GOOGLE-SIGNIN P1-S02: add BTRADE_User.Email (with IX_BTRADE_User_Email)
-- and BTRADE_ReturnOrder.SubmittedBy columns (ARCHITECTURE §8).
-- Idempotent: skips columns and index that already exist; safe to re-run.
-- Existing rows receive the column defaults (''); additive only, no other
-- schema change.
-- Source of truth: btrade.sqldb/BarcodeContext/BTRADE_User.sql,
--                  btrade.sqldb/ReturnOrderContext/BTRADE_ReturnOrder.sql
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.BTRADE_User', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.BTRADE_User', N'Email') IS NULL
BEGIN
    ALTER TABLE [dbo].[BTRADE_User]
        ADD Email VARCHAR(100) NOT NULL CONSTRAINT DF_BTRADE_User_Email DEFAULT('')
END
GO

IF OBJECT_ID(N'dbo.BTRADE_User', N'U') IS NOT NULL
   AND NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTRADE_User_Email'
      AND object_id = OBJECT_ID(N'dbo.BTRADE_User'))
BEGIN
    CREATE INDEX IX_BTRADE_User_Email
        ON [dbo].[BTRADE_User] (Email)
END
GO

IF OBJECT_ID(N'dbo.BTRADE_ReturnOrder', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.BTRADE_ReturnOrder', N'SubmittedBy') IS NULL
BEGIN
    ALTER TABLE [dbo].[BTRADE_ReturnOrder]
        ADD SubmittedBy VARCHAR(50) NOT NULL CONSTRAINT DF_BTRADE_ReturnOrder_SubmittedBy DEFAULT('')
END
GO
