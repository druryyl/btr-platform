CREATE TABLE BTRPD_CustomerPortfolioTierDist

(

    CustomerPortfolioTierDistId  VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_Id DEFAULT(''),

    SnapshotKey                  VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_SnapshotKey DEFAULT('CURRENT'),

    PortfolioTier                VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_PortfolioTier DEFAULT(''),

    TierLabel                    VARCHAR(60)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_TierLabel DEFAULT(''),

    CustomerCount                INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_CustomerCount DEFAULT(0),

    SortOrder                    INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioTierDist_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioTierDist PRIMARY KEY CLUSTERED (CustomerPortfolioTierDistId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioTierDist_SnapshotKey

    ON BTRPD_CustomerPortfolioTierDist (SnapshotKey, SortOrder)

GO

