CREATE TABLE BTRPD_SalesmanSegmentation
(
    SalesmanSegmentationId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SalesmanSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50)   NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SegmentLabel DEFAULT(''),
    SalesmanCount          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SalesmanCount DEFAULT(0),
    ActiveCount            INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_ActiveCount DEFAULT(0),
    InactiveCount          INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_InactiveCount DEFAULT(0),
    SortOrder              INT           NOT NULL CONSTRAINT DF_BTRPD_SalesmanSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_SalesmanSegmentation PRIMARY KEY CLUSTERED (SalesmanSegmentationId),
    CONSTRAINT UX_BTRPD_SalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
GO
