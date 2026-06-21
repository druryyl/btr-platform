/*
 * BTR Portal Dashboard materialized tables (BTRPD_*)
 * Generated: 2026-06-20 09:53:27
 * Source: localhost / btr
 * Tables: 50
 */

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

------------------------------------------------------------------------
-- Table: dbo.BTRPD_CollectionAging
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CollectionAging](
	[CollectionAgingId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BucketKey] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BucketLabel] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_CollectionAging] PRIMARY KEY CLUSTERED 
(
	[CollectionAgingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CollectionAging_SnapshotKey_BucketKey] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[BucketKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CollectionAging] ADD  CONSTRAINT [DF_BTRPD_CollectionAging_CollectionAgingId]  DEFAULT ('') FOR [CollectionAgingId]

ALTER TABLE [dbo].[BTRPD_CollectionAging] ADD  CONSTRAINT [DF_BTRPD_CollectionAging_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CollectionAging] ADD  CONSTRAINT [DF_BTRPD_CollectionAging_BucketKey]  DEFAULT ('') FOR [BucketKey]

ALTER TABLE [dbo].[BTRPD_CollectionAging] ADD  CONSTRAINT [DF_BTRPD_CollectionAging_BucketLabel]  DEFAULT ('') FOR [BucketLabel]

ALTER TABLE [dbo].[BTRPD_CollectionAging] ADD  CONSTRAINT [DF_BTRPD_CollectionAging_Amount]  DEFAULT ((0)) FOR [Amount]

ALTER TABLE [dbo].[BTRPD_CollectionAging] ADD  CONSTRAINT [DF_BTRPD_CollectionAging_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CollectionAttention
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CollectionAttention](
	[CollectionAttentionId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityType] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalKey] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ValueAmount] [decimal](18, 2) NULL,
	[ValueText] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WilayahName] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_CollectionAttention] PRIMARY KEY CLUSTERED 
(
	[CollectionAttentionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_CollectionAttention_SnapshotKey_SortOrder] ON [dbo].[BTRPD_CollectionAttention]
(
	[SnapshotKey] ASC,
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_CollectionAttentionId]  DEFAULT ('') FOR [CollectionAttentionId]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_EntityType]  DEFAULT ('') FOR [EntityType]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_EntityId]  DEFAULT ('') FOR [EntityId]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_EntityCode]  DEFAULT ('') FOR [EntityCode]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_EntityName]  DEFAULT ('') FOR [EntityName]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_SignalKey]  DEFAULT ('') FOR [SignalKey]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_SignalLabel]  DEFAULT ('') FOR [SignalLabel]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_WilayahName]  DEFAULT ('') FOR [WilayahName]

ALTER TABLE [dbo].[BTRPD_CollectionAttention] ADD  CONSTRAINT [DF_BTRPD_CollectionAttention_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CollectionKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CollectionKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[OverdueExposure] [decimal](18, 2) NOT NULL,
	[AgingOver90Exposure] [decimal](18, 2) NOT NULL,
	[OverdueConcentrationPercent] [decimal](9, 4) NULL,
	[CashCollectedMtd] [decimal](18, 2) NOT NULL,
	[MonthCollections] [decimal](18, 2) NOT NULL,
	[MonthFakturOmzet] [decimal](18, 2) NOT NULL,
	[RecoveryVsBillingPercent] [decimal](9, 4) NULL,
	[PaymentMixCashAmount] [decimal](18, 2) NOT NULL,
	[PaymentMixGiroAmount] [decimal](18, 2) NOT NULL,
	[PaymentMixAdjustmentAmount] [decimal](18, 2) NOT NULL,
	[PaymentMixCashPercent] [decimal](9, 4) NULL,
	[PaymentMixGiroPercent] [decimal](9, 4) NULL,
	[PaymentMixAdjustmentPercent] [decimal](9, 4) NULL,
	[LegacyDebtCount] [int] NOT NULL,
	[ChronicOverdueCount] [int] NOT NULL,
	[WilayahHotspotCount] [int] NOT NULL,
	[LowRecoveryVsBillingCount] [int] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_CollectionKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_OverdueExposure]  DEFAULT ((0)) FOR [OverdueExposure]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_AgingOver90Exposure]  DEFAULT ((0)) FOR [AgingOver90Exposure]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_CashCollectedMtd]  DEFAULT ((0)) FOR [CashCollectedMtd]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_MonthCollections]  DEFAULT ((0)) FOR [MonthCollections]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_MonthFakturOmzet]  DEFAULT ((0)) FOR [MonthFakturOmzet]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_PaymentMixCashAmount]  DEFAULT ((0)) FOR [PaymentMixCashAmount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_PaymentMixGiroAmount]  DEFAULT ((0)) FOR [PaymentMixGiroAmount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_PaymentMixAdjustmentAmount]  DEFAULT ((0)) FOR [PaymentMixAdjustmentAmount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_LegacyDebtCount]  DEFAULT ((0)) FOR [LegacyDebtCount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_ChronicOverdueCount]  DEFAULT ((0)) FOR [ChronicOverdueCount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_WilayahHotspotCount]  DEFAULT ((0)) FOR [WilayahHotspotCount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_LowRecoveryVsBillingCount]  DEFAULT ((0)) FOR [LowRecoveryVsBillingCount]

ALTER TABLE [dbo].[BTRPD_CollectionKpi] ADD  CONSTRAINT [DF_BTRPD_CollectionKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CollectionTopOverdueCustomer
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer](
	[CollectionTopOverdueCustomerId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[CustomerCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OverdueBalance] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_CollectionTopOverdueCustomer] PRIMARY KEY CLUSTERED 
(
	[CollectionTopOverdueCustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CollectionTopOverdueCustomer_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueCustomer_CollectionTopOverdueCustomerId]  DEFAULT ('') FOR [CollectionTopOverdueCustomerId]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueCustomer_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueCustomer_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueCustomer_CustomerCode]  DEFAULT ('') FOR [CustomerCode]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueCustomer_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueCustomer] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueCustomer_OverdueBalance]  DEFAULT ((0)) FOR [OverdueBalance]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CollectionTopOverdueSalesman
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman](
	[CollectionTopOverdueSalesmanId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OverdueBalance] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_CollectionTopOverdueSalesman] PRIMARY KEY CLUSTERED 
(
	[CollectionTopOverdueSalesmanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CollectionTopOverdueSalesman_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_CollectionTopOverdueSalesmanId]  DEFAULT ('') FOR [CollectionTopOverdueSalesmanId]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueSalesman] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueSalesman_OverdueBalance]  DEFAULT ((0)) FOR [OverdueBalance]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CollectionTopOverdueWilayah
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah](
	[CollectionTopOverdueWilayahId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[WilayahId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WilayahName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OverdueBalance] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_CollectionTopOverdueWilayah] PRIMARY KEY CLUSTERED 
(
	[CollectionTopOverdueWilayahId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CollectionTopOverdueWilayah_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueWilayah_CollectionTopOverdueWilayahId]  DEFAULT ('') FOR [CollectionTopOverdueWilayahId]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueWilayah_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueWilayah_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueWilayah_WilayahId]  DEFAULT ('') FOR [WilayahId]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueWilayah_WilayahName]  DEFAULT ('') FOR [WilayahName]

ALTER TABLE [dbo].[BTRPD_CollectionTopOverdueWilayah] ADD  CONSTRAINT [DF_BTRPD_CollectionTopOverdueWilayah_OverdueBalance]  DEFAULT ((0)) FOR [OverdueBalance]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CustomerAttention
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CustomerAttention](
	[CustomerAttentionId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalKey] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ValueAmount] [decimal](18, 2) NULL,
	[ValueText] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WilayahName] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_CustomerAttention] PRIMARY KEY CLUSTERED 
