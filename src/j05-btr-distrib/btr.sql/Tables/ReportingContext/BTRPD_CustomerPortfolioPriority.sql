CREATE TABLE BTRPD_CustomerPortfolioPriority

(

    CustomerPortfolioPriorityId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_Id DEFAULT(''),

    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SnapshotKey DEFAULT('CURRENT'),

    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SortOrder DEFAULT(0),

    PortfolioPriorityScore        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PortfolioPriorityScore DEFAULT(0),

    CustomerKey                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerKey DEFAULT(''),

    CustomerId                    VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerId DEFAULT(''),

    CustomerCode                  VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerCode DEFAULT(''),

    CustomerName                  VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerName DEFAULT(''),

    WilayahName                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_WilayahName DEFAULT(''),

    Klasifikasi                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_Klasifikasi DEFAULT(''),

    LifecycleStage                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_LifecycleStage DEFAULT(''),

    LifecycleLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_LifecycleLabel DEFAULT(''),

    PortfolioTier                 VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PortfolioTier DEFAULT(''),

    TierLabel                     VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_TierLabel DEFAULT(''),

    PrimaryActionKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PrimaryActionKey DEFAULT(''),

    PrimaryActionLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_PrimaryActionLabel DEFAULT(''),

    ActionOwner                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_ActionOwner DEFAULT(''),

    ActionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_ActionReasonText DEFAULT(''),

    TriggeredRuleIds              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_TriggeredRuleIds DEFAULT(''),

    MtdOmzet                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_MtdOmzet DEFAULT(0),

    OpenBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_OpenBalance DEFAULT(0),

    OverdueBalance                DECIMAL(18,2)  NULL,

    M29Category                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_M29Category DEFAULT(''),

    SalesPersonName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SalesPersonName DEFAULT(''),

    SalesmanAchievementPercent    DECIMAL(9,4)   NULL,

    SalesmanHighPiutangExposure   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_SalesmanHighPiutangExposure DEFAULT(0),

    IsAttention                   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_IsAttention DEFAULT(0),

    M30LinkRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_M30LinkRoute DEFAULT(''),

    CustomerReportRoute           VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_CustomerReportRoute DEFAULT(''),

    DrillDownRouteM17             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_DrillDownRouteM17 DEFAULT(''),

    DrillDownRouteM29             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioPriority_DrillDownRouteM29 DEFAULT(''),



    CONSTRAINT PK_BTRPD_CustomerPortfolioPriority PRIMARY KEY CLUSTERED (CustomerPortfolioPriorityId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioPriority_SnapshotKey_SortOrder

    ON BTRPD_CustomerPortfolioPriority (SnapshotKey, SortOrder)

GO

