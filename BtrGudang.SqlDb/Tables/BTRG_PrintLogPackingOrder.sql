CREATE TABLE [dbo].[BTRG_PrintLogPackingOrder]
(
	PrintLogId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRG_PrintLogPackingOrder_PrintLogId DEFAULT(''),
	PackingOrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRG_PrintLogPackingOrder_PackingOrderId DEFAULT(''),
	FakturId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRG_PrintLogPackingOrder_FakturId DEFAULT(''),

	CONSTRAINT PK_BTRG_PrintLogPackingOrder PRIMARY KEY CLUSTERED (PrintLogId, PackingOrderId)
)
