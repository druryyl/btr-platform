-- BGUD-GOOGLE-SIGNIN P1-S01: add BTR_User.Email (Google-email mapping) with
-- default constraint and filtered unique index (TD-14, ARCHITECTURE §8).
-- Idempotent: skips objects that already exist; safe to re-run.
-- Source of truth: btr.sql/Tables/Helper/BTR_User.sql
SET NOCOUNT ON;
GO

IF COL_LENGTH('BTR_User', 'Email') IS NULL
BEGIN
    ALTER TABLE BTR_User
        ADD Email VARCHAR(100) NOT NULL
            CONSTRAINT DF_BTR_User_Email DEFAULT('');
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_BTR_User_Email'
      AND object_id = OBJECT_ID(N'dbo.BTR_User'))
BEGIN
CREATE UNIQUE INDEX UX_BTR_User_Email
    ON BTR_User (Email)
    WHERE Email <> ''
END
GO