(
	[CustomerAttentionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_CustomerAttention_SnapshotKey_SortOrder] ON [dbo].[BTRPD_CustomerAttention]
(
	[SnapshotKey] ASC,
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_CustomerAttentionId]  DEFAULT ('') FOR [CustomerAttentionId]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_CustomerCode]  DEFAULT ('') FOR [CustomerCode]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_SignalKey]  DEFAULT ('') FOR [SignalKey]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_SignalLabel]  DEFAULT ('') FOR [SignalLabel]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_WilayahName]  DEFAULT ('') FOR [WilayahName]

ALTER TABLE [dbo].[BTRPD_CustomerAttention] ADD  CONSTRAINT [DF_BTRPD_CustomerAttention_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CustomerKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CustomerKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[TotalOmzet] [decimal](18, 2) NOT NULL,
	[TotalPiutang] [decimal](18, 2) NOT NULL,
	[ActiveCustomerCount] [int] NOT NULL,
	[DormantCustomerCount] [int] NOT NULL,
	[OverdueCustomerCount] [int] NOT NULL,
	[PlafondBreachCount] [int] NOT NULL,
	[SuspendedWithSalesCount] [int] NOT NULL,
	[AgingOver90Amount] [decimal](18, 2) NOT NULL,
	[TopOmzetCustomerPercent] [decimal](9, 4) NULL,
	[TopPiutangCustomerPercent] [decimal](9, 4) NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_CustomerKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_TotalOmzet]  DEFAULT ((0)) FOR [TotalOmzet]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_TotalPiutang]  DEFAULT ((0)) FOR [TotalPiutang]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_ActiveCustomerCount]  DEFAULT ((0)) FOR [ActiveCustomerCount]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_DormantCustomerCount]  DEFAULT ((0)) FOR [DormantCustomerCount]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_OverdueCustomerCount]  DEFAULT ((0)) FOR [OverdueCustomerCount]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_PlafondBreachCount]  DEFAULT ((0)) FOR [PlafondBreachCount]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_SuspendedWithSalesCount]  DEFAULT ((0)) FOR [SuspendedWithSalesCount]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_AgingOver90Amount]  DEFAULT ((0)) FOR [AgingOver90Amount]

ALTER TABLE [dbo].[BTRPD_CustomerKpi] ADD  CONSTRAINT [DF_BTRPD_CustomerKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CustomerSegmentation
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CustomerSegmentation](
	[CustomerSegmentationId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SegmentType] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SegmentKey] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SegmentLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerCount] [int] NOT NULL,
	[ActiveCount] [int] NOT NULL,
	[DormantCount] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_CustomerSegmentation] PRIMARY KEY CLUSTERED 
(
	[CustomerSegmentationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CustomerSegmentation_SnapshotKey_SegmentType_SegmentKey] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[SegmentType] ASC,
	[SegmentKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_CustomerSegmentationId]  DEFAULT ('') FOR [CustomerSegmentationId]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_SegmentType]  DEFAULT ('') FOR [SegmentType]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_SegmentKey]  DEFAULT ('') FOR [SegmentKey]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_SegmentLabel]  DEFAULT ('') FOR [SegmentLabel]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_CustomerCount]  DEFAULT ((0)) FOR [CustomerCount]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_ActiveCount]  DEFAULT ((0)) FOR [ActiveCount]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_DormantCount]  DEFAULT ((0)) FOR [DormantCount]

