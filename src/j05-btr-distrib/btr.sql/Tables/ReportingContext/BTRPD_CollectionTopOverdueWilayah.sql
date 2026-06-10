CREATE TABLE BTRPD_CollectionTopOverdueWilayah
(
    CollectionTopOverdueWilayahId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_CollectionTopOverdueWilayahId DEFAULT(''),
    SnapshotKey                   VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_SnapshotKey DEFAULT('CURRENT'),
    Rank                          INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_Rank DEFAULT(0),
    WilayahId                     VARCHAR(13)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_WilayahId DEFAULT(''),
    WilayahName                   VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_WilayahName DEFAULT(''),
    OverdueBalance                DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionTopOverdueWilayah_OverdueBalance DEFAULT(0),
    PercentOfTotal                DECIMAL(9,4)  NULL,

    CONSTRAINT PK_BTRPD_CollectionTopOverdueWilayah PRIMARY KEY CLUSTERED (CollectionTopOverdueWilayahId),
    CONSTRAINT UX_BTRPD_CollectionTopOverdueWilayah_SnapshotKey_Rank UNIQUE (SnapshotKey, Rank)
)
GO
