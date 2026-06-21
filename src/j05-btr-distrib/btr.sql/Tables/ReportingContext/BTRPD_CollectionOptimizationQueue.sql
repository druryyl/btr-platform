CREATE TABLE BTRPD_CollectionOptimizationQueue
(
    CollectionOptimizationQueueId    VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SnapshotKey DEFAULT('CURRENT'),
    QueueKey                         VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_QueueKey DEFAULT(''),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SortOrder DEFAULT(0),
    CollectionPriorityScore          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CollectionPriorityScore DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_SalesPersonName DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionCategoryLabel DEFAULT(''),
    RecommendedActionKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_RecommendedActionKey DEFAULT(''),
    RecommendedActionLabel           VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_RecommendedActionLabel DEFAULT(''),
    ActionOwner                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ActionOwner DEFAULT(''),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_DueWithin7Days DEFAULT(0),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_CollectionImpactAmount DEFAULT(0),
    M29Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_M29Category DEFAULT(''),
    QueueReasonText                  VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_QueueReasonText DEFAULT(''),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationQueue_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationQueue PRIMARY KEY CLUSTERED (CollectionOptimizationQueueId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationQueue_SnapshotKey_QueueKey_SortOrder
    ON BTRPD_CollectionOptimizationQueue (SnapshotKey, QueueKey, SortOrder)
GO