ALTER TABLE [dbo].[BTRPD_CustomerSegmentation] ADD  CONSTRAINT [DF_BTRPD_CustomerSegmentation_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CustomerTopOmzet
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CustomerTopOmzet](
	[CustomerTopOmzetId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[CustomerCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OmzetAmount] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_CustomerTopOmzet] PRIMARY KEY CLUSTERED 
(
	[CustomerTopOmzetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CustomerTopOmzet_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CustomerTopOmzet] ADD  CONSTRAINT [DF_BTRPD_CustomerTopOmzet_CustomerTopOmzetId]  DEFAULT ('') FOR [CustomerTopOmzetId]

ALTER TABLE [dbo].[BTRPD_CustomerTopOmzet] ADD  CONSTRAINT [DF_BTRPD_CustomerTopOmzet_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CustomerTopOmzet] ADD  CONSTRAINT [DF_BTRPD_CustomerTopOmzet_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_CustomerTopOmzet] ADD  CONSTRAINT [DF_BTRPD_CustomerTopOmzet_CustomerCode]  DEFAULT ('') FOR [CustomerCode]

ALTER TABLE [dbo].[BTRPD_CustomerTopOmzet] ADD  CONSTRAINT [DF_BTRPD_CustomerTopOmzet_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_CustomerTopOmzet] ADD  CONSTRAINT [DF_BTRPD_CustomerTopOmzet_OmzetAmount]  DEFAULT ((0)) FOR [OmzetAmount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_CustomerTopPiutang
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_CustomerTopPiutang](
	[CustomerTopPiutangId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[CustomerCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OutstandingBalance] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_CustomerTopPiutang] PRIMARY KEY CLUSTERED 
(
	[CustomerTopPiutangId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_CustomerTopPiutang_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_CustomerTopPiutang] ADD  CONSTRAINT [DF_BTRPD_CustomerTopPiutang_CustomerTopPiutangId]  DEFAULT ('') FOR [CustomerTopPiutangId]

ALTER TABLE [dbo].[BTRPD_CustomerTopPiutang] ADD  CONSTRAINT [DF_BTRPD_CustomerTopPiutang_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_CustomerTopPiutang] ADD  CONSTRAINT [DF_BTRPD_CustomerTopPiutang_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_CustomerTopPiutang] ADD  CONSTRAINT [DF_BTRPD_CustomerTopPiutang_CustomerCode]  DEFAULT ('') FOR [CustomerCode]

ALTER TABLE [dbo].[BTRPD_CustomerTopPiutang] ADD  CONSTRAINT [DF_BTRPD_CustomerTopPiutang_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_CustomerTopPiutang] ADD  CONSTRAINT [DF_BTRPD_CustomerTopPiutang_OutstandingBalance]  DEFAULT ((0)) FOR [OutstandingBalance]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryBreakdown
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryBreakdown](
	[InventoryBreakdownId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[DimensionType] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Name] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[InventoryValue] [decimal](18, 2) NOT NULL,
	[IsTop10] [bit] NOT NULL,
	[Top10Rank] [int] NULL,
 CONSTRAINT [PK_BTRPD_InventoryBreakdown] PRIMARY KEY CLUSTERED 
(
	[InventoryBreakdownId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_InventoryBreakdown_SnapshotKey_DimensionType] ON [dbo].[BTRPD_InventoryBreakdown]
(
	[SnapshotKey] ASC,
	[DimensionType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_InventoryBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryBreakdown_InventoryBreakdownId]  DEFAULT ('') FOR [InventoryBreakdownId]

ALTER TABLE [dbo].[BTRPD_InventoryBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryBreakdown_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryBreakdown_DimensionType]  DEFAULT ('') FOR [DimensionType]

ALTER TABLE [dbo].[BTRPD_InventoryBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryBreakdown_Name]  DEFAULT ('') FOR [Name]

ALTER TABLE [dbo].[BTRPD_InventoryBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryBreakdown_InventoryValue]  DEFAULT ((0)) FOR [InventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryBreakdown_IsTop10]  DEFAULT ((0)) FOR [IsTop10]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[TotalInventoryValue] [decimal](18, 2) NOT NULL,
	[TotalItem] [int] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_InventoryKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_InventoryKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_InventoryKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryKpi_TotalInventoryValue]  DEFAULT ((0)) FOR [TotalInventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryKpi_TotalItem]  DEFAULT ((0)) FOR [TotalItem]

ALTER TABLE [dbo].[BTRPD_InventoryKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryRiskAging
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryRiskAging](
	[InventoryRiskAgingId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BucketKey] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BucketLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[InventoryValue] [decimal](18, 2) NOT NULL,
	[ItemCount] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_InventoryRiskAging] PRIMARY KEY CLUSTERED 
(
	[InventoryRiskAgingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE UNIQUE NONCLUSTERED INDEX [UX_BTRPD_InventoryRiskAging_SnapshotKey_BucketKey] ON [dbo].[BTRPD_InventoryRiskAging]
(
	[SnapshotKey] ASC,
	[BucketKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_InventoryRiskAgingId]  DEFAULT ('') FOR [InventoryRiskAgingId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_BucketKey]  DEFAULT ('') FOR [BucketKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_BucketLabel]  DEFAULT ('') FOR [BucketLabel]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_InventoryValue]  DEFAULT ((0)) FOR [InventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_ItemCount]  DEFAULT ((0)) FOR [ItemCount]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAging] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAging_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryRiskAttention
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryRiskAttention](
	[InventoryRiskAttentionId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgName] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[KategoriName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SupplierName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Qty] [int] NOT NULL,
	[InventoryValue] [decimal](18, 2) NOT NULL,
	[DaysSinceLastFaktur] [int] NULL,
	[SignalKey] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_InventoryRiskAttention] PRIMARY KEY CLUSTERED 
(
	[InventoryRiskAttentionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_InventoryRiskAttention_SnapshotKey_SortOrder] ON [dbo].[BTRPD_InventoryRiskAttention]
(
	[SnapshotKey] ASC,
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_InventoryRiskAttentionId]  DEFAULT ('') FOR [InventoryRiskAttentionId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_BrgId]  DEFAULT ('') FOR [BrgId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_BrgCode]  DEFAULT ('') FOR [BrgCode]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_BrgName]  DEFAULT ('') FOR [BrgName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_KategoriName]  DEFAULT ('') FOR [KategoriName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_SupplierName]  DEFAULT ('') FOR [SupplierName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_Qty]  DEFAULT ((0)) FOR [Qty]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_InventoryValue]  DEFAULT ((0)) FOR [InventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_SignalKey]  DEFAULT ('') FOR [SignalKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_SignalLabel]  DEFAULT ('') FOR [SignalLabel]

ALTER TABLE [dbo].[BTRPD_InventoryRiskAttention] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskAttention_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryRiskBreakdown
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryRiskBreakdown](
	[InventoryRiskBreakdownId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[DimensionType] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Name] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[AtRiskValue] [decimal](18, 2) NOT NULL,
	[ItemCount] [int] NOT NULL,
	[Rank] [int] NOT NULL,
	[PercentOfAtRisk] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_InventoryRiskBreakdown] PRIMARY KEY CLUSTERED 
(
	[InventoryRiskBreakdownId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE UNIQUE NONCLUSTERED INDEX [UX_BTRPD_InventoryRiskBreakdown_SnapshotKey_DimensionType_Rank] ON [dbo].[BTRPD_InventoryRiskBreakdown]
(
	[SnapshotKey] ASC,
	[DimensionType] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_InventoryRiskBreakdownId]  DEFAULT ('') FOR [InventoryRiskBreakdownId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_DimensionType]  DEFAULT ('') FOR [DimensionType]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_Name]  DEFAULT ('') FOR [Name]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_AtRiskValue]  DEFAULT ((0)) FOR [AtRiskValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_ItemCount]  DEFAULT ((0)) FOR [ItemCount]

ALTER TABLE [dbo].[BTRPD_InventoryRiskBreakdown] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskBreakdown_Rank]  DEFAULT ((0)) FOR [Rank]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryRiskKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryRiskKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[TotalInventoryValue] [decimal](18, 2) NOT NULL,
	[TotalItem] [int] NOT NULL,
	[DeadStockItemCount] [int] NOT NULL,
	[DeadStockValue] [decimal](18, 2) NOT NULL,
	[SlowMovingItemCount] [int] NOT NULL,
	[SlowMovingValue] [decimal](18, 2) NOT NULL,
	[NeverSoldItemCount] [int] NOT NULL,
	[NeverSoldValue] [decimal](18, 2) NOT NULL,
	[AtRiskInventoryValue] [decimal](18, 2) NOT NULL,
	[AtRiskInventoryPercent] [decimal](9, 4) NULL,
	[RequiresAttention] [bit] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_InventoryRiskKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_TotalInventoryValue]  DEFAULT ((0)) FOR [TotalInventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_TotalItem]  DEFAULT ((0)) FOR [TotalItem]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_DeadStockItemCount]  DEFAULT ((0)) FOR [DeadStockItemCount]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_DeadStockValue]  DEFAULT ((0)) FOR [DeadStockValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_SlowMovingItemCount]  DEFAULT ((0)) FOR [SlowMovingItemCount]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_SlowMovingValue]  DEFAULT ((0)) FOR [SlowMovingValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_NeverSoldItemCount]  DEFAULT ((0)) FOR [NeverSoldItemCount]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_NeverSoldValue]  DEFAULT ((0)) FOR [NeverSoldValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_AtRiskInventoryValue]  DEFAULT ((0)) FOR [AtRiskInventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_RequiresAttention]  DEFAULT ((0)) FOR [RequiresAttention]

ALTER TABLE [dbo].[BTRPD_InventoryRiskKpi] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryRiskTopDead
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryRiskTopDead](
	[InventoryRiskTopDeadId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[BrgId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgName] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[KategoriName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SupplierName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Qty] [int] NOT NULL,
	[InventoryValue] [decimal](18, 2) NOT NULL,
	[DaysSinceLastFaktur] [int] NOT NULL,
	[PercentOfAtRisk] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_InventoryRiskTopDead] PRIMARY KEY CLUSTERED 
(
	[InventoryRiskTopDeadId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE UNIQUE NONCLUSTERED INDEX [UX_BTRPD_InventoryRiskTopDead_SnapshotKey_Rank] ON [dbo].[BTRPD_InventoryRiskTopDead]
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_InventoryRiskTopDeadId]  DEFAULT ('') FOR [InventoryRiskTopDeadId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_BrgId]  DEFAULT ('') FOR [BrgId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_BrgCode]  DEFAULT ('') FOR [BrgCode]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_BrgName]  DEFAULT ('') FOR [BrgName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_KategoriName]  DEFAULT ('') FOR [KategoriName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_SupplierName]  DEFAULT ('') FOR [SupplierName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_Qty]  DEFAULT ((0)) FOR [Qty]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_InventoryValue]  DEFAULT ((0)) FOR [InventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopDead] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopDead_DaysSinceLastFaktur]  DEFAULT ((0)) FOR [DaysSinceLastFaktur]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_InventoryRiskTopSlow
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_InventoryRiskTopSlow](
	[InventoryRiskTopSlowId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[BrgId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BrgName] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[KategoriName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SupplierName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Qty] [int] NOT NULL,
	[InventoryValue] [decimal](18, 2) NOT NULL,
	[DaysSinceLastFaktur] [int] NOT NULL,
	[PercentOfAtRisk] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_InventoryRiskTopSlow] PRIMARY KEY CLUSTERED 
(
	[InventoryRiskTopSlowId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE UNIQUE NONCLUSTERED INDEX [UX_BTRPD_InventoryRiskTopSlow_SnapshotKey_Rank] ON [dbo].[BTRPD_InventoryRiskTopSlow]
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_InventoryRiskTopSlowId]  DEFAULT ('') FOR [InventoryRiskTopSlowId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_BrgId]  DEFAULT ('') FOR [BrgId]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_BrgCode]  DEFAULT ('') FOR [BrgCode]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_BrgName]  DEFAULT ('') FOR [BrgName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_KategoriName]  DEFAULT ('') FOR [KategoriName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_SupplierName]  DEFAULT ('') FOR [SupplierName]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_Qty]  DEFAULT ((0)) FOR [Qty]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_InventoryValue]  DEFAULT ((0)) FOR [InventoryValue]

