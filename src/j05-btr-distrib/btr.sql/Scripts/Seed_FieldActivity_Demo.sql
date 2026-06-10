/*
  Seed_FieldActivity_Demo.sql
  ---------------------------------------------------------------------------
  DEV / STAGING ONLY — NEVER RUN IN PRODUCTION.

  Purpose: Provide a salesman-day dataset for M18.5 Field Activity dashboard demos
  when operational check-in volume is sparse.

  Guard: Set @EnableFieldActivityDemoSeed = 1 before executing.
  Idempotent: Safe to re-run; uses FAD00x customer keys and MERGE patterns.
  ---------------------------------------------------------------------------
*/
SET NOCOUNT ON;

DECLARE @EnableFieldActivityDemoSeed BIT = 0;
IF @EnableFieldActivityDemoSeed = 0
BEGIN
    RAISERROR('Field Activity demo seed is disabled. Set @EnableFieldActivityDemoSeed = 1 to run (dev/staging only).', 16, 1);
    RETURN;
END;

DECLARE @DemoDate DATE = CAST(DATEADD(DAY, -1, GETDATE()) AS DATE);
DECLARE @DemoDateText VARCHAR(10) = CONVERT(VARCHAR(10), @DemoDate, 120);
DECLARE @SalesPersonId VARCHAR(5);
DECLARE @SalesPersonEmail VARCHAR(100);

SELECT TOP 1
    @SalesPersonId = SalesPersonId,
    @SalesPersonEmail = Email
FROM BTR_SalesPerson
WHERE ISNULL(Email, '') <> ''
ORDER BY SalesPersonCode;

IF @SalesPersonId IS NULL
BEGIN
    RAISERROR('No salesperson with Email found. Configure BTR_SalesPerson.Email before running demo seed.', 16, 1);
    RETURN;
END;

PRINT CONCAT('Field Activity demo seed for SalesPersonId=', @SalesPersonId, ' VisitDate=', @DemoDateText);

DECLARE @Customers TABLE (
    CustomerId VARCHAR(6),
    CustomerCode VARCHAR(10),
    CustomerName VARCHAR(50),
    Latitude FLOAT,
    Longitude FLOAT,
    NoUrut INT
);

INSERT INTO @Customers (CustomerId, CustomerCode, CustomerName, Latitude, Longitude, NoUrut)
VALUES
    ('FAD001', 'FA-C01', 'Demo Toko Menteng', -6.195100, 106.830500, 1),
    ('FAD002', 'FA-C02', 'Demo Toko Senayan', -6.227000, 106.799700, 2),
    ('FAD003', 'FA-C03', 'Demo Toko Kuningan', -6.224900, 106.825300, 3),
    ('FAD004', 'FA-C04', 'Demo Toko Kemang', -6.261500, 106.810600, 4),
    ('FAD005', 'FA-C05', 'Demo Toko Tebet', -6.230000, 106.851400, 5),
    ('FAD006', 'FA-C06', 'Demo Toko Cikini', -6.194700, 106.838900, 6),
    ('FAD007', 'FA-C07', 'Demo Toko Rawamangun', -6.187400, 106.874300, 7),
    ('FAD008', 'FA-C08', 'Demo Toko Pluit', -6.120000, 106.790000, 8),
    ('FAD009', 'FA-C09', 'Demo Toko Unplanned', -6.175100, 106.865400, 99),
    ('FAD010', 'FA-C10', 'Demo Toko No Coords', 0, 0, 10);

MERGE BTR_Customer AS tgt
USING (
    SELECT CustomerId, CustomerCode, CustomerName, Latitude, Longitude
    FROM @Customers
) AS src
ON tgt.CustomerId = src.CustomerId
WHEN MATCHED THEN
    UPDATE SET
        CustomerCode = src.CustomerCode,
        CustomerName = src.CustomerName,
        Latitude = src.Latitude,
        Longitude = src.Longitude
WHEN NOT MATCHED THEN
    INSERT (CustomerId, CustomerCode, CustomerName, Latitude, Longitude)
    VALUES (src.CustomerId, src.CustomerCode, src.CustomerName, src.Latitude, src.Longitude);

DELETE FROM BTR_VisitPlan
WHERE SalesPersonId = @SalesPersonId
  AND VisitDate = @DemoDate
  AND CustomerId LIKE 'FAD0%';

INSERT INTO BTR_VisitPlan (
    VisitPlanId, SalesPersonId, VisitDate, CustomerId, NoUrut, HariRuteId, PlanSource, MaterializedAt)
SELECT
    CONCAT('FAVP', RIGHT(CONCAT('00', c.NoUrut), 2)),
    @SalesPersonId,
    @DemoDate,
    c.CustomerId,
    c.NoUrut,
    '',
    'DemoSeed',
    GETDATE()
FROM @Customers c
WHERE c.NoUrut BETWEEN 1 AND 10;

DELETE FROM BTR_CheckIn
WHERE UserEmail = @SalesPersonEmail
  AND CheckInDate = @DemoDateText
  AND CustomerId LIKE 'FAD0%';

INSERT INTO BTR_CheckIn (
    CheckInId, CheckInDate, CheckInTime, UserEmail,
    CheckInLatitude, CheckInLongitude, Accuracy,
    CustomerId, CustomerCode, CustomerName, CustomerAddress,
    CustomerLatitude, CustomerLongitude, StatusSync)
SELECT
    CONCAT('FACHK', x.CheckInIdSuffix),
    @DemoDateText,
    x.CheckInTime,
    @SalesPersonEmail,
    x.CheckInLat,
    x.CheckInLng,
    x.Accuracy,
    c.CustomerId,
    c.CustomerCode,
    c.CustomerName,
    'Demo address',
    c.Latitude,
    c.Longitude,
    'Synced'
FROM (
    VALUES
        ('01', 'FAD001', '08:05:00', -6.195150, 106.830520, 12.0),
        ('02', 'FAD002', '08:45:00', -6.227600, 106.799900, 35.0),
        ('03', 'FAD003', '09:20:00', -6.226300, 106.826900, 55.0),
        ('05', 'FAD005', '10:10:00', -6.230050, 106.851450, 15.0),
        ('06', 'FAD006', '10:55:00', -6.194720, 106.838920, 18.0),
        ('07', 'FAD007', '11:40:00', -6.187420, 106.874320, 20.0),
        ('09', 'FAD009', '12:15:00', -6.175120, 106.865420, 22.0)
) AS x(CheckInIdSuffix, CustomerId, CheckInTime, CheckInLat, CheckInLng, Accuracy)
INNER JOIN @Customers c ON c.CustomerId = x.CustomerId;

IF OBJECT_ID('dbo.BTR_Order', 'U') IS NOT NULL
BEGIN
    DELETE FROM BTR_Order
    WHERE UserEmail = @SalesPersonEmail
      AND OrderDate = @DemoDateText
      AND CustomerId LIKE 'FAD0%';

    INSERT INTO BTR_Order (OrderId, OrderDate, UserEmail, CustomerId)
    VALUES
        ('FAORD01', @DemoDateText, @SalesPersonEmail, 'FAD001'),
        ('FAORD02', @DemoDateText, @SalesPersonEmail, 'FAD002'),
        ('FAORD05', @DemoDateText, @SalesPersonEmail, 'FAD005');
END
ELSE
BEGIN
    PRINT 'BTR_Order not found — effective-call demo orders skipped.';
END;

PRINT 'Field Activity demo seed completed.';
PRINT CONCAT('Load portal Field Activity for salesman ', @SalesPersonId, ' on ', @DemoDateText);
GO
