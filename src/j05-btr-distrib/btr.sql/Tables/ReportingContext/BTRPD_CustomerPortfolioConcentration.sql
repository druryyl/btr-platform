CREATE TABLE BTRPD_CustomerPortfolioConcentration

(

    CustomerPortfolioConcentrationId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_Id DEFAULT(''),

    SnapshotKey                      VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_SnapshotKey DEFAULT('CURRENT'),

    ConcentrationType                VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_ConcentrationType DEFAULT(''),

    SortOrder                        INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_SortOrder DEFAULT(0),

    Rank                             INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_Rank DEFAULT(0),

    CustomerCode                     VARCHAR(20)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_CustomerCode DEFAULT(''),

    CustomerName                     VARCHAR(100) NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_CustomerName DEFAULT(''),

    Amount                           DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioConcentration_Amount DEFAULT(0),

    PercentOfTotal                   DECIMAL(9,4) NULL,



    CONSTRAINT PK_BTRPD_CustomerPortfolioConcentration PRIMARY KEY CLUSTERED (CustomerPortfolioConcentrationId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioConcentration_SnapshotKey_Type

    ON BTRPD_CustomerPortfolioConcentration (SnapshotKey, ConcentrationType, SortOrder)

GO