ALTER TABLE [dbo].[BTRPD_InventoryRiskTopSlow] ADD  CONSTRAINT [DF_BTRPD_InventoryRiskTopSlow_DaysSinceLastFaktur]  DEFAULT ((0)) FOR [DaysSinceLastFaktur]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationAttention
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationAttention](
	[LocationAttentionId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityType] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityCode] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[EntityName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalKey] [varchar](40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ValueAmount] [decimal](18, 2) NULL,
	[ValueText] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_LocationAttention] PRIMARY KEY CLUSTERED 
(
	[LocationAttentionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_LocationAttention_SnapshotKey_SortOrder] ON [dbo].[BTRPD_LocationAttention]
(
	[SnapshotKey] ASC,
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_LocationAttentionId]  DEFAULT ('') FOR [LocationAttentionId]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_EntityType]  DEFAULT ('') FOR [EntityType]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_EntityName]  DEFAULT ('') FOR [EntityName]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_SignalKey]  DEFAULT ('') FOR [SignalKey]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_SignalLabel]  DEFAULT ('') FOR [SignalLabel]

ALTER TABLE [dbo].[BTRPD_LocationAttention] ADD  CONSTRAINT [DF_BTRPD_LocationAttention_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[Top1WarehouseInventoryPercent] [decimal](9, 4) NULL,
	[Top3WarehouseInventoryPercent] [decimal](9, 4) NULL,
	[Top1WarehouseAtRiskPercent] [decimal](9, 4) NULL,
	[Top1WarehouseSalesPercent] [decimal](9, 4) NULL,
	[Top1WilayahSalesPercent] [decimal](9, 4) NULL,
	[InactiveWarehouseWithStockCount] [int] NOT NULL,
	[WarehouseNoSalesWithInventoryCount] [int] NOT NULL,
	[TotalInventoryValue] [decimal](18, 2) NOT NULL,
	[TotalAtRiskValue] [decimal](18, 2) NOT NULL,
	[TotalOmzet] [decimal](18, 2) NOT NULL,
	[TotalPurchase] [decimal](18, 2) NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_LocationKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_InactiveWarehouseWithStockCount]  DEFAULT ((0)) FOR [InactiveWarehouseWithStockCount]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_WarehouseNoSalesWithInventoryCount]  DEFAULT ((0)) FOR [WarehouseNoSalesWithInventoryCount]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_TotalInventoryValue]  DEFAULT ((0)) FOR [TotalInventoryValue]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_TotalAtRiskValue]  DEFAULT ((0)) FOR [TotalAtRiskValue]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_TotalOmzet]  DEFAULT ((0)) FOR [TotalOmzet]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_TotalPurchase]  DEFAULT ((0)) FOR [TotalPurchase]

ALTER TABLE [dbo].[BTRPD_LocationKpi] ADD  CONSTRAINT [DF_BTRPD_LocationKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationTopWarehouseAtRisk
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk](
	[LocationTopWarehouseAtRiskId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[WarehouseId] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WarehouseName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[AtRiskValue] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_BTRPD_LocationTopWarehouseAtRisk] PRIMARY KEY CLUSTERED 
(
	[LocationTopWarehouseAtRiskId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseAtRisk_LocationTopWarehouseAtRiskId]  DEFAULT ('') FOR [LocationTopWarehouseAtRiskId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseAtRisk_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseAtRisk_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseId]  DEFAULT ('') FOR [WarehouseId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseAtRisk_WarehouseName]  DEFAULT ('') FOR [WarehouseName]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseAtRisk] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseAtRisk_AtRiskValue]  DEFAULT ((0)) FOR [AtRiskValue]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationTopWarehouseInventory
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationTopWarehouseInventory](
	[LocationTopWarehouseInventoryId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[WarehouseId] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WarehouseName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[InventoryValue] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_BTRPD_LocationTopWarehouseInventory] PRIMARY KEY CLUSTERED 
(
	[LocationTopWarehouseInventoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_LocationTopWarehouseInventory_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseInventory] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseInventory_LocationTopWarehouseInventoryId]  DEFAULT ('') FOR [LocationTopWarehouseInventoryId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseInventory] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseInventory_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseInventory] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseInventory_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseInventory] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseInventory_WarehouseId]  DEFAULT ('') FOR [WarehouseId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseInventory] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseInventory_WarehouseName]  DEFAULT ('') FOR [WarehouseName]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseInventory] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseInventory_InventoryValue]  DEFAULT ((0)) FOR [InventoryValue]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationTopWarehousePurchasing
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing](
	[LocationTopWarehousePurchasingId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[WarehouseId] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WarehouseName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MtdPurchaseAmount] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_BTRPD_LocationTopWarehousePurchasing] PRIMARY KEY CLUSTERED 
(
	[LocationTopWarehousePurchasingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_LocationTopWarehousePurchasing_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehousePurchasing_LocationTopWarehousePurchasingId]  DEFAULT ('') FOR [LocationTopWarehousePurchasingId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehousePurchasing_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehousePurchasing_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehousePurchasing_WarehouseId]  DEFAULT ('') FOR [WarehouseId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehousePurchasing_WarehouseName]  DEFAULT ('') FOR [WarehouseName]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehousePurchasing] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehousePurchasing_MtdPurchaseAmount]  DEFAULT ((0)) FOR [MtdPurchaseAmount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationTopWarehouseSales
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationTopWarehouseSales](
	[LocationTopWarehouseSalesId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[WarehouseId] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WarehouseName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MtdOmzet] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_BTRPD_LocationTopWarehouseSales] PRIMARY KEY CLUSTERED 
(
	[LocationTopWarehouseSalesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_LocationTopWarehouseSales_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseSales_LocationTopWarehouseSalesId]  DEFAULT ('') FOR [LocationTopWarehouseSalesId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseSales_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseSales_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseSales_WarehouseId]  DEFAULT ('') FOR [WarehouseId]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseSales_WarehouseName]  DEFAULT ('') FOR [WarehouseName]

