CREATE TABLE BTRPD_CustomerPortfolioCustomer

(

    CustomerPortfolioCustomerId   VARCHAR(26)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_Id DEFAULT(''),

    SnapshotKey                   VARCHAR(10)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SnapshotKey DEFAULT('CURRENT'),

    SortOrder                     INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SortOrder DEFAULT(0),

    CustomerKey                   VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerKey DEFAULT(''),

    CustomerId                    VARCHAR(13)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerId DEFAULT(''),

    CustomerCode                  VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerCode DEFAULT(''),

    CustomerName                  VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerName DEFAULT(''),

    WilayahName                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_WilayahName DEFAULT(''),

    Klasifikasi                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_Klasifikasi DEFAULT(''),

    LifecycleStage                VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_LifecycleStage DEFAULT(''),

    LifecycleLabel                VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_LifecycleLabel DEFAULT(''),

    PortfolioTier                 VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PortfolioTier DEFAULT(''),

    TierLabel                     VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_TierLabel DEFAULT(''),

    PrimaryActionKey              VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PrimaryActionKey DEFAULT(''),

    PrimaryActionLabel            VARCHAR(60)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PrimaryActionLabel DEFAULT(''),

    ActionOwner                   VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_ActionOwner DEFAULT(''),

    ActionReasonText              VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_ActionReasonText DEFAULT(''),

    TriggeredRuleIds              VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_TriggeredRuleIds DEFAULT(''),

    MtdOmzet                      DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_MtdOmzet DEFAULT(0),

    OpenBalance                   DECIMAL(18,2)  NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_OpenBalance DEFAULT(0),

    OverdueBalance                DECIMAL(18,2)  NULL,

    FakturCount6Mo                INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_FakturCount6Mo DEFAULT(0),

    IsActiveMtd                   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_IsActiveMtd DEFAULT(0),

    LastPurchaseDate              DATETIME       NULL,

    FirstPurchaseDate             DATETIME       NULL,

    M29Category                   VARCHAR(20)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_M29Category DEFAULT(''),

    M29PrimarySignalKey           VARCHAR(30)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_M29PrimarySignalKey DEFAULT(''),

    SalesPersonName               VARCHAR(50)    NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SalesPersonName DEFAULT(''),

    SalesmanAchievementPercent    DECIMAL(9,4)   NULL,

    SalesmanHighPiutangExposure   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_SalesmanHighPiutangExposure DEFAULT(0),

    IsAttention                   BIT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_IsAttention DEFAULT(0),

    PortfolioPriorityScore        INT            NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_PortfolioPriorityScore DEFAULT(0),

    M30LinkRoute                  VARCHAR(200)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_M30LinkRoute DEFAULT(''),

    CustomerReportRoute           VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_CustomerReportRoute DEFAULT(''),

    DrillDownRouteM17             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_DrillDownRouteM17 DEFAULT(''),

    DrillDownRouteM29             VARCHAR(100)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_DrillDownRouteM29 DEFAULT(''),

    ValueDisclaimer               VARCHAR(500)   NOT NULL CONSTRAINT DF_BTRPD_CustomerPortfolioCustomer_ValueDisclaimer DEFAULT(''),



    CONSTRAINT PK_BTRPD_CustomerPortfolioCustomer PRIMARY KEY CLUSTERED (CustomerPortfolioCustomerId)

)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioCustomer_SnapshotKey_IsAttention

    ON BTRPD_CustomerPortfolioCustomer (SnapshotKey, IsAttention)

GO



CREATE INDEX IX_BTRPD_CustomerPortfolioCustomer_SnapshotKey_CustomerCode

    ON BTRPD_CustomerPortfolioCustomer (SnapshotKey, CustomerCode)

GO

