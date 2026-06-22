CREATE TABLE BTRPD_CustomerPortfolioWilayah

(

    CustomerPortfolioWilayahId VARCHAR(26)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_Id DEFAULT(''),

    SnapshotKey                VARCHAR(10)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_SnapshotKey DEFAULT('CURRENT'),

    SortOrder                  INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_SortOrder DEFAULT(0),

    WilayahName                VARCHAR(30)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_WilayahName DEFAULT(''),

    CustomerCount              INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_CustomerCount DEFAULT(0),

    AttentionCustomerCount     INT          NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioWilayah_AttentionCustomerCount DEFAULT(0),



    CONSTRAINT PK_BTRPD_CustomerPortfolioWilayah PRIMARY KEY CLUSTERED (CustomerPortfolioWilayahId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioWilayah_SnapshotKey

    ON BTRPD_CustomerPortfolioWilayah (SnapshotKey, SortOrder)

GO

