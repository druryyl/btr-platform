CREATE TABLE [dbo].[BTR_VisitPlan]
(
	VisitPlanId     VARCHAR(26) NOT NULL CONSTRAINT DF_BTR_VisitPlan_VisitPlanId DEFAULT(''),
	SalesPersonId   VARCHAR(5)  NOT NULL CONSTRAINT DF_BTR_VisitPlan_SalesPersonId DEFAULT(''),
	VisitDate       DATE        NOT NULL,
	CustomerId      VARCHAR(6)  NOT NULL CONSTRAINT DF_BTR_VisitPlan_CustomerId DEFAULT(''),
	NoUrut          INT         NOT NULL CONSTRAINT DF_BTR_VisitPlan_NoUrut DEFAULT(0),
	HariRuteId      VARCHAR(3)  NOT NULL CONSTRAINT DF_BTR_VisitPlan_HariRuteId DEFAULT(''),
	PlanSource      VARCHAR(10) NOT NULL CONSTRAINT DF_BTR_VisitPlan_PlanSource DEFAULT('Template'),
	MaterializedAt  DATETIME    NOT NULL,

	CONSTRAINT PK_BTR_VisitPlan PRIMARY KEY CLUSTERED (VisitPlanId),
	CONSTRAINT UX_BTR_VisitPlan UNIQUE (SalesPersonId, VisitDate, CustomerId)
)
GO

CREATE INDEX IX_BTR_VisitPlan_VisitDate
	ON [dbo].[BTR_VisitPlan] (VisitDate)
GO

CREATE INDEX IX_BTR_VisitPlan_SalesPersonDate
	ON [dbo].[BTR_VisitPlan] (SalesPersonId, VisitDate)
GO