ALTER TABLE [dbo].[BTRPD_LocationTopWarehouseSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWarehouseSales_MtdOmzet]  DEFAULT ((0)) FOR [MtdOmzet]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_LocationTopWilayahSales
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_LocationTopWilayahSales](
	[LocationTopWilayahSalesId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[WilayahId] [varchar](5) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WilayahName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MtdOmzet] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[DashboardRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_BTRPD_LocationTopWilayahSales] PRIMARY KEY CLUSTERED 
(
	[LocationTopWilayahSalesId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_LocationTopWilayahSales_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_LocationTopWilayahSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWilayahSales_LocationTopWilayahSalesId]  DEFAULT ('') FOR [LocationTopWilayahSalesId]

ALTER TABLE [dbo].[BTRPD_LocationTopWilayahSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWilayahSales_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_LocationTopWilayahSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWilayahSales_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_LocationTopWilayahSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWilayahSales_WilayahName]  DEFAULT ('') FOR [WilayahName]

ALTER TABLE [dbo].[BTRPD_LocationTopWilayahSales] ADD  CONSTRAINT [DF_BTRPD_LocationTopWilayahSales_MtdOmzet]  DEFAULT ((0)) FOR [MtdOmzet]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PiutangAging
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PiutangAging](
	[PiutangAgingId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BucketKey] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[BucketLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SortOrder] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_PiutangAging] PRIMARY KEY CLUSTERED 
(
	[PiutangAgingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_PiutangAging_SnapshotKey_BucketKey] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[BucketKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PiutangAging] ADD  CONSTRAINT [DF_BTRPD_PiutangAging_PiutangAgingId]  DEFAULT ('') FOR [PiutangAgingId]

ALTER TABLE [dbo].[BTRPD_PiutangAging] ADD  CONSTRAINT [DF_BTRPD_PiutangAging_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PiutangAging] ADD  CONSTRAINT [DF_BTRPD_PiutangAging_BucketKey]  DEFAULT ('') FOR [BucketKey]

ALTER TABLE [dbo].[BTRPD_PiutangAging] ADD  CONSTRAINT [DF_BTRPD_PiutangAging_BucketLabel]  DEFAULT ('') FOR [BucketLabel]

ALTER TABLE [dbo].[BTRPD_PiutangAging] ADD  CONSTRAINT [DF_BTRPD_PiutangAging_SortOrder]  DEFAULT ((0)) FOR [SortOrder]

ALTER TABLE [dbo].[BTRPD_PiutangAging] ADD  CONSTRAINT [DF_BTRPD_PiutangAging_Amount]  DEFAULT ((0)) FOR [Amount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PiutangCustomerAging
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PiutangCustomerAging](
	[PiutangCustomerAgingId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CurrentAmount] [decimal](18, 2) NOT NULL,
	[Aging30Amount] [decimal](18, 2) NOT NULL,
	[Aging60Amount] [decimal](18, 2) NOT NULL,
	[Aging90Amount] [decimal](18, 2) NOT NULL,
	[AgingOver90Amount] [decimal](18, 2) NOT NULL,
	[LastUpdate] [datetime] NOT NULL,
 CONSTRAINT [PK_BTRPD_PiutangCustomerAging] PRIMARY KEY CLUSTERED 
(
	[PiutangCustomerAgingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_PiutangCustomerAging_SnapshotKey_CustomerId] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[CustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_PiutangCustomerAging_SnapshotKey] ON [dbo].[BTRPD_PiutangCustomerAging]
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_PiutangCustomerAgingId]  DEFAULT ('') FOR [PiutangCustomerAgingId]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_CustomerId]  DEFAULT ('') FOR [CustomerId]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_CustomerCode]  DEFAULT ('') FOR [CustomerCode]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_CurrentAmount]  DEFAULT ((0)) FOR [CurrentAmount]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_Aging30Amount]  DEFAULT ((0)) FOR [Aging30Amount]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_Aging60Amount]  DEFAULT ((0)) FOR [Aging60Amount]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_Aging90Amount]  DEFAULT ((0)) FOR [Aging90Amount]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_AgingOver90Amount]  DEFAULT ((0)) FOR [AgingOver90Amount]

ALTER TABLE [dbo].[BTRPD_PiutangCustomerAging] ADD  CONSTRAINT [DF_BTRPD_PiutangCustomerAging_LastUpdate]  DEFAULT ('3000-01-01') FOR [LastUpdate]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PiutangKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PiutangKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[TotalPiutang] [decimal](18, 2) NOT NULL,
	[TotalCustomer] [int] NOT NULL,
	[OverdueCustomer] [int] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OverduePiutang] [decimal](18, 2) NOT NULL,
	[AgingOver90Amount] [decimal](18, 2) NOT NULL,
	[AgingOver90Percent] [decimal](9, 4) NULL,
	[Top10CustomerConcentrationPercent] [decimal](9, 4) NULL,
	[Top20CustomerConcentrationPercent] [decimal](9, 4) NULL,
 CONSTRAINT [PK_BTRPD_PiutangKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_TotalPiutang]  DEFAULT ((0)) FOR [TotalPiutang]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_TotalCustomer]  DEFAULT ((0)) FOR [TotalCustomer]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_OverdueCustomer]  DEFAULT ((0)) FOR [OverdueCustomer]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_OverduePiutang]  DEFAULT ((0)) FOR [OverduePiutang]

ALTER TABLE [dbo].[BTRPD_PiutangKpi] ADD  CONSTRAINT [DF_BTRPD_PiutangKpi_AgingOver90Amount]  DEFAULT ((0)) FOR [AgingOver90Amount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PiutangTopCustomer
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PiutangTopCustomer](
	[PiutangTopCustomerId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OutstandingBalance] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_PiutangTopCustomer] PRIMARY KEY CLUSTERED 
(
	[PiutangTopCustomerId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_PiutangTopCustomer_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomer] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomer_PiutangTopCustomerId]  DEFAULT ('') FOR [PiutangTopCustomerId]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomer] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomer_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomer] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomer_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomer] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomer_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomer] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomer_OutstandingBalance]  DEFAULT ((0)) FOR [OutstandingBalance]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PiutangTopCustomerRisk
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PiutangTopCustomerRisk](
	[PiutangTopCustomerRiskId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[CustomerId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CustomerName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TotalPiutang] [decimal](18, 2) NOT NULL,
	[CurrentAmount] [decimal](18, 2) NOT NULL,
	[Aging30Amount] [decimal](18, 2) NOT NULL,
	[Aging60Amount] [decimal](18, 2) NOT NULL,
	[Aging90Amount] [decimal](18, 2) NOT NULL,
	[AgingOver90Amount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_PiutangTopCustomerRisk] PRIMARY KEY CLUSTERED 
(
	[PiutangTopCustomerRiskId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_PiutangTopCustomerRisk_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_PiutangTopCustomerRiskId]  DEFAULT ('') FOR [PiutangTopCustomerRiskId]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_CustomerId]  DEFAULT ('') FOR [CustomerId]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_CustomerCode]  DEFAULT ('') FOR [CustomerCode]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_CustomerName]  DEFAULT ('') FOR [CustomerName]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_TotalPiutang]  DEFAULT ((0)) FOR [TotalPiutang]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_CurrentAmount]  DEFAULT ((0)) FOR [CurrentAmount]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_Aging30Amount]  DEFAULT ((0)) FOR [Aging30Amount]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_Aging60Amount]  DEFAULT ((0)) FOR [Aging60Amount]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_Aging90Amount]  DEFAULT ((0)) FOR [Aging90Amount]

ALTER TABLE [dbo].[BTRPD_PiutangTopCustomerRisk] ADD  CONSTRAINT [DF_BTRPD_PiutangTopCustomerRisk_AgingOver90Amount]  DEFAULT ((0)) FOR [AgingOver90Amount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[GrandTotalPurchase] [decimal](18, 2) NOT NULL,
	[TotalInvoice] [int] NOT NULL,
	[PendingPostingInvoiceCount] [int] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_GrandTotalPurchase]  DEFAULT ((0)) FOR [GrandTotalPurchase]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_TotalInvoice]  DEFAULT ((0)) FOR [TotalInvoice]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_PendingPostingInvoiceCount]  DEFAULT ((0)) FOR [PendingPostingInvoiceCount]

ALTER TABLE [dbo].[BTRPD_PurchasingKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingManagementAttention
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingManagementAttention](
	[PurchasingManagementAttentionId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityType] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[EntityName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalKey] [varchar](40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ValueAmount] [decimal](18, 2) NULL,
	[ValueText] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingManagementAttention] PRIMARY KEY CLUSTERED 
