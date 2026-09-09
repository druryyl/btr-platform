-- PCM-037 CU01 last-invoicing Salesman recency label.
-- Stores the latest-Faktur Salesman name on CU01 rows only.
-- Does not add Assigned Salesman, Owner, a Customer–Principal master, or Principal mix.
SET NOCOUNT ON;
GO

IF COL_LENGTH(N'dbo.BTRPD_CustomerAttention', N'LastInvoicingSalesmanName') IS NULL
BEGIN
    ALTER TABLE dbo.BTRPD_CustomerAttention
        ADD LastInvoicingSalesmanName VARCHAR(50) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerAttention_LastInvoicingSalesmanName DEFAULT('');
END
GO

IF COL_LENGTH(N'dbo.BTRPD_CustomerTopOmzet', N'LastInvoicingSalesmanName') IS NULL
BEGIN
    ALTER TABLE dbo.BTRPD_CustomerTopOmzet
        ADD LastInvoicingSalesmanName VARCHAR(50) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerTopOmzet_LastInvoicingSalesmanName DEFAULT('');
END
GO

IF COL_LENGTH(N'dbo.BTRPD_CustomerTopPiutang', N'LastInvoicingSalesmanName') IS NULL
BEGIN
    ALTER TABLE dbo.BTRPD_CustomerTopPiutang
        ADD LastInvoicingSalesmanName VARCHAR(50) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerTopPiutang_LastInvoicingSalesmanName DEFAULT('');
END
GO
