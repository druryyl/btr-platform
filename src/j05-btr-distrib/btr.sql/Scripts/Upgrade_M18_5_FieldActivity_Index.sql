-- Upgrade_M18_5_FieldActivity_Index.sql (idempotent)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BTR_CheckIn_CheckInDate_UserEmail'
      AND object_id = OBJECT_ID(N'dbo.BTR_CheckIn'))
CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_CheckInDate_UserEmail
    ON BTR_CheckIn (CheckInDate, UserEmail)
    INCLUDE (CustomerId, CheckInTime, CheckInLatitude, CheckInLongitude,
             CustomerLatitude, CustomerLongitude, Accuracy);
GO
