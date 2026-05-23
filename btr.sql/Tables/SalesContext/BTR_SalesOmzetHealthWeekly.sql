CREATE TABLE BTR_SalesOmzetHealthWeekly
(
    HealthWeeklyId         VARCHAR(13)  NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_HealthWeeklyId DEFAULT(''),
    YearNumber             INT          NOT NULL,
    WeekNumber             INT          NOT NULL,
    PeriodStartDate        DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_PeriodStartDate DEFAULT('3000-01-01'),
    PeriodEndDate          DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_PeriodEndDate DEFAULT('3000-01-01'),
    HealthLevel            VARCHAR(10)  NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_HealthLevel DEFAULT(''),
    HealthScore            INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_HealthScore DEFAULT(0),
    MissingOrdersCount     INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_MissingOrdersCount DEFAULT(0),
    MissingFaktursCount    INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_MissingFaktursCount DEFAULT(0),
    UnlinkedFaktursCount   INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_UnlinkedFaktursCount DEFAULT(0),
    StaleDataCount         INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_StaleDataCount DEFAULT(0),
    LastCalculatedAt       DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_LastCalculatedAt DEFAULT('3000-01-01'),
    CalculationDurationMs  INT          NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_CalculationDurationMs DEFAULT(0),
    CreatedAt              DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_CreatedAt DEFAULT('3000-01-01'),
    UpdatedAt              DATETIME     NOT NULL CONSTRAINT DF_BTR_SalesOmzetHealthWeekly_UpdatedAt DEFAULT('3000-01-01'),

    CONSTRAINT PK_BTR_SalesOmzetHealthWeekly PRIMARY KEY CLUSTERED (HealthWeeklyId),
    CONSTRAINT UX_BTR_SalesOmzetHealthWeekly_YearWeek UNIQUE (YearNumber, WeekNumber)
)
GO

CREATE INDEX IX_BTR_SalesOmzetHealthWeekly_YearWeek
    ON BTR_SalesOmzetHealthWeekly (YearNumber, WeekNumber)
GO
