CREATE TABLE [dbo].[BTRADE_PackingOrderDepo]
(
	PackingOrderId VARCHAR(26) NOT NULL CONSTRAINT DF_BTRADE_PackingOrderDepo_PackingOrderId DEFAULT ('') ,
	DepoId VARCHAR(10) NOT NULL CONSTRAINT DF_BTRADE_PackingOrderDepo_DepoId DEFAULT (''),
	UpdateTimestamp DATETIME NOT NULL CONSTRAINT DF_BTRADE_PackingOrderDepo_UpdateTimestamp DEFAULT ('3000-01-01'),
	DownloadTimestamp DATETIME NOT NULL CONSTRAINT DF_BTRADE_PackingOrderDepo_DownloadTimestamp DEFAULT ('3000-01-01'),

	CONSTRAINT PK_BTRADE_PackingOrderDepo PRIMARY KEY CLUSTERED (PackingOrderId, DepoId)
)
