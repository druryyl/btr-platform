CREATE TABLE BTRPD_CollectionOptimizationPriority
(
    CollectionOptimizationPriorityId VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_Id DEFAULT(''),
    SnapshotKey                      VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SnapshotKey DEFAULT('CURRENT'),
    SortOrder                        INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SortOrder DEFAULT(0),
    CollectionPriorityScore          INT            NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CollectionPriorityScore DEFAULT(0),
    CustomerCode                     VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CustomerCode DEFAULT(''),
    CustomerName                     VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CustomerName DEFAULT(''),
    WilayahName                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_WilayahName DEFAULT(''),
    SalesPersonName                  VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SalesPersonName DEFAULT(''),
    Klasifikasi                      VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_Klasifikasi DEFAULT(''),
    ActionCategoryKey                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionCategoryKey DEFAULT(''),
    ActionCategoryLabel              VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionCategoryLabel DEFAULT(''),
    RecommendedActionKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_RecommendedActionKey DEFAULT(''),
    RecommendedActionLabel           VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_RecommendedActionLabel DEFAULT(''),
    ActionOwner                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionOwner DEFAULT(''),
    OpenBalance                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_OpenBalance DEFAULT(0),
    OverdueBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_OverdueBalance DEFAULT(0),
    DueWithin7Days                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_DueWithin7Days DEFAULT(0),
    CollectionImpactAmount           DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_CollectionImpactAmount DEFAULT(0),
    M29Category                      VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29Category DEFAULT(''),
    M29RecommendationKey             VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29RecommendationKey DEFAULT(''),
    M29PrimarySignalKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_M29PrimarySignalKey DEFAULT(''),
    MinDaysUntilDue                  INT            NULL,
    CreditUtilizationPercent         DECIMAL(9,4)   NULL,
    SelectionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_SelectionReasonText DEFAULT(''),
    PriorityReasonText               VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_PriorityReasonText DEFAULT(''),
    ActionReasonText                 VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ActionReasonText DEFAULT(''),
    TriggeredRuleIds                 VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_TriggeredRuleIds DEFAULT(''),
    ReportRoute                      VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_ReportRoute DEFAULT(''),
    DrillDownRoute                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CollectionOptimizationPriority_DrillDownRoute DEFAULT(''),

    CONSTRAINT PK_BTRPD_CollectionOptimizationPriority PRIMARY KEY CLUSTERED (CollectionOptimizationPriorityId)
)
GO

CREATE INDEX IX_BTRPD_CollectionOptimizationPriority_SnapshotKey_SortOrder
    ON BTRPD_CollectionOptimizationPriority (SnapshotKey, SortOrder)
GO
