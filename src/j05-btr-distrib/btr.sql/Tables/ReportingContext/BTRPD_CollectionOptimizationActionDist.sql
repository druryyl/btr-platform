CREATE TABLE BTRPD_CollectionOptimizationActionDist
(
    CollectionOptimizationActionDistId VARCHAR(26)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_Id DEFAULT(''),
    SnapshotKey                        VARCHAR(10)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_SnapshotKey DEFAULT('CURRENT'),
    ActionCategoryKey                  VARCHAR(30)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel                VARCHAR(60)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ActionCategoryLabel DEFAULT(''),
    CustomerCount                      INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_CustomerCount DEFAULT(0),
    ImpactTotal                        DECIMAL(18,2) NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_ImpactTotal DEFAULT(0),
    SortOrder                          INT           NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationActionDist_SortOrder DEFAULT(0),

    CONSTRAINT PK_BTRPD_CollectionOptimizationActionDist PRIMARY KEY CLUSTERED (CollectionOptimizationActionDistId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationActionDist_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationActionDist (SnapshotKey, SortOrder)
GO
