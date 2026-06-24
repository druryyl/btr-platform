CREATE TABLE BTRPD_CustomerPortfolioActionDist

(

    CustomerPortfolioActionDistId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_Id DEFAULT(''),

    SnapshotKey                   VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_SnapshotKey DEFAULT('CURRENT'),

    PrimaryActionKey              VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_PrimaryActionKey DEFAULT(''),

    PrimaryActionLabel            VARCHAR(60)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_PrimaryActionLabel DEFAULT(''),

    CustomerCount                 INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_CustomerCount DEFAULT(0),

    SortOrder                     INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioActionDist_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioActionDist PRIMARY KEY CLUSTERED (CustomerPortfolioActionDistId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioActionDist_SnapshotKey

    ON BTRPD_CustomerPortfolioActionDist (SnapshotKey, SortOrder)

GO

