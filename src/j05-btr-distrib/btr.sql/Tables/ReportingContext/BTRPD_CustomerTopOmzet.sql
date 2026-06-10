CREATE TABLE BTRPD_CustomerTopOmzet
(
    CustomerTopOmzetId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerTopOmzetId DEFAULT(''),
    SnapshotKey        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_SnapshotKey DEFAULT('CURRENT'),
    Rank               INT           NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_Rank DEFAULT(0),
    CustomerCode       VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerCode DEFAULT(''),
    CustomerName       VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_CustomerName DEFAULT(''),
    OmzetAmount        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CustomerTopOmzet_OmzetAmount DEFAULT(0),
    PercentOfTotal     DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CustomerTopOmzet PRIMARY KEY CLUSTERED (CustomerTopOmzetId),
    CONSTRAINT UX_BTRPD_CustomerTopOmzet_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
