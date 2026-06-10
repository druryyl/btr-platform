CREATE TABLE BTRPD_CustomerSegmentation
(
    CustomerSegmentationId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_CustomerSegmentationId DEFAULT(''),
    SnapshotKey            VARCHAR(10) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SnapshotKey DEFAULT('CURRENT'),
    SegmentType            VARCHAR(20) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentType DEFAULT(''),
    SegmentKey             VARCHAR(30) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentKey DEFAULT(''),
    SegmentLabel           VARCHAR(50) NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SegmentLabel DEFAULT(''),
    CustomerCount          INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_CustomerCount DEFAULT(0),
    ActiveCount            INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_ActiveCount DEFAULT(0),
    DormantCount           INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_DormantCount DEFAULT(0),
    SortOrder              INT         NOT NULL CONSTRAINT DF_BTRPD_CustomerSegmentation_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CustomerSegmentation PRIMARY KEY CLUSTERED (CustomerSegmentationId),
    CONSTRAINT UX_BTRPD_CustomerSegmentation_SnapshotKey_SegmentType_SegmentKey UNIQUE (SnapshotKey, SegmentType, SegmentKey)
)
GO