(
	[PurchasingManagementAttentionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_PurchasingManagementAttention_SnapshotKey_SortOrder] ON [dbo].[BTRPD_PurchasingManagementAttention]
(
	[SnapshotKey] ASC,
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_PurchasingManagementAttentionId]  DEFAULT ('') FOR [PurchasingManagementAttentionId]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_EntityType]  DEFAULT ('') FOR [EntityType]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_EntityName]  DEFAULT ('') FOR [EntityName]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_SignalKey]  DEFAULT ('') FOR [SignalKey]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_SignalLabel]  DEFAULT ('') FOR [SignalLabel]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementAttention] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementAttention_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingManagementKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingManagementKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[QualifiedBacklogCount] [int] NOT NULL,
	[QualifiedBacklogValue] [decimal](18, 2) NOT NULL,
	[PendingPostingValue] [decimal](18, 2) NOT NULL,
	[PostedPercent] [decimal](9, 4) NULL,
	[Top1PrincipalPercent] [decimal](9, 4) NULL,
	[Top3PrincipalPercent] [decimal](9, 4) NULL,
	[Top1SupplierInventoryPercent] [decimal](9, 4) NULL,
	[CompoundDependencyCount] [int] NOT NULL,
	[PrincipalInventoryNoPurchaseCount] [int] NOT NULL,
	[UnknownPrincipalCount] [int] NOT NULL,
	[PurchasingInactivityFlag] [bit] NOT NULL,
	[QualifiedBacklogPrincipalCount] [int] NOT NULL,
	[PrincipalAtRiskExposureCount] [int] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingManagementKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogCount]  DEFAULT ((0)) FOR [QualifiedBacklogCount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogValue]  DEFAULT ((0)) FOR [QualifiedBacklogValue]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_PendingPostingValue]  DEFAULT ((0)) FOR [PendingPostingValue]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_CompoundDependencyCount]  DEFAULT ((0)) FOR [CompoundDependencyCount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_PrincipalInventoryNoPurchaseCount]  DEFAULT ((0)) FOR [PrincipalInventoryNoPurchaseCount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_UnknownPrincipalCount]  DEFAULT ((0)) FOR [UnknownPrincipalCount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_PurchasingInactivityFlag]  DEFAULT ((0)) FOR [PurchasingInactivityFlag]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_QualifiedBacklogPrincipalCount]  DEFAULT ((0)) FOR [QualifiedBacklogPrincipalCount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_PrincipalAtRiskExposureCount]  DEFAULT ((0)) FOR [PrincipalAtRiskExposureCount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementKpi] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingManagementTopPrincipal
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal](
	[PurchasingManagementTopPrincipalId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[PrincipalName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[MtdPurchaseAmount] [decimal](18, 2) NOT NULL,
	[PercentOfPurchase] [decimal](9, 4) NULL,
	[InventoryValue] [decimal](18, 2) NULL,
	[PercentOfInventory] [decimal](9, 4) NULL,
	[AtRiskValue] [decimal](18, 2) NULL,
	[PercentOfAtRisk] [decimal](9, 4) NULL,
	[IsCompoundDependency] [bit] NOT NULL,
	[IsInventoryNoPurchase] [bit] NOT NULL,
	[ReportRoute] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingManagementTopPrincipal] PRIMARY KEY CLUSTERED 
(
	[PurchasingManagementTopPrincipalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE UNIQUE NONCLUSTERED INDEX [UX_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey_Rank] ON [dbo].[BTRPD_PurchasingManagementTopPrincipal]
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_PurchasingManagementTopPrincipalId]  DEFAULT ('') FOR [PurchasingManagementTopPrincipalId]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_PrincipalName]  DEFAULT ('') FOR [PrincipalName]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_MtdPurchaseAmount]  DEFAULT ((0)) FOR [MtdPurchaseAmount]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_IsCompoundDependency]  DEFAULT ((0)) FOR [IsCompoundDependency]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_IsInventoryNoPurchase]  DEFAULT ((0)) FOR [IsInventoryNoPurchase]

ALTER TABLE [dbo].[BTRPD_PurchasingManagementTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingManagementTopPrincipal_ReportRoute]  DEFAULT ('') FOR [ReportRoute]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingPostingStatus
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingPostingStatus](
	[PurchasingPostingStatusId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[StatusKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[StatusLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SortOrder] [int] NOT NULL,
	[PurchaseAmount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingPostingStatus] PRIMARY KEY CLUSTERED 
(
	[PurchasingPostingStatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_PurchasingPostingStatus_SnapshotKey_StatusKey] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[StatusKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_PurchasingPostingStatus] ADD  CONSTRAINT [DF_BTRPD_PurchasingPostingStatus_PurchasingPostingStatusId]  DEFAULT ('') FOR [PurchasingPostingStatusId]

ALTER TABLE [dbo].[BTRPD_PurchasingPostingStatus] ADD  CONSTRAINT [DF_BTRPD_PurchasingPostingStatus_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingPostingStatus] ADD  CONSTRAINT [DF_BTRPD_PurchasingPostingStatus_StatusKey]  DEFAULT ('') FOR [StatusKey]

ALTER TABLE [dbo].[BTRPD_PurchasingPostingStatus] ADD  CONSTRAINT [DF_BTRPD_PurchasingPostingStatus_StatusLabel]  DEFAULT ('') FOR [StatusLabel]

ALTER TABLE [dbo].[BTRPD_PurchasingPostingStatus] ADD  CONSTRAINT [DF_BTRPD_PurchasingPostingStatus_SortOrder]  DEFAULT ((0)) FOR [SortOrder]

ALTER TABLE [dbo].[BTRPD_PurchasingPostingStatus] ADD  CONSTRAINT [DF_BTRPD_PurchasingPostingStatus_PurchaseAmount]  DEFAULT ((0)) FOR [PurchaseAmount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingTopPrincipal
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingTopPrincipal](
	[PurchasingTopPrincipalId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[PrincipalName] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PurchaseAmount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingTopPrincipal] PRIMARY KEY CLUSTERED 
(
	[PurchasingTopPrincipalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_PurchasingTopPrincipal_SnapshotKey_Rank] ON [dbo].[BTRPD_PurchasingTopPrincipal]
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_PurchasingTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingTopPrincipal_PurchasingTopPrincipalId]  DEFAULT ('') FOR [PurchasingTopPrincipalId]

ALTER TABLE [dbo].[BTRPD_PurchasingTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingTopPrincipal_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingTopPrincipal_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_PurchasingTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingTopPrincipal_PrincipalName]  DEFAULT ('') FOR [PrincipalName]

ALTER TABLE [dbo].[BTRPD_PurchasingTopPrincipal] ADD  CONSTRAINT [DF_BTRPD_PurchasingTopPrincipal_PurchaseAmount]  DEFAULT ((0)) FOR [PurchaseAmount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_PurchasingWeekTrend
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_PurchasingWeekTrend](
	[PurchasingWeekTrendId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WeekStart] [datetime] NOT NULL,
	[WeekEnd] [datetime] NOT NULL,
	[WeekLabel] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PurchaseAmount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_PurchasingWeekTrend] PRIMARY KEY CLUSTERED 
(
	[PurchasingWeekTrendId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_PurchasingWeekTrend_SnapshotKey_WeekStart] ON [dbo].[BTRPD_PurchasingWeekTrend]
(
	[SnapshotKey] ASC,
	[WeekStart] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_PurchasingWeekTrend] ADD  CONSTRAINT [DF_BTRPD_PurchasingWeekTrend_PurchasingWeekTrendId]  DEFAULT ('') FOR [PurchasingWeekTrendId]

ALTER TABLE [dbo].[BTRPD_PurchasingWeekTrend] ADD  CONSTRAINT [DF_BTRPD_PurchasingWeekTrend_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_PurchasingWeekTrend] ADD  CONSTRAINT [DF_BTRPD_PurchasingWeekTrend_WeekStart]  DEFAULT ('3000-01-01') FOR [WeekStart]

ALTER TABLE [dbo].[BTRPD_PurchasingWeekTrend] ADD  CONSTRAINT [DF_BTRPD_PurchasingWeekTrend_WeekEnd]  DEFAULT ('3000-01-01') FOR [WeekEnd]

ALTER TABLE [dbo].[BTRPD_PurchasingWeekTrend] ADD  CONSTRAINT [DF_BTRPD_PurchasingWeekTrend_WeekLabel]  DEFAULT ('') FOR [WeekLabel]

ALTER TABLE [dbo].[BTRPD_PurchasingWeekTrend] ADD  CONSTRAINT [DF_BTRPD_PurchasingWeekTrend_PurchaseAmount]  DEFAULT ((0)) FOR [PurchaseAmount]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_RefreshLog
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_RefreshLog](
	[RefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Domain] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[StartedAt] [datetime] NOT NULL,
	[CompletedAt] [datetime] NULL,
	[Status] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[DurationMs] [int] NOT NULL,
	[ErrorMessage] [varchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TriggeredBy] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_RefreshLog] PRIMARY KEY CLUSTERED 
(
	[RefreshLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_RefreshLog_Domain_CompletedAt] ON [dbo].[BTRPD_RefreshLog]
(
	[Domain] ASC,
	[CompletedAt] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_RefreshLogId]  DEFAULT ('') FOR [RefreshLogId]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_Domain]  DEFAULT ('') FOR [Domain]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_StartedAt]  DEFAULT ('3000-01-01') FOR [StartedAt]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_Status]  DEFAULT ('') FOR [Status]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_DurationMs]  DEFAULT ((0)) FOR [DurationMs]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_ErrorMessage]  DEFAULT ('') FOR [ErrorMessage]

ALTER TABLE [dbo].[BTRPD_RefreshLog] ADD  CONSTRAINT [DF_BTRPD_RefreshLog_TriggeredBy]  DEFAULT ('') FOR [TriggeredBy]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[TotalOmzet] [decimal](18, 2) NOT NULL,
	[TotalFaktur] [int] NOT NULL,
	[TotalCustomer] [int] NOT NULL,
	[TotalTarget] [decimal](18, 2) NOT NULL,
	[TotalAchievement] [decimal](18, 2) NOT NULL,
	[AchievementPercent] [decimal](9, 4) NULL,
	[CompletedOmzet] [decimal](18, 2) NOT NULL,
	[PipelineOmzet] [decimal](18, 2) NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_TotalOmzet]  DEFAULT ((0)) FOR [TotalOmzet]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_TotalFaktur]  DEFAULT ((0)) FOR [TotalFaktur]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_TotalCustomer]  DEFAULT ((0)) FOR [TotalCustomer]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_TotalTarget]  DEFAULT ((0)) FOR [TotalTarget]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_TotalAchievement]  DEFAULT ((0)) FOR [TotalAchievement]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_CompletedOmzet]  DEFAULT ((0)) FOR [CompletedOmzet]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_PipelineOmzet]  DEFAULT ((0)) FOR [PipelineOmzet]

