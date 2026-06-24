CREATE TABLE BTRPD_CustomerPortfolioKpi

(

    SnapshotKey                 VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_SnapshotKey DEFAULT('CURRENT'),

    GeneratedAt                   DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_GeneratedAt DEFAULT('3000-01-01'),

    BusinessDate                  DATETIME       NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_BusinessDate DEFAULT('3000-01-01'),

    PortfolioHealthScore          DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_PortfolioHealthScore DEFAULT(0),

    PortfolioHealthyPercent       DECIMAL(9,4)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_PortfolioHealthyPercent DEFAULT(0),

    TotalCustomerCount            INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_TotalCustomerCount DEFAULT(0),

    AttentionCustomerCount        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_AttentionCustomerCount DEFAULT(0),

    StrategicCustomerCount        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_StrategicCustomerCount DEFAULT(0),

    StrategicAtRiskCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_StrategicAtRiskCount DEFAULT(0),

    CustomersAtRiskCount          INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_CustomersAtRiskCount DEFAULT(0),

    WorkingCapitalTiedAmount      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_WorkingCapitalTiedAmount DEFAULT(0),

    TotalMtdOmzet                 DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_TotalMtdOmzet DEFAULT(0),

    TotalOpenBalance              DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_TotalOpenBalance DEFAULT(0),

    NeverPurchasedCount           INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_NeverPurchasedCount DEFAULT(0),

    DormantCount                  INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_DormantCount DEFAULT(0),

    DecliningCount                INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_DecliningCount DEFAULT(0),

    ExecutiveSummaryText          VARCHAR(2000)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_ExecutiveSummaryText DEFAULT(''),

    ValueDisclaimerText           VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_ValueDisclaimerText DEFAULT(''),

    LastRefreshLogId              VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioKpi_LastRefreshLogId DEFAULT(''),



    CONSTRAINT PK_BTRPD_CustomerPortfolioKpi PRIMARY KEY CLUSTERED (SnapshotKey)

)

GO

