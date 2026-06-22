CREATE TABLE BTRPD_CustomerPortfolioLifecycleDist

(

    CustomerPortfolioLifecycleDistId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_Id DEFAULT(''),

    SnapshotKey                      VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_SnapshotKey DEFAULT('CURRENT'),

    LifecycleStage                   VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_LifecycleStage DEFAULT(''),

    LifecycleLabel                   VARCHAR(60)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_LifecycleLabel DEFAULT(''),

    CustomerCount                    INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_CustomerCount DEFAULT(0),

    SortOrder                        INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioLifecycleDist_SortOrder DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioLifecycleDist PRIMARY KEY CLUSTERED (CustomerPortfolioLifecycleDistId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioLifecycleDist_SnapshotKey

    ON BTRPD_CustomerPortfolioLifecycleDist (SnapshotKey, SortOrder)

GO

