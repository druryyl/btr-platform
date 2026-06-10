CREATE TABLE [dbo].[BTR_VisitPlanException]
(
	VisitPlanExceptionId  VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_VisitPlanException_VisitPlanExceptionId DEFAULT(''),
	SalesPersonId         VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_SalesPersonId DEFAULT(''),
	VisitDate             DATE        NOT NULL,
	ExceptionType         VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_VisitPlanException_ExceptionType DEFAULT(''),
	CustomerId            VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_CustomerId DEFAULT(''),
	ReplacementCustomerId VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlanException_ReplacementCustomerId DEFAULT(''),
	CreatedAt             DATETIME    NOT NULL,
	CreatedByUserId       VARCHAR(20) NOT NULL CONSTRAINT DF_BTR_VisitPlanException_CreatedByUserId DEFAULT(''),

	CONSTRAINT PK_BTR_VisitPlanException PRIMARY KEY CLUSTERED (VisitPlanExceptionId)
)
GO

CREATE INDEX IX_BTR_VisitPlanException_Lookup
	ON [dbo].[BTR_VisitPlanException] (SalesPersonId, VisitDate)
GO