ALTER TABLE [dbo].[BTRPD_SalesKpi] ADD  CONSTRAINT [DF_BTRPD_SalesKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanAttention
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanAttention](
	[SalesmanAttentionId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalKey] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SignalLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ValueAmount] [decimal](18, 2) NULL,
	[ValueText] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[WilayahName] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SortOrder] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanAttention] PRIMARY KEY CLUSTERED 
(
	[SalesmanAttentionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_SalesmanAttention_SnapshotKey_SortOrder] ON [dbo].[BTRPD_SalesmanAttention]
(
	[SnapshotKey] ASC,
	[SortOrder] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SalesmanAttentionId]  DEFAULT ('') FOR [SalesmanAttentionId]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SignalKey]  DEFAULT ('') FOR [SignalKey]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SignalLabel]  DEFAULT ('') FOR [SignalLabel]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_WilayahName]  DEFAULT ('') FOR [WilayahName]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_SortOrder]  DEFAULT ((0)) FOR [SortOrder]

ALTER TABLE [dbo].[BTRPD_SalesmanAttention] ADD  CONSTRAINT [DF_BTRPD_SalesmanAttention_IsActive]  DEFAULT ((0)) FOR [IsActive]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanKpi
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanKpi](
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[GeneratedAt] [datetime] NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[TotalTeamOmzet] [decimal](18, 2) NOT NULL,
	[TotalPiutang] [decimal](18, 2) NOT NULL,
	[ActiveSalesmanCount] [int] NOT NULL,
	[BelowTargetCount] [int] NOT NULL,
	[MissingTargetSetupCount] [int] NOT NULL,
	[HighOverdueExposureCount] [int] NOT NULL,
	[HighPiutangExposureCount] [int] NOT NULL,
	[CustomerConcentrationCount] [int] NOT NULL,
	[DormantPortfolioCount] [int] NOT NULL,
	[TopOmzetSalesmanPercent] [decimal](9, 4) NULL,
	[TopPiutangSalesmanPercent] [decimal](9, 4) NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanKpi] PRIMARY KEY CLUSTERED 
(
	[SnapshotKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_GeneratedAt]  DEFAULT ('3000-01-01') FOR [GeneratedAt]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_TotalTeamOmzet]  DEFAULT ((0)) FOR [TotalTeamOmzet]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_TotalPiutang]  DEFAULT ((0)) FOR [TotalPiutang]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_ActiveSalesmanCount]  DEFAULT ((0)) FOR [ActiveSalesmanCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_BelowTargetCount]  DEFAULT ((0)) FOR [BelowTargetCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_NoTargetCount]  DEFAULT ((0)) FOR [MissingTargetSetupCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_HighOverdueExposureCount]  DEFAULT ((0)) FOR [HighOverdueExposureCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_HighPiutangExposureCount]  DEFAULT ((0)) FOR [HighPiutangExposureCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_CustomerConcentrationCount]  DEFAULT ((0)) FOR [CustomerConcentrationCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_DormantPortfolioCount]  DEFAULT ((0)) FOR [DormantPortfolioCount]

ALTER TABLE [dbo].[BTRPD_SalesmanKpi] ADD  CONSTRAINT [DF_BTRPD_SalesmanKpi_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanPrincipalAchievement
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement](
	[SalesmanPrincipalAchievementId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SupplierId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SupplierName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TargetAmount] [decimal](18, 2) NULL,
	[CompletedOmzet] [decimal](18, 2) NOT NULL,
	[AchievementPercent] [decimal](9, 4) NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanPrincipalAchievement] PRIMARY KEY CLUSTERED 
(
	[SalesmanPrincipalAchievementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId_SupplierId] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[SalesPersonId] ASC,
	[SupplierId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_SalesmanPrincipalAchievement_SnapshotKey_SalesPersonId] ON [dbo].[BTRPD_SalesmanPrincipalAchievement]
(
	[SnapshotKey] ASC,
	[SalesPersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SalesmanPrincipalAchievementId]  DEFAULT ('') FOR [SalesmanPrincipalAchievementId]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SupplierId]  DEFAULT ('') FOR [SupplierId]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SupplierName]  DEFAULT ('') FOR [SupplierName]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_CompletedOmzet]  DEFAULT ((0)) FOR [CompletedOmzet]

ALTER TABLE [dbo].[BTRPD_SalesmanPrincipalAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanPrincipalAchievement_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanRepHistory
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanRepHistory](
	[SalesmanRepHistoryId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[PeriodYear] [int] NOT NULL,
	[PeriodMonth] [int] NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TargetAmount] [decimal](18, 2) NULL,
	[CompletedOmzet] [decimal](18, 2) NOT NULL,
	[AchievementPercent] [decimal](9, 4) NULL,
	[AchievementBand] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[OpenBalance] [decimal](18, 2) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[LastRefreshLogId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanRepHistory] PRIMARY KEY CLUSTERED 
(
	[SalesmanRepHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesmanRepHistory_PeriodYear_PeriodMonth_SalesPersonId] UNIQUE NONCLUSTERED 
(
	[PeriodYear] ASC,
	[PeriodMonth] ASC,
	[SalesPersonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_SalesmanRepHistory_SalesPersonId] ON [dbo].[BTRPD_SalesmanRepHistory]
(
	[SalesPersonId] ASC,
	[PeriodYear] DESC,
	[PeriodMonth] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_SalesmanRepHistoryId]  DEFAULT ('') FOR [SalesmanRepHistoryId]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_PeriodYear]  DEFAULT ((0)) FOR [PeriodYear]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_PeriodMonth]  DEFAULT ((0)) FOR [PeriodMonth]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_CompletedOmzet]  DEFAULT ((0)) FOR [CompletedOmzet]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_OpenBalance]  DEFAULT ((0)) FOR [OpenBalance]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_IsActive]  DEFAULT ((0)) FOR [IsActive]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_LastRefreshLogId]  DEFAULT ('') FOR [LastRefreshLogId]

