-- M18.6 Field Activity batch query indexes (idempotent)
SET NOCOUNT ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTR_CheckIn_CheckInDate' AND object_id = OBJECT_ID(N'dbo.BTR_CheckIn'))
CREATE NONCLUSTERED INDEX IX_BTR_CheckIn_CheckInDate
    ON BTR_CheckIn (CheckInDate)
    INCLUDE (UserEmail, CustomerId, CheckInTime, CheckInLatitude, CheckInLongitude,
             CustomerLatitude, CustomerLongitude, Accuracy);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTR_Order_OrderDate_UserEmail' AND object_id = OBJECT_ID(N'dbo.BTR_Order'))
CREATE NONCLUSTERED INDEX IX_BTR_Order_OrderDate_UserEmail
    ON BTR_Order (OrderDate, UserEmail)
    INCLUDE (CustomerId, TotalAmount);
GO
