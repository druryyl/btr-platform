-- M14 Piutang Dashboard V2 — idempotent schema upgrade.
-- Adds KPI columns and new snapshot tables for customer aging + top-20 risk.
-- Next Piutang refresh populates new columns/tables; no data backfill required.
SET NOCOUNT ON;
GO

-- BTRPD_PiutangKpi — new columns
IF COL_LENGTH(N'dbo.BTRPD_PiutangKpi', N'OverduePiutang') IS NULL
    ALTER TABLE BTRPD_PiutangKpi ADD OverduePiutang DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_BTRPD_PiutangKpi_OverduePiutang DEFAULT(0);
GO

IF COL_LENGTH(N'dbo.BTRPD_PiutangKpi', N'AgingOver90Amount') IS NULL
    ALTER TABLE BTRPD_PiutangKpi ADD AgingOver90Amount DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_BTRPD_PiutangKpi_AgingOver90Amount DEFAULT(0);
GO

IF COL_LENGTH(N'dbo.BTRPD_PiutangKpi', N'AgingOver90Percent') IS NULL
    ALTER TABLE BTRPD_PiutangKpi ADD AgingOver90Percent DECIMAL(9,4) NULL;
GO

IF COL_LENGTH(N'dbo.BTRPD_PiutangKpi', N'Top10CustomerConcentrationPercent') IS NULL
    ALTER TABLE BTRPD_PiutangKpi ADD Top10CustomerConcentrationPercent DECIMAL(9,4) NULL;
GO

IF COL_LENGTH(N'dbo.BTRPD_PiutangKpi', N'Top20CustomerConcentrationPercent') IS NULL
    ALTER TABLE BTRPD_PiutangKpi ADD Top20CustomerConcentrationPercent DECIMAL(9,4) NULL;
GO

-- BTRPD_PiutangCustomerAging
IF OBJECT_ID(N'dbo.BTRPD_PiutangCustomerAging', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangCustomerAging
(
    PiutangCustomerAgingId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_PiutangCustomerAgingId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_SnapshotKey DEFAULT('CURRENT'),
    CustomerId             VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerId DEFAULT(''),
    CustomerCode           VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerCode DEFAULT(''),
    CustomerName           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CustomerName DEFAULT(''),
    CurrentAmount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_CurrentAmount DEFAULT(0),
    Aging30Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging30Amount DEFAULT(0),
    Aging60Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging60Amount DEFAULT(0),
    Aging90Amount          DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_Aging90Amount DEFAULT(0),
    AgingOver90Amount      DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_AgingOver90Amount DEFAULT(0),
    LastUpdate             DATETIME      NOT NULL CONSTRAINT DF_BTRPD_PiutangCustomerAging_LastUpdate DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTRPD_PiutangCustomerAging PRIMARY KEY CLUSTERED (PiutangCustomerAgingId),
    CONSTRAINT UX_BTRPD_PiutangCustomerAging_SnapshotKey_CustomerId UNIQUE (SnapshotKey, CustomerId)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BTRPD_PiutangCustomerAging_SnapshotKey' AND object_id = OBJECT_ID(N'dbo.BTRPD_PiutangCustomerAging'))
CREATE INDEX IX_BTRPD_PiutangCustomerAging_SnapshotKey
    ON BTRPD_PiutangCustomerAging (SnapshotKey)
GO

-- BTRPD_PiutangTopCustomerRisk
IF OBJECT_ID(N'dbo.BTRPD_PiutangTopCustomerRisk', N'U') IS NULL
BEGIN
CREATE TABLE BTRPD_PiutangTopCustomerRisk
(
    PiutangTopCustomerRiskId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_PiutangTopCustomerRiskId DEFAULT(''),
    SnapshotKey              VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_SnapshotKey DEFAULT('CURRENT'),
    Rank                     INT           NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Rank DEFAULT(0),
    CustomerId               VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerId DEFAULT(''),
    CustomerCode             VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerCode DEFAULT(''),
    CustomerName             VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CustomerName DEFAULT(''),
    TotalPiutang             DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_TotalPiutang DEFAULT(0),
    CurrentAmount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_CurrentAmount DEFAULT(0),
    Aging30Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging30Amount DEFAULT(0),
    Aging60Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging60Amount DEFAULT(0),
    Aging90Amount            DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_Aging90Amount DEFAULT(0),
    AgingOver90Amount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_PiutangTopCustomerRisk_AgingOver90Amount DEFAULT(0),

    CONSTRAINT PK_BTRPD_PiutangTopCustomerRisk PRIMARY KEY CLUSTERED (PiutangTopCustomerRiskId),
    CONSTRAINT UX_BTRPD_PiutangTopCustomerRisk_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
END
GO
