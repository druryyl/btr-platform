CREATE TABLE BTRPD_CollectionOptimizationWorkload
(
    CollectionOptimizationWorkloadId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_SnapshotKey DEFAULT('CURRENT'),
    WorkloadType                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_WorkloadType DEFAULT(''),
    EntityKey                        VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_EntityKey DEFAULT(''),
    EntityLabel                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_EntityLabel DEFAULT(''),
    ActionCount                      INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ActionCount DEFAULT(0),
    ImmediateCount                   INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ImmediateCount DEFAULT(0),
    ImpactTotal                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_ImpactTotal DEFAULT(0),
    OverdueExposure                  DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_OverdueExposure DEFAULT(0),
    IsHotspot                        BIT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_IsHotspot DEFAULT(0),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationWorkload_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionOptimizationWorkload PRIMARY KEY CLUSTERED (CollectionOptimizationWorkloadId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationWorkload_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationWorkload (SnapshotKey, SortOrder)
GO
