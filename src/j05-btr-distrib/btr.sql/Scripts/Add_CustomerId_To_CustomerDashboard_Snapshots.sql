-- Add CustomerId to customer dashboard snapshot tables for Entity Analytics EntityId routing.
-- Idempotent: safe to re-run when column already exists.

IF COL_LENGTH('BTRPD_CustomerTopOmzet', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE BTRPD_CustomerTopOmzet
        ADD CustomerId VARCHAR(13) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerId DEFAULT('');
END
GO

IF COL_LENGTH('BTRPD_CustomerTopPiutang', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE BTRPD_CustomerTopPiutang
        ADD CustomerId VARCHAR(13) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerTopPiutang_CustomerId DEFAULT('');
END
GO

IF COL_LENGTH('BTRPD_CustomerAttention', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE BTRPD_CustomerAttention
        ADD CustomerId VARCHAR(13) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerAttention_CustomerId DEFAULT('');
END
GO

IF COL_LENGTH('BTRPD_CustomerPortfolioPriority', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE BTRPD_CustomerPortfolioPriority
        ADD CustomerId VARCHAR(13) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerId DEFAULT('');
END
GO

IF COL_LENGTH('BTRPD_CustomerPortfolioCustomer', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE BTRPD_CustomerPortfolioCustomer
        ADD CustomerId VARCHAR(13) NOT NULL
            CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerId DEFAULT('');
END
GO