ALTER TABLE [dbo].[BTRPD_SalesmanRepHistory] ADD  CONSTRAINT [DF_BTRPD_SalesmanRepHistory_UpdatedAt]  DEFAULT ('3000-01-01') FOR [UpdatedAt]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanSegmentation
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanSegmentation](
	[SalesmanSegmentationId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SegmentType] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SegmentKey] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SegmentLabel] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesmanCount] [int] NOT NULL,
	[ActiveCount] [int] NOT NULL,
	[InactiveCount] [int] NOT NULL,
	[SortOrder] [int] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanSegmentation] PRIMARY KEY CLUSTERED 
(
	[SalesmanSegmentationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesmanSegmentation_SnapshotKey_SegmentType_SegmentKey] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[SegmentType] ASC,
	[SegmentKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SalesmanSegmentationId]  DEFAULT ('') FOR [SalesmanSegmentationId]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SegmentType]  DEFAULT ('') FOR [SegmentType]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SegmentKey]  DEFAULT ('') FOR [SegmentKey]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SegmentLabel]  DEFAULT ('') FOR [SegmentLabel]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SalesmanCount]  DEFAULT ((0)) FOR [SalesmanCount]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_ActiveCount]  DEFAULT ((0)) FOR [ActiveCount]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_InactiveCount]  DEFAULT ((0)) FOR [InactiveCount]

ALTER TABLE [dbo].[BTRPD_SalesmanSegmentation] ADD  CONSTRAINT [DF_BTRPD_SalesmanSegmentation_SortOrder]  DEFAULT ((0)) FOR [SortOrder]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanTopAchievement
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanTopAchievement](
	[SalesmanTopAchievementId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[TargetAmount] [decimal](18, 2) NULL,
	[CompletedOmzet] [decimal](18, 2) NOT NULL,
	[AchievementPercent] [decimal](9, 4) NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanTopAchievement] PRIMARY KEY CLUSTERED 
(
	[SalesmanTopAchievementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesmanTopAchievement_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_SalesmanTopAchievementId]  DEFAULT ('') FOR [SalesmanTopAchievementId]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_CompletedOmzet]  DEFAULT ((0)) FOR [CompletedOmzet]

ALTER TABLE [dbo].[BTRPD_SalesmanTopAchievement] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopAchievement_IsActive]  DEFAULT ((0)) FOR [IsActive]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanTopOmzet
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanTopOmzet](
	[SalesmanTopOmzetId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CompletedOmzet] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanTopOmzet] PRIMARY KEY CLUSTERED 
(
	[SalesmanTopOmzetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesmanTopOmzet_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_SalesmanTopOmzetId]  DEFAULT ('') FOR [SalesmanTopOmzetId]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_CompletedOmzet]  DEFAULT ((0)) FOR [CompletedOmzet]

ALTER TABLE [dbo].[BTRPD_SalesmanTopOmzet] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopOmzet_IsActive]  DEFAULT ((0)) FOR [IsActive]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesmanTopPiutang
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesmanTopPiutang](
	[SalesmanTopPiutangId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[SalesPersonId] [varchar](13) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonCode] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SalesPersonName] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[OutstandingBalance] [decimal](18, 2) NOT NULL,
	[PercentOfTotal] [decimal](9, 4) NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesmanTopPiutang] PRIMARY KEY CLUSTERED 
(
	[SalesmanTopPiutangId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesmanTopPiutang_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_SalesmanTopPiutangId]  DEFAULT ('') FOR [SalesmanTopPiutangId]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_SalesPersonId]  DEFAULT ('') FOR [SalesPersonId]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_SalesPersonCode]  DEFAULT ('') FOR [SalesPersonCode]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_OutstandingBalance]  DEFAULT ((0)) FOR [OutstandingBalance]

ALTER TABLE [dbo].[BTRPD_SalesmanTopPiutang] ADD  CONSTRAINT [DF_BTRPD_SalesmanTopPiutang_IsActive]  DEFAULT ((0)) FOR [IsActive]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesTopSalesman
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesTopSalesman](
	[SalesTopSalesmanId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Rank] [int] NOT NULL,
	[SalesPersonName] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[CompletedOmzet] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesTopSalesman] PRIMARY KEY CLUSTERED 
(
	[SalesTopSalesmanId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_BTRPD_SalesTopSalesman_SnapshotKey_Rank] UNIQUE NONCLUSTERED 
(
	[SnapshotKey] ASC,
	[Rank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

ALTER TABLE [dbo].[BTRPD_SalesTopSalesman] ADD  CONSTRAINT [DF_BTRPD_SalesTopSalesman_SalesTopSalesmanId]  DEFAULT ('') FOR [SalesTopSalesmanId]

ALTER TABLE [dbo].[BTRPD_SalesTopSalesman] ADD  CONSTRAINT [DF_BTRPD_SalesTopSalesman_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesTopSalesman] ADD  CONSTRAINT [DF_BTRPD_SalesTopSalesman_Rank]  DEFAULT ((0)) FOR [Rank]

ALTER TABLE [dbo].[BTRPD_SalesTopSalesman] ADD  CONSTRAINT [DF_BTRPD_SalesTopSalesman_SalesPersonName]  DEFAULT ('') FOR [SalesPersonName]

ALTER TABLE [dbo].[BTRPD_SalesTopSalesman] ADD  CONSTRAINT [DF_BTRPD_SalesTopSalesman_CompletedOmzet]  DEFAULT ((0)) FOR [CompletedOmzet]


------------------------------------------------------------------------
-- Table: dbo.BTRPD_SalesWeekTrend
------------------------------------------------------------------------

SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[BTRPD_SalesWeekTrend](
	[SalesWeekTrendId] [varchar](26) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[SnapshotKey] [varchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[WeekStart] [datetime] NOT NULL,
	[WeekEnd] [datetime] NOT NULL,
	[WeekLabel] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[RecognizedAmount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_BTRPD_SalesWeekTrend] PRIMARY KEY CLUSTERED 
(
	[SalesWeekTrendId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


CREATE NONCLUSTERED INDEX [IX_BTRPD_SalesWeekTrend_SnapshotKey_WeekStart] ON [dbo].[BTRPD_SalesWeekTrend]
(
	[SnapshotKey] ASC,
	[WeekStart] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[BTRPD_SalesWeekTrend] ADD  CONSTRAINT [DF_BTRPD_SalesWeekTrend_SalesWeekTrendId]  DEFAULT ('') FOR [SalesWeekTrendId]

ALTER TABLE [dbo].[BTRPD_SalesWeekTrend] ADD  CONSTRAINT [DF_BTRPD_SalesWeekTrend_SnapshotKey]  DEFAULT ('CURRENT') FOR [SnapshotKey]

ALTER TABLE [dbo].[BTRPD_SalesWeekTrend] ADD  CONSTRAINT [DF_BTRPD_SalesWeekTrend_WeekStart]  DEFAULT ('3000-01-01') FOR [WeekStart]

ALTER TABLE [dbo].[BTRPD_SalesWeekTrend] ADD  CONSTRAINT [DF_BTRPD_SalesWeekTrend_WeekEnd]  DEFAULT ('3000-01-01') FOR [WeekEnd]

ALTER TABLE [dbo].[BTRPD_SalesWeekTrend] ADD  CONSTRAINT [DF_BTRPD_SalesWeekTrend_WeekLabel]  DEFAULT ('') FOR [WeekLabel]

ALTER TABLE [dbo].[BTRPD_SalesWeekTrend] ADD  CONSTRAINT [DF_BTRPD_SalesWeekTrend_RecognizedAmount]  DEFAULT ((0)) FOR [RecognizedAmount]


